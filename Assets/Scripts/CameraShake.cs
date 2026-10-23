using UnityEngine;

/// <summary>
/// Briefly shakes the camera when the surfer loses a life,
/// so getting hit feels like an impact.
/// </summary>
public class CameraShake : MonoBehaviour
{
    [SerializeField] private SurferHealth surferHealth;
    [Tooltip("How long the shake lasts, in seconds.")]
    [SerializeField] private float duration = 0.25f;
    [Tooltip("How far the camera jumps around during the shake.")]
    [SerializeField] private float strength = 0.2f;

    private Vector3 restPosition;
    private float shakeTimeLeft;
    private int lastLives = -1;

    private void Awake()
    {
        restPosition = transform.position;
    }

    private void OnEnable()
    {
        surferHealth.LivesChanged += OnLivesChanged;
    }

    private void OnDisable()
    {
        surferHealth.LivesChanged -= OnLivesChanged;
    }

    private void OnLivesChanged(int lives)
    {
        // Shake only when a life is lost, not when lives are first set.
        if (lastLives >= 0 && lives < lastLives)
        {
            shakeTimeLeft = duration;
        }
        lastLives = lives;
    }

    private void Update()
    {
        if (shakeTimeLeft > 0f)
        {
            shakeTimeLeft -= Time.deltaTime;
            float fade = shakeTimeLeft / duration; // shake gets weaker as it ends
            Vector2 offset = Random.insideUnitCircle * strength * fade;
            transform.position = restPosition + (Vector3)offset;
        }
        else
        {
            transform.position = restPosition;
        }
    }
}