using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Pauses and unpauses the game with Esc or P.
/// Pausing works by setting Time.timeScale to 0, which freezes
/// everything that uses Time.deltaTime (movement, breath, spawning).
/// </summary>
public class PauseMenu : MonoBehaviour
{
    [Tooltip("The panel shown while paused. Starts hidden.")]
    [SerializeField] private GameObject pausePanel;

    /// <summary>True while the game is paused.</summary>
    public bool IsPaused { get; private set; }

    private void Start()
    {
        pausePanel.SetActive(false);
        // Make sure time is running when a new run starts (in case we restarted while paused).
        Time.timeScale = 1f;
        AudioListener.pause = false;
    }

    private void Update()
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard == null)
        {
            return;
        }

        bool pausePressed = keyboard.escapeKey.wasPressedThisFrame || keyboard.pKey.wasPressedThisFrame;
        bool gameIsRunning = GameManager.Instance == null || GameManager.Instance.IsPlaying;

        // Only allow pausing during an active run (not on the game over screen).
        if (pausePressed && gameIsRunning)
        {
            TogglePause();
        }
    }

    /// <summary>Switches between paused and playing.</summary>
    public void TogglePause()
    {
        IsPaused = !IsPaused;
        Time.timeScale = IsPaused ? 0f : 1f;
        AudioListener.pause = IsPaused;
        pausePanel.SetActive(IsPaused);
    }
}