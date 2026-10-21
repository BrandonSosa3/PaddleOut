using System.Collections;
using UnityEngine;

/// <summary>
/// Tracks the surfer's lives (shown as surfboards) and the Bubble Shield.
/// Touching a hazard costs one life, unless the shield is up, in which case
/// the shield pops instead. After a hit, the surfer blinks and is briefly
/// invincible so a single wave can't hit them twice. At zero lives, the run ends.
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public class SurferHealth : MonoBehaviour
{
    [Header("Lives")]
    [Tooltip("How many hits the surfer can take before game over.")]
    [SerializeField] private int maxLives = 3;

    [Header("After Getting Hit")]
    [Tooltip("Seconds the surfer can't be hurt again after a hit.")]
    [SerializeField] private float invincibleTime = 1.5f;
    [Tooltip("How fast the surfer blinks while invincible.")]
    [SerializeField] private float blinkInterval = 0.1f;

    [Header("Bubble Shield")]
    [Tooltip("The bubble drawn around the surfer while shielded. Starts hidden.")]
    [SerializeField] private GameObject shieldVisual;
    [Tooltip("Short safe window after the shield pops, so the same hazard can't hit right after.")]
    [SerializeField] private float shieldPopGrace = 0.75f;

    /// <summary>Lives remaining.</summary>
    public int Lives { get; private set; }

    /// <summary>True while the Bubble Shield is active.</summary>
    public bool HasShield { get; private set; }

    /// <summary>Fires whenever lives change (used to update the life icons).</summary>
    public event System.Action<int> LivesChanged;

    /// <summary>Fires when the shield blocks a hit (used later for the pop sound).</summary>
    public event System.Action ShieldBlocked;

    private SpriteRenderer spriteRenderer;
    private float invincibleUntil;

    private bool IsInvincible => Time.time < invincibleUntil;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        Lives = maxLives;
    }

    private void Start()
    {
        LivesChanged?.Invoke(Lives);
        SetShieldVisual(false);
    }

    /// <summary>Unity calls this when the surfer's collider overlaps a trigger collider.</summary>
    private void OnTriggerEnter2D(Collider2D other)
    {
        Hazard hazard = other.GetComponent<Hazard>();
        if (hazard == null || IsInvincible || Lives <= 0)
        {
            return;
        }

        if (HasShield)
        {
            PopShield();
        }
        else
        {
            TakeHit();
        }

        hazard.OnHitSurfer();
    }

    /// <summary>Turns on the Bubble Shield. Called by the shield pickup.</summary>
    public void GiveShield()
    {
        HasShield = true;
        SetShieldVisual(true);
    }

    /// <summary>The shield absorbs a hit instead of the surfer losing a life.</summary>
    private void PopShield()
    {
        HasShield = false;
        SetShieldVisual(false);
        invincibleUntil = Time.time + shieldPopGrace;
        Debug.Log("Shield blocked a hit!");
        ShieldBlocked?.Invoke();
    }

    /// <summary>Removes a life, then either ends the game or starts invincibility.</summary>
    private void TakeHit()
    {
        Lives--;
        Debug.Log($"Hit! Lives left: {Lives}");
        LivesChanged?.Invoke(Lives);

        if (Lives <= 0)
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.EndGame();
            }
            return;
        }

        invincibleUntil = Time.time + invincibleTime;
        StartCoroutine(Blink());
    }

    /// <summary>Flickers the surfer on and off while invincible.</summary>
    private IEnumerator Blink()
    {
        while (IsInvincible)
        {
            spriteRenderer.enabled = !spriteRenderer.enabled;
            yield return new WaitForSeconds(blinkInterval);
        }
        spriteRenderer.enabled = true;
    }

    private void SetShieldVisual(bool visible)
    {
        if (shieldVisual != null)
        {
            shieldVisual.SetActive(visible);
        }
    }
}