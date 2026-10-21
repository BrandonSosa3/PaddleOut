using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

/// <summary>
/// Shows the game over panel with the final score and high score,
/// and restarts the run when the player clicks Play Again (or presses Enter).
/// </summary>
public class GameOverScreen : MonoBehaviour
{
    [Tooltip("The whole game over panel. Hidden until the run ends.")]
    [SerializeField] private GameObject panel;
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private TMP_Text highScoreText;
    [Tooltip("Short pause before the panel appears, so the wipeout can be seen.")]
    [SerializeField] private float showDelay = 1f;

    private void Start()
    {
        panel.SetActive(false);
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
        // Let the player restart with Enter instead of reaching for the mouse.
        if (panel.activeSelf && Keyboard.current != null && Keyboard.current.enterKey.wasPressedThisFrame)
        {
            PlayAgain();
        }
    }

    private void OnGameEnded()
    {
        Invoke(nameof(Show), showDelay);
    }

    /// <summary>Fills in the score text and shows the panel.</summary>
    private void Show()
    {
        GameManager game = GameManager.Instance;
        scoreText.text = $"{Mathf.FloorToInt(game.Distance)} m  +  {game.Shells} shells  =  {game.Score} points";
        highScoreText.text = game.IsNewHighScore ? "NEW HIGH SCORE!" : $"Best: {game.HighScore} points";
        panel.SetActive(true);
    }

    /// <summary>Reloads the scene to start a fresh run. Hooked up to the Play Again button.</summary>
    public void PlayAgain()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}