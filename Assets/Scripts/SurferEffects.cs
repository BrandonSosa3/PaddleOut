using UnityEngine;

/// <summary>
/// Visual water effects for the surfer: a splash of spray when diving under
/// or popping back up, and a trail of bubbles while underwater.
/// </summary>
[RequireComponent(typeof(SurferController))]
public class SurferEffects : MonoBehaviour
{
    [Tooltip("Particle system that sprays water at the surface.")]
    [SerializeField] private ParticleSystem splash;
    [Tooltip("Particle system attached to the surfer that releases bubbles underwater.")]
    [SerializeField] private ParticleSystem bubbleTrail;
    [Tooltip("How many droplets each splash throws.")]
    [SerializeField] private int splashDroplets = 25;
    [Tooltip("Height of the water surface, where splashes appear.")]
    [SerializeField] private float surfaceY = 1f;

    private SurferController surfer;

    private void Awake()
    {
        surfer = GetComponent<SurferController>();
    }

    private void OnEnable()
    {
        surfer.Dove += PlaySplash;
        surfer.Surfaced += PlaySplash;
    }

    private void OnDisable()
    {
        surfer.Dove -= PlaySplash;
        surfer.Surfaced -= PlaySplash;
    }

    private void Update()
    {
        // Only release bubbles while underwater during an active run.
        bool runActive = GameManager.Instance == null || GameManager.Instance.IsPlaying;
        ParticleSystem.EmissionModule emission = bubbleTrail.emission;
        emission.enabled = surfer.IsUnderwater && runActive;
    }

    /// <summary>Moves the splash to where the surfer crosses the surface and fires a burst of droplets.</summary>
    private void PlaySplash()
    {
        splash.transform.position = new Vector3(transform.position.x, surfaceY, 0f);
        splash.Emit(splashDroplets);
    }
}