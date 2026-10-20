using UnityEngine;

/// <summary>
/// Runs the overall game: how fast the ocean scrolls, how far the
/// surfer has paddled, and whether a run is in progress.
/// Other scripts reach it through GameManager.Instance.
/// </summary>
public class GameManager : MonoBehaviour
{
    /// <summary>The one GameManager in the scene, so other scripts can find it easily.</summary>
    public static GameManager Instance { get; private set; }

    [Header("Ocean Speed")]
    [Tooltip("How fast obstacles move toward the surfer at the start of a run.")]
    [SerializeField] private float startSpeed = 5f;
    [Tooltip("The fastest the ocean can ever scroll.")]
    [SerializeField] private float maxSpeed = 12f;
    [Tooltip("How much the speed increases every second.")]
    [SerializeField] private float speedIncreasePerSecond = 0.1f;

    /// <summary>Current speed everything scrolls left at.</summary>
    public float ScrollSpeed { get; private set; }

    /// <summary>Total distance paddled this run (used for score).</summary>
    public float Distance { get; private set; }

    /// <summary>False once the run is over.</summary>
    public bool IsPlaying { get; private set; } = true;

    private void Awake()
    {
        Instance = this;
        ScrollSpeed = startSpeed;
    }

    private void Update()
    {
        if (!IsPlaying)
        {
            return;
        }

        // Slowly ramp up the speed so the game gets harder the longer you last.
        ScrollSpeed = Mathf.Min(maxSpeed, ScrollSpeed + speedIncreasePerSecond * Time.deltaTime);
        Distance += ScrollSpeed * Time.deltaTime;
    }
}