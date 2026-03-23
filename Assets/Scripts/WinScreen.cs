using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// WinScreen — Shown when the player completes all 8 planets.
/// Attach to an empty GameObject in your WinScreen scene.
/// </summary>
public class WinScreen : MonoBehaviour
{
    [Header("UI Elements")]
    public TextMeshProUGUI congratsText;
    public TextMeshProUGUI summaryText;
    public TextMeshProUGUI starRatingText;   // ⭐ rating based on score
    public Button playAgainButton;
    public Button mainMenuButton;

    [Header("Celebration")]
    public ParticleSystem confettiEffect;   // Optional — assign a particle system

    private void Start()
    {
        // Wire buttons
        if (playAgainButton != null) playAgainButton.onClick.AddListener(PlayAgain);
        if (mainMenuButton  != null) mainMenuButton.onClick.AddListener(GoToMenu);

        // Show score summary
        ShowResults();

        // Play celebration effect
        if (confettiEffect != null) confettiEffect.Play();

        // Ensure cursor is visible
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void ShowResults()
    {
        if (GameManager.Instance == null) return;

        string summary = GameManager.Instance.GetScoreSummary();
        int score      = GameManager.Instance.totalScore;
        int maxScore   = 8 * 3 * 10; // 8 planets × 3 questions × 10 pts

        if (congratsText != null)
            congratsText.text = "🎉 You Explored the Whole Solar System!";

        if (summaryText != null)
            summaryText.text = summary;

        if (starRatingText != null)
        {
            float pct = (float)score / maxScore;
            if      (pct >= 0.9f) starRatingText.text = "⭐⭐⭐ Amazing Explorer!";
            else if (pct >= 0.6f) starRatingText.text = "⭐⭐ Great Job!";
            else                  starRatingText.text = "⭐ Keep Exploring!";
        }
    }

    private void PlayAgain()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.StartGame();
        else
            UnityEngine.SceneManagement.SceneManager.LoadScene("SolarSystem");
    }

    private void GoToMenu()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.GoToMainMenu();
        else
            UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
    }
}
