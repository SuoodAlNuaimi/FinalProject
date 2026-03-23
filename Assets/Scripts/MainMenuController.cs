using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// MainMenuController — Drives the Main Menu scene.
/// Attach to an empty GameObject in your MainMenu scene.
/// </summary>
public class MainMenuController : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject mainPanel;       // Start, How to Play, Quit buttons
    public GameObject howToPlayPanel;  // Instructions panel

    [Header("Buttons")]
    public Button startButton;
    public Button howToPlayButton;
    public Button backButton;
    public Button quitButton;

    [Header("Animated Title")]
    public RectTransform titleTransform;
    public float titleBobSpeed = 1.5f;
    public float titleBobAmount = 8f;

    private Vector3 titleStartPos;

    private void Start()
    {
        // Wire buttons
        if (startButton    != null) startButton.onClick.AddListener(StartGame);
        if (howToPlayButton!= null) howToPlayButton.onClick.AddListener(ShowHowToPlay);
        if (backButton     != null) backButton.onClick.AddListener(ShowMain);
        if (quitButton     != null) quitButton.onClick.AddListener(QuitGame);

        // Show main panel
        ShowMain();

        // Save title start position for bobbing animation
        if (titleTransform != null)
            titleStartPos = titleTransform.anchoredPosition;

        // Make sure cursor is visible on menu
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void Update()
    {
        // Gently bob the title text up and down
        if (titleTransform != null)
        {
            float newY = titleStartPos.y + Mathf.Sin(Time.time * titleBobSpeed) * titleBobAmount;
            titleTransform.anchoredPosition = new Vector2(titleStartPos.x, newY);
        }
    }

    // ─── Button Handlers ──────────────────────────────────────────────────────

    private void StartGame()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.StartGame();
        else
            UnityEngine.SceneManagement.SceneManager.LoadScene("SolarSystem");
    }

    private void ShowHowToPlay()
    {
        mainPanel.SetActive(false);
        howToPlayPanel.SetActive(true);
    }

    private void ShowMain()
    {
        mainPanel.SetActive(true);
        howToPlayPanel.SetActive(false);
    }

    private void QuitGame()
    {
        Application.Quit();
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }
}
