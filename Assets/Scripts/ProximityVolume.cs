using UnityEngine;

/// <summary>
/// Makes a looping sound get louder as this object gets closer to the surfer.
/// Used on whitewash waves so you can hear a set building before it hits.
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class ProximityVolume : MonoBehaviour
{
    [Tooltip("Volume when the object is right on top of the surfer.")]
    [Range(0f, 1f)]
    [SerializeField] private float maxVolume = 0.6f;
    [Tooltip("At this distance (or farther) from the surfer, the sound is silent.")]
    [SerializeField] private float hearingRange = 14f;

    private AudioSource audioSource;
    private Transform surfer;

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();

        SurferController surferController = FindAnyObjectByType<SurferController>();
        if (surferController != null)
        {
            surfer = surferController.transform;
        }
    }

    private void Update()
    {
        if (surfer == null)
        {
            return;
        }

        // 0 when far away, 1 when right next to the surfer.
        float distance = Mathf.Abs(transform.position.x - surfer.position.x);
        float closeness = 1f - Mathf.Clamp01(distance / hearingRange);

        // Squaring makes the roar build up slowly, then swell as it gets close.
        audioSource.volume = maxVolume * closeness * closeness;
    }
}