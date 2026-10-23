using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Plays the game's sounds in response to what happens: diving, surfacing,
/// getting hit, pickups, sets arriving, and game over. Also loops the ocean
/// ambience (M to mute) and a heartbeat when breath is running low.
/// Other scripts can play a sound with SoundManager.Instance.PlaySfx(clip).
/// </summary>
public class SoundManager : MonoBehaviour
{
    /// <summary>The one SoundManager in the scene.</summary>
    public static SoundManager Instance { get; private set; }

    [Header("What the SoundManager listens to")]
    [SerializeField] private SurferController surfer;
    [SerializeField] private SurferHealth surferHealth;
    [SerializeField] private BreathMeter breath;
    [SerializeField] private ObstacleSpawner spawner;

    [Header("Audio Sources")]
    [Tooltip("Plays one-off sound effects.")]
    [SerializeField] private AudioSource sfxSource;
    [Tooltip("Loops the calm ocean background sound.")]
    [SerializeField] private AudioSource ambienceSource;
    [Tooltip("Loops the heartbeat when breath is low.")]
    [SerializeField] private AudioSource heartbeatSource;

    [Header("Surfer Sounds")]
    [SerializeField] private AudioClip diveSplash;
    [SerializeField] private AudioClip gasp;
    [SerializeField] private AudioClip hit;
    [SerializeField] private AudioClip wipeout;

    [Header("Game Sounds")]
    [SerializeField] private AudioClip setHorn;
    [SerializeField] private AudioClip shellPickup;
    [SerializeField] private AudioClip breathPickup;
    [SerializeField] private AudioClip shieldPickup;
    [SerializeField] private AudioClip shieldPop;

    [Header("Tuning")]
    [Tooltip("The heartbeat starts when breath drops below this fraction.")]
    [Range(0f, 1f)]
    [SerializeField] private float heartbeatBelow = 0.3f;
    [Tooltip("Gasp volume when surfacing with plenty of breath. It gets louder the closer you were to running out.")]
    [Range(0f, 1f)]
    [SerializeField] private float calmGaspVolume = 0.3f;

    // Used to tell when a life was lost (vs. the starting lives being set).
    private int lastLives = -1;

    private void Awake()
    {
        Instance = this;
    }

    private void OnEnable()
    {
        surfer.Dove += OnDove;
        surfer.Surfaced += OnSurfaced;
        surferHealth.LivesChanged += OnLivesChanged;
        surferHealth.ShieldBlocked += OnShieldBlocked;
        spawner.SetIncoming += OnSetIncoming;
        Pickup.Collected += OnPickupCollected;
    }

    private void OnDisable()
    {
        // Always unsubscribe, especially from static events like Pickup.Collected,
        // or they keep pointing at this destroyed object after the scene reloads.
        surfer.Dove -= OnDove;
        surfer.Surfaced -= OnSurfaced;
        surferHealth.LivesChanged -= OnLivesChanged;
        surferHealth.ShieldBlocked -= OnShieldBlocked;
        spawner.SetIncoming -= OnSetIncoming;
        Pickup.Collected -= OnPickupCollected;
    }

    private void Start()
    {
        GameManager.Instance.GameEnded += OnGameEnded;
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.GameEnded -= OnGameEnded;
        }
    }

    private void Update()
    {
        UpdateHeartbeat();

        // M toggles the ocean ambience, so the game sounds can be heard clearly.
        if (Keyboard.current != null && Keyboard.current.mKey.wasPressedThisFrame)
        {
            ambienceSource.mute = !ambienceSource.mute;
        }
    }

    /// <summary>Plays a one-off sound effect. Safe to call with a missing clip.</summary>
    public void PlaySfx(AudioClip clip, float volume = 1f)
    {
        if (clip != null)
        {
            sfxSource.PlayOneShot(clip, volume);
        }
    }

    private void OnDove()
    {
        PlaySfx(diveSplash);
    }

    private void OnSurfaced()
    {
        // The emptier the breath meter, the louder the gasp.
        float volume = Mathf.Lerp(1f, calmGaspVolume, breath.BreathFraction);
        PlaySfx(gasp, volume);
    }

    private void OnLivesChanged(int lives)
    {
        // Only play the hit sound when lives go DOWN (not when they're first set at the start).
        if (lastLives >= 0 && lives < lastLives)
        {
            PlaySfx(hit);
        }
        lastLives = lives;
    }

    private void OnShieldBlocked()
    {
        PlaySfx(shieldPop);
    }

    private void OnSetIncoming()
    {
        PlaySfx(setHorn);
    }

    private void OnPickupCollected(Pickup.PickupType type)
    {
        switch (type)
        {
            case Pickup.PickupType.Shell:
                PlaySfx(shellPickup, 0.7f);
                break;
            case Pickup.PickupType.DeepBreath:
                PlaySfx(breathPickup);
                break;
            case Pickup.PickupType.BubbleShield:
                PlaySfx(shieldPickup);
                break;
        }
    }

    private void OnGameEnded()
    {
        heartbeatSource.Stop();
        PlaySfx(wipeout);
    }

    /// <summary>Loops the heartbeat while breath is low during a run, and stops it otherwise.</summary>
    private void UpdateHeartbeat()
    {
        bool runActive = GameManager.Instance == null || GameManager.Instance.IsPlaying;
        bool breathLow = runActive && breath.BreathFraction < heartbeatBelow;

        if (breathLow && !heartbeatSource.isPlaying)
        {
            heartbeatSource.Play();
        }
        else if (!breathLow && heartbeatSource.isPlaying)
        {
            heartbeatSource.Stop();
        }
    }
}