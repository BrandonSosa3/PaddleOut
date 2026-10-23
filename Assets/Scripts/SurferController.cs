using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Controls the surfer's vertical movement.
/// The surfer floats on the surface by default. Holding the dive key pushes
/// them underwater, and letting go lets them float back up like a real board.
/// If the surfer is above the water (after a kite ride), gravity pulls them
/// back down and they plunge into the water.
/// While the kite is flying, KiteFlight takes over movement instead.
/// </summary>
public class SurferController : MonoBehaviour
{
    [Header("Depth Limits")]
    [Tooltip("The Y position of the water surface, where the surfer floats.")]
    [SerializeField] private float surfaceY = 1f;
    [Tooltip("The deepest Y position the surfer can dive to.")]
    [SerializeField] private float maxDepthY = -3.5f;

    [Header("Movement Feel")]
    [Tooltip("How hard the surfer is pushed down while holding dive.")]
    [SerializeField] private float diveForce = 25f;
    [Tooltip("How hard the water pushes the surfer back up when not diving.")]
    [SerializeField] private float buoyancy = 18f;
    [Tooltip("How quickly the water slows the surfer down. Higher = thicker water.")]
    [SerializeField] private float waterDrag = 3f;
    [Tooltip("Fastest the surfer can move up or down.")]
    [SerializeField] private float maxSpeed = 8f;
    [Tooltip("How much the surfer bounces when popping back to the surface (0 = no bounce).")]
    [SerializeField] private float surfaceBounce = 0.25f;
    [Tooltip("How much the board tilts based on vertical speed.")]
    [SerializeField] private float tiltAmount = 3f;

    [Header("Falling From the Sky")]
    [Tooltip("How fast the surfer falls after the kite lets go.")]
    [SerializeField] private float airGravity = 15f;
    [Tooltip("How much speed is kept when hitting the water from above (0 = full stop, 1 = no slowdown).")]
    [Range(0f, 1f)]
    [SerializeField] private float landingSpeedKept = 0.4f;

    [Header("Sound Triggers")]
    [Tooltip("How far below the surface counts as a real dive. Stops tiny bobs from triggering splash sounds.")]
    [SerializeField] private float splashDepth = 0.4f;

    /// <summary>Fires the moment the surfer dives under (used for the splash sound).</summary>
    public event System.Action Dove;

    /// <summary>Fires the moment the surfer comes back up (used for the gasp sound).</summary>
    public event System.Action Surfaced;

    /// <summary>True when the surfer is below the water surface.</summary>
    public bool IsUnderwater => transform.position.y < surfaceY - 0.1f;

    /// <summary>True when the surfer is above the water (falling after a kite ride).</summary>
    public bool IsInAir => transform.position.y > surfaceY + 0.01f;

    // Current vertical speed. Negative = moving down, positive = moving up.
    private float velocityY;

    // Used to block diving when the surfer is out of breath.
    private BreathMeter breath;

    // While the kite is flying, it controls movement instead of this script.
    private KiteFlight kite;

    // Tracks whether we're in a "real" dive, so splash/gasp only fire once per dive.
    private bool isSubmerged;

    private void Awake()
    {
        breath = GetComponent<BreathMeter>();
        kite = GetComponent<KiteFlight>();
    }

    private void Update()
    {
        // Freeze the surfer once the run is over.
        if (GameManager.Instance != null && !GameManager.Instance.IsPlaying)
        {
            return;
        }

        // The kite is in charge while flying. Start from a standstill when it lets go.
        if (kite != null && kite.IsFlying)
        {
            velocityY = 0f;
            return;
        }

        bool wasInAir = IsInAir;

        // In the air, gravity pulls down. In the water, dive or float.
        float acceleration;
        if (wasInAir)
        {
            acceleration = -airGravity;
        }
        else
        {
            // Can only dive if the key is held AND the surfer has breath to spare.
            bool isDiving = IsDiveHeld() && (breath == null || breath.CanDive);
            acceleration = isDiving ? -diveForce : buoyancy;
        }
        velocityY += acceleration * Time.deltaTime;

        // Water drag slows the surfer down so movement feels heavy and smooth (no drag in the air).
        if (!wasInAir)
        {
            velocityY -= velocityY * waterDrag * Time.deltaTime;
        }
        velocityY = Mathf.Clamp(velocityY, -maxSpeed, maxSpeed);

        // Move the surfer.
        Vector3 position = transform.position;
        position.y += velocityY * Time.deltaTime;

        if (wasInAir)
        {
            // Hitting the water from above: the water absorbs most of the speed,
            // so the surfer plunges a little way under, then floats back up.
            if (position.y <= surfaceY)
            {
                velocityY *= landingSpeedKept;
            }
        }
        else if (position.y > surfaceY)
        {
            // Coming up from below: stop at the surface, with a small bounce.
            position.y = surfaceY;
            if (velocityY > 0f)
            {
                velocityY = -velocityY * surfaceBounce;
            }
        }

        // Stop at the maximum depth.
        if (position.y < maxDepthY)
        {
            position.y = maxDepthY;
            if (velocityY < 0f)
            {
                velocityY = 0f;
            }
        }

        transform.position = position;

        // Tilt the board: nose down when diving, nose up when rising.
        transform.rotation = Quaternion.Euler(0f, 0f, velocityY * tiltAmount);

        CheckSurfaceCrossing(position.y);
    }

    /// <summary>Fires Dove/Surfaced once each time the surfer goes under or comes back up.</summary>
    private void CheckSurfaceCrossing(float y)
    {
        if (!isSubmerged && y < surfaceY - splashDepth)
        {
            isSubmerged = true;
            Dove?.Invoke();
        }
        else if (isSubmerged && y >= surfaceY - 0.05f)
        {
            isSubmerged = false;
            Surfaced?.Invoke();
        }
    }

    /// <summary>Returns true while Space, S, or the Down Arrow is held.</summary>
    private bool IsDiveHeld()
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard == null)
        {
            return false;
        }

        return keyboard.spaceKey.isPressed
            || keyboard.sKey.isPressed
            || keyboard.downArrowKey.isPressed;
    }
} 