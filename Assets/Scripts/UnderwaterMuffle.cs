using UnityEngine;

/// <summary>
/// Muffles ALL game audio while the surfer is underwater, then snaps it back
/// when they surface. Works by sliding the cutoff of a low pass filter on the
/// camera (where the Audio Listener, the game's "ears", lives). A low cutoff
/// removes high frequencies, which is what water does to sound in real life.
/// </summary>
[RequireComponent(typeof(AudioLowPassFilter))]
public class UnderwaterMuffle : MonoBehaviour
{
    [SerializeField] private SurferController surfer;

    [Tooltip("Filter cutoff above water. 22000 = no muffling at all.")]
    [SerializeField] private float normalCutoff = 22000f;
    [Tooltip("Filter cutoff underwater. Lower = more muffled.")]
    [SerializeField] private float underwaterCutoff = 800f;
    [Tooltip("How quickly the sound changes when diving or surfacing.")]
    [SerializeField] private float changeSpeed = 10f;

    private AudioLowPassFilter lowPassFilter;

    private void Awake()
    {
        lowPassFilter = GetComponent<AudioLowPassFilter>();
        lowPassFilter.cutoffFrequency = normalCutoff;
    }

    private void Update()
    {
        bool runActive = GameManager.Instance == null || GameManager.Instance.IsPlaying;

        // Muffle only while underwater during a run (the wipeout should sound clear).
        float targetCutoff = (surfer.IsUnderwater && runActive) ? underwaterCutoff : normalCutoff;

        // Slide toward the target smoothly. Unscaled time keeps it working while paused.
        lowPassFilter.cutoffFrequency = Mathf.Lerp(lowPassFilter.cutoffFrequency, targetCutoff, changeSpeed * Time.unscaledDeltaTime);
    }
}