using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Tracks how much breath the surfer has left.
/// Breath drains while underwater and refills at the surface.
/// If it runs out, the surfer is forced up and can't dive again
/// until breath refills past a set amount.
/// </summary>
[RequireComponent(typeof(SurferController))]
public class BreathMeter : MonoBehaviour
{
    [Header("Breath Settings")]
    [Tooltip("Total amount of breath.")]
    [SerializeField] private float maxBreath = 100f;
    [Tooltip("Breath lost per second underwater. 25 = about 4 seconds from full to empty.")]
    [SerializeField] private float drainPerSecond = 25f;
    [Tooltip("Breath regained per second at the surface.")]
    [SerializeField] private float refillPerSecond = 40f;
    [Tooltip("After running out, breath must refill to this fraction before diving again.")]
    [Range(0f, 1f)]
    [SerializeField] private float recoverFraction = 0.4f;

    [Header("UI (optional)")]
    [Tooltip("The bar image that shrinks as breath runs out.")]
    [SerializeField] private Image breathFill;
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color lowColor = Color.red;
    [SerializeField] private Color exhaustedColor = Color.gray;
    [Tooltip("Below this fraction the bar turns the low color.")]
    [Range(0f, 1f)]
    [SerializeField] private float lowFraction = 0.3f;

    private SurferController surfer;
    private float currentBreath;
    private bool isExhausted;

    /// <summary>False while the surfer is recovering from running out of breath.</summary>
    public bool CanDive => !isExhausted;

    /// <summary>Breath left from 0 (empty) to 1 (full).</summary>
    public float BreathFraction => currentBreath / maxBreath;

    private void Awake()
    {
        surfer = GetComponent<SurferController>();
        currentBreath = maxBreath;
    }

    private void Update()
    {
        // Drain underwater, refill at the surface.
        if (surfer.IsUnderwater)
        {
            currentBreath -= drainPerSecond * Time.deltaTime;
        }
        else
        {
            currentBreath += refillPerSecond * Time.deltaTime;
        }
        currentBreath = Mathf.Clamp(currentBreath, 0f, maxBreath);

        // Out of breath: lock diving until the surfer recovers enough.
        if (currentBreath <= 0f)
        {
            isExhausted = true;
        }
        else if (isExhausted && BreathFraction >= recoverFraction)
        {
            isExhausted = false;
        }

        UpdateBar();
    }

    /// <summary>Instantly fills breath back up. Used later by the Deep Breath power-up.</summary>
    public void RefillFull()
    {
        currentBreath = maxBreath;
        isExhausted = false;
    }

    /// <summary>Shrinks the bar and changes its color to match the current breath.</summary>
    private void UpdateBar()
    {
        if (breathFill == null)
        {
            return;
        }

        breathFill.rectTransform.localScale = new Vector3(BreathFraction, 1f, 1f);

        if (isExhausted)
        {
            breathFill.color = exhaustedColor;
        }
        else if (BreathFraction < lowFraction)
        {
            breathFill.color = lowColor;
        }
        else
        {
            breathFill.color = normalColor;
        }
    }
}