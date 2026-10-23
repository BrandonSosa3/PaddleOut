using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// The kitesurfing power-up. Grabbing a kite pickup lifts the surfer out of
/// the water into the sky for a few seconds. Hold Space (or W / Up Arrow) to
/// climb, let go to drift down. A row of bonus shells appears in the sky,
/// and seagulls are the danger up there. The kite blinks just before it
/// lets go, then the surfer falls back into the water.
/// </summary>
[RequireComponent(typeof(SurferController))]
public class KiteFlight : MonoBehaviour
{
    [Header("Flight Time")]
    [Tooltip("How many seconds one kite lasts.")]
    [SerializeField] private float flightDuration = 5f;
    [Tooltip("The kite blinks for this many seconds before letting go, as a warning.")]
    [SerializeField] private float warningTime = 1f;

    [Header("Sky Limits")]
    [Tooltip("Lowest the surfer can drift while the kite is up (keeps them out of the water).")]
    [SerializeField] private float minSkyY = 2f;
    [Tooltip("Highest the surfer can fly.")]
    [SerializeField] private float maxSkyY = 4.2f;

    [Header("Movement Feel")]
    [Tooltip("Upward push while holding climb.")]
    [SerializeField] private float liftForce = 16f;
    [Tooltip("Downward pull while not holding climb.")]
    [SerializeField] private float gravity = 10f;
    [Tooltip("Air resistance. Higher = smoother, floatier movement.")]
    [SerializeField] private float airDrag = 2f;
    [Tooltip("Fastest the surfer can move up or down while flying.")]
    [SerializeField] private float maxSpeed = 6f;
    [Tooltip("Upward speed the moment the kite catches the wind.")]
    [SerializeField] private float launchSpeed = 9f;
    [Tooltip("How much the surfer tilts based on vertical speed.")]
    [SerializeField] private float tiltAmount = 3f;

    [Header("Visuals and Sound")]
    [Tooltip("The kite (with its string) drawn above the surfer. Hidden when not flying.")]
    [SerializeField] private GameObject kiteVisual;
    [Tooltip("Played when the kite lifts off.")]
    [SerializeField] private AudioClip launchSound;
    [Tooltip("Loops the wind sound while flying.")]
    [SerializeField] private AudioSource windSource;

    [Header("Sky Shell Bonus")]
    [Tooltip("Shell prefab that spawns at sky height (its Pickup random height keeps it in the sky).")]
    [SerializeField] private GameObject skyShellPrefab;
    [SerializeField] private int skyShellCount = 5;
    [Tooltip("X position of the first bonus shell (just off the right edge of the screen).")]
    [SerializeField] private float firstShellX = 11f;
    [Tooltip("Space between bonus shells.")]
    [SerializeField] private float shellSpacing = 2f;

    /// <summary>True while the kite is carrying the surfer.</summary>
    public bool IsFlying { get; private set; }

    private float timeLeft;
    private float velocityY;

    // False while still rising out of the water after launch; true once the surfer reaches the sky.
    private bool reachedSky;

    private void Start()
    {
        SetKiteVisible(false);
    }

    /// <summary>Starts a kite ride. Grabbing another kite mid-flight just refills the timer.</summary>
    public void StartKite()
    {
        timeLeft = flightDuration;
        if (IsFlying)
        {
            return;
        }

        IsFlying = true;
        reachedSky = false;
        velocityY = launchSpeed;
        SetKiteVisible(true);

        if (windSource != null)
        {
            windSource.Play();
        }
        if (launchSound != null && SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySfx(launchSound);
        }

        SpawnSkyShells();
    }

    private void Update()
    {
        if (!IsFlying)
        {
            return;
        }

        // Let go of the kite if the run ends.
        if (GameManager.Instance != null && !GameManager.Instance.IsPlaying)
        {
            EndKite();
            return;
        }

        timeLeft -= Time.deltaTime;
        if (timeLeft <= 0f)
        {
            EndKite();
            return;
        }

        Fly();
        UpdateWarningBlink();
    }

    /// <summary>Moves the surfer up and down in the sky based on the climb key.</summary>
    private void Fly()
    {
        Vector3 position = transform.position;

        // Until the surfer reaches the sky, the kite pulls them up no matter what.
        if (position.y >= minSkyY)
        {
            reachedSky = true;
        }
        bool climbing = !reachedSky || IsClimbHeld();

        velocityY += (climbing ? liftForce : -gravity) * Time.deltaTime;
        velocityY -= velocityY * airDrag * Time.deltaTime;
        velocityY = Mathf.Clamp(velocityY, -maxSpeed, maxSpeed);
        position.y += velocityY * Time.deltaTime;

        // Keep the surfer between the sky limits.
        if (position.y > maxSkyY)
        {
            position.y = maxSkyY;
            velocityY = Mathf.Min(velocityY, 0f);
        }
        if (reachedSky && position.y < minSkyY)
        {
            position.y = minSkyY;
            velocityY = Mathf.Max(velocityY, 0f);
        }

        transform.position = position;
        transform.rotation = Quaternion.Euler(0f, 0f, velocityY * tiltAmount);
    }

    /// <summary>Blinks the kite during the last second so the player knows it's about to let go.</summary>
    private void UpdateWarningBlink()
    {
        if (kiteVisual == null)
        {
            return;
        }

        bool visible = timeLeft > warningTime || Mathf.Repeat(timeLeft, 0.2f) > 0.1f;
        kiteVisual.SetActive(visible);
    }

    /// <summary>The kite lets go. SurferController takes over and gravity pulls the surfer down.</summary>
    private void EndKite()
    {
        IsFlying = false;
        SetKiteVisible(false);

        if (windSource != null)
        {
            windSource.Stop();
        }
    }

    /// <summary>Places a row of bonus shells in the sky, only reachable with the kite.</summary>
    private void SpawnSkyShells()
    {
        if (skyShellPrefab == null)
        {
            return;
        }

        for (int i = 0; i < skyShellCount; i++)
        {
            Vector3 position = new Vector3(firstShellX + i * shellSpacing, maxSkyY, 0f);
            Instantiate(skyShellPrefab, position, Quaternion.identity);
        }
    }

    private void SetKiteVisible(bool visible)
    {
        if (kiteVisual != null)
        {
            kiteVisual.SetActive(visible);
        }
    }

    /// <summary>Returns true while Space, W, or the Up Arrow is held.</summary>
    private bool IsClimbHeld()
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard == null)
        {
            return false;
        }

        return keyboard.spaceKey.isPressed
            || keyboard.wKey.isPressed
            || keyboard.upArrowKey.isPressed;
    }
}