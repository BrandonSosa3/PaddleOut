using UnityEngine;

/// <summary>
/// Something the surfer collects by touching it: a shell (points),
/// a Deep Breath (refills breath), or a Bubble Shield (blocks one hit).
/// </summary>
public class Pickup : MonoBehaviour
{
    public enum PickupType { Shell, DeepBreath, BubbleShield }

    [Tooltip("What this pickup does when collected.")]
    [SerializeField] private PickupType type = PickupType.Shell;

    [Header("Random Height")]
    [Tooltip("If on, spawns at a random height so the player has to dive to different depths to grab it.")]
    [SerializeField] private bool randomHeight = true;
    [SerializeField] private float minY = -3f;
    [SerializeField] private float maxY = 1f;

    /// <summary>Fires whenever any pickup is collected (used later to play sounds).</summary>
    public static event System.Action<PickupType> Collected;

    private void Start()
    {
        if (randomHeight)
        {
            Vector3 position = transform.position;
            position.y = Random.Range(minY, maxY);
            transform.position = position;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Only the surfer can collect pickups (it's the only thing with SurferHealth).
        SurferHealth surfer = other.GetComponent<SurferHealth>();
        if (surfer == null)
        {
            return;
        }

        // Ignore pickups after the run is over.
        if (GameManager.Instance != null && !GameManager.Instance.IsPlaying)
        {
            return;
        }

        ApplyEffect(surfer);
        Collected?.Invoke(type);
        Destroy(gameObject);
    }

    /// <summary>Does whatever this type of pickup is supposed to do.</summary>
    private void ApplyEffect(SurferHealth surfer)
    {
        switch (type)
        {
            case PickupType.Shell:
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.AddShell();
                }
                break;

            case PickupType.DeepBreath:
                BreathMeter breath = surfer.GetComponent<BreathMeter>();
                if (breath != null)
                {
                    breath.RefillFull();
                }
                break;

            case PickupType.BubbleShield:
                surfer.GiveShield();
                break;
        }
    }
}