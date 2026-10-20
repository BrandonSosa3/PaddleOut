using System.Collections;
using UnityEngine;

/// <summary>
/// Tracks the surfer's lives (shown as surfboards).
/// Touching a hazard costs one life, then the surfer blinks and is
/// briefly invincible so a single wave can't hit them twice.
/// At zero lives, the run ends.
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

    /// <summary>Lives remaining.</summary>
    public int Lives { get; private set; }

    /// <summary>Fires whenever lives change (used later to update the surfboard icons).</summary>
    public event System.Action<int> LivesChanged;

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
    }

    /// <summary>Unity calls this when the surfer's collider overlaps a trigger collider.</summary>
    private void OnTriggerEnter2D(Collider2D other)
    {
        Hazard hazard = other.GetComponent<Hazard>();
        if (hazard == null || IsInvincible || Lives <= 0)
        {
            return;
        }

        TakeHit();
        hazard.OnHitSurfer();
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
}