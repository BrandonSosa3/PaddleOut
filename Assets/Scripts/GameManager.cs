using UnityEngine;

/// <summary>
/// Runs the overall game: how fast the ocean scrolls, how far the
/// surfer has paddled, shells collected, the score, and whether a run
/// is in progress. Other scripts reach it through GameManager.Instance.
/// </summary>
public class GameManager : MonoBehaviour
{
    /// <summary>The one GameManager in the scene, so other scripts can find it easily.</summary>
    public static GameManager Instance { get; private set; }

    // Key used to save the high score on the player's computer.
    public const string HighScoreKey = "HighScore";

    [Header("Ocean Speed")]
    [Tooltip("How fast obstacles move toward the surfer at the start of a run.")]
    [SerializeField] private float startSpeed = 5f;
    [Tooltip("The fastest the ocean can ever scroll.")]
    [SerializeField] private float maxSpeed = 12f;
    [Tooltip("How much the speed increases every second.")]
    [SerializeField] private float speedIncreasePerSecond = 0.1f;

    [Header("Scoring")]
    [Tooltip("Points each shell is worth.")]
    [SerializeField] private int pointsPerShell = 10;

    /// <summary>Current speed everything scrolls left at.</summary>
    public float ScrollSpeed { get; private set; }

    /// <summary>Total distance paddled this run.</summary>
    public float Distance { get; private set; }

    /// <summary>Shells collected this run.</summary>
    public int Shells { get; private set; }

    /// <summary>False once the run is over.</summary>
    public bool IsPlaying { get; private set; } = true;

    /// <summary>This run's score: distance plus bonus points for shells.</summary>
    public int Score => Mathf.FloorToInt(Distance) + Shells * pointsPerShell;

    /// <summary>Best score ever, saved between play sessions.</summary>
    public int HighScore => PlayerPrefs.GetInt(HighScoreKey, 0);

    /// <summary>True if this run beat the old high score.</summary>
    public bool IsNewHighScore { get; private set; }

    /// <summary>Fires once when the run ends (used by the game over screen).</summary>
    public event System.Action GameEnded;

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

    /// <summary>Adds one shell. Called by shell pickups.</summary>
    public void AddShell()
    {
        if (IsPlaying)
        {
            Shells++;
        }
    }

    /// <summary>Stops the run and saves the high score. Called when the surfer runs out of lives.</summary>
    public void EndGame()
    {
        if (!IsPlaying)
        {
            return;
        }

        IsPlaying = false;
        ScrollSpeed = 0f;

        // Save a new high score if this run beat it.
        if (Score > HighScore)
        {
            PlayerPrefs.SetInt(HighScoreKey, Score);
            PlayerPrefs.Save();
            IsNewHighScore = true;
        }

        GameEnded?.Invoke();
    }
}