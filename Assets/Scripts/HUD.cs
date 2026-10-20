using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Keeps the on-screen info up to date: distance paddled, lives left,
/// and the "SET INCOMING" warning banner.
/// </summary>
public class HUD : MonoBehaviour
{
    [Header("What the HUD listens to")]
    [SerializeField] private SurferHealth surferHealth;
    [SerializeField] private ObstacleSpawner spawner;

    [Header("UI Elements")]
    [Tooltip("Text that shows distance paddled.")]
    [SerializeField] private TMP_Text distanceText;
    [Tooltip("One icon per life, in order. Icons hide as lives are lost.")]
    [SerializeField] private Image[] lifeIcons;
    [Tooltip("The SET INCOMING banner. Starts hidden.")]
    [SerializeField] private GameObject setBanner;
    [Tooltip("How long the banner stays on screen.")]
    [SerializeField] private float bannerDuration = 2f;

    private void OnEnable()
    {
        // Start listening for lives changing and sets coming in.
        if (surferHealth != null) surferHealth.LivesChanged += ShowLives;
        if (spawner != null) spawner.SetIncoming += ShowSetBanner;
    }

    private void OnDisable()
    {
        // Stop listening when the HUD is turned off (avoids errors).
        if (surferHealth != null) surferHealth.LivesChanged -= ShowLives;
        if (spawner != null) spawner.SetIncoming -= ShowSetBanner;
    }

    private void Start()
    {
        if (setBanner != null)
        {
            setBanner.SetActive(false);
        }
    }

    private void Update()
    {
        if (distanceText != null && GameManager.Instance != null)
        {
            distanceText.text = $"{GameManager.Instance.Distance:F0} m";
        }
    }

    /// <summary>Shows one icon for each life remaining.</summary>
    private void ShowLives(int lives)
    {
        for (int i = 0; i < lifeIcons.Length; i++)
        {
            lifeIcons[i].enabled = i < lives;
        }
    }

    /// <summary>Flashes the SET INCOMING banner for a couple of seconds.</summary>
    private void ShowSetBanner()
    {
        if (setBanner == null)
        {
            return;
        }

        setBanner.SetActive(true);
        CancelInvoke(nameof(HideSetBanner));
        Invoke(nameof(HideSetBanner), bannerDuration);
    }

    private void HideSetBanner()
    {
        setBanner.SetActive(false);
    }
}