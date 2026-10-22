using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Runs the title screen: shows the high score, starts the game,
/// opens the How to Play screen, and quits.
/// Each public method is hooked up to a button's On Click.
/// </summary>
public class MainMenu : MonoBehaviour
{
    [Tooltip("Name of the gameplay scene to load when Play is pressed.")]
    [SerializeField] private string gameSceneName = "Game";
    [SerializeField] private TMP_Text highScoreText;
    [Tooltip("The How to Play panel. Starts hidden.")]
    [SerializeField] private GameObject howToPlayPanel;

    private void Start()
    {
        // Make sure time is running (in case we came back here from a paused game).
        Time.timeScale = 1f;

        if (howToPlayPanel != null)
        {
            howToPlayPanel.SetActive(false);
        }

        int best = PlayerPrefs.GetInt(GameManager.HighScoreKey, 0);
        highScoreText.text = $"Best: {best} points";
    }

    /// <summary>Starts a new run.</summary>
    public void Play()
    {
        SceneManager.LoadScene(gameSceneName);
    }

    /// <summary>Opens the How to Play screen.</summary>
    public void ShowHowToPlay()
    {
        howToPlayPanel.SetActive(true);
    }

    /// <summary>Closes the How to Play screen.</summary>
    public void HideHowToPlay()
    {
        howToPlayPanel.SetActive(false);
    }

    /// <summary>Closes the game. (In the Unity editor, it stops Play mode instead.)</summary>
    public void Quit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}