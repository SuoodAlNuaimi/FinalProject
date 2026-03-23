using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// PlanetInteraction — Handles flying close to a planet, showing a facts panel,
/// and triggering the quiz when the player is ready.
///
/// Setup:
///   1. Attach this script to each Planet GameObject (e.g. "Earth", "Mars").
///   2. Add a SphereCollider (Is Trigger = true) with a large radius (~5 units).
///   3. Assign the UI panel references in the Inspector.
///   4. Fill in the planetData fields in the Inspector.
/// </summary>
public class PlanetInteraction : MonoBehaviour
{
    [Header("Planet Identity")]
    public int planetIndex = 0;           // 0=Mercury, 1=Venus, ... 7=Neptune
    public string planetName = "Earth";

    [Header("Planet Facts (shown in panel)")]
    [TextArea(2, 4)]
    public string fact1 = "Earth is the third planet from the Sun.";
    [TextArea(2, 4)]
    public string fact2 = "It is the only known planet with life.";
    [TextArea(2, 4)]
    public string fact3 = "Earth has one moon called Luna.";
    [TextArea(2, 4)]
    public string funFact = "One year on Earth is 365.25 days!";

    [Header("UI References (assign in Inspector)")]
    public GameObject factsPanel;         // The full facts UI panel
    public TextMeshProUGUI planetNameText;
    public TextMeshProUGUI fact1Text;
    public TextMeshProUGUI fact2Text;
    public TextMeshProUGUI fact3Text;
    public TextMeshProUGUI funFactText;
    public Button startQuizButton;
    public Button closeButton;
    public GameObject alreadyCompletedBadge; // Optional "✓ Completed" badge

    [Header("Approach Settings")]
    public float approachDistance = 8f;   // How close the ship must be to trigger

    [Header("Visual Feedback")]
    public GameObject glowEffect;         // Optional glow/highlight on hover
    public float rotationSpeed = 10f;     // Planets slowly rotate

    private bool playerInRange = false;
    private bool panelOpen = false;
    private Transform playerShip;

    private void Start()
    {
        // Make sure panel is hidden at start
        if (factsPanel != null) factsPanel.SetActive(false);
        if (glowEffect != null) glowEffect.SetActive(false);

        // Wire up buttons
        if (startQuizButton != null)
            startQuizButton.onClick.AddListener(OnStartQuiz);
        if (closeButton != null)
            closeButton.onClick.AddListener(ClosePanel);

        // Show completed badge if already done
        UpdateCompletedBadge();

        // Find the player ship
        playerShip = GameObject.FindGameObjectWithTag("Player")?.transform;
    }

    private void Update()
    {
        // Slow rotation to make planets look alive
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);

        // Check distance to player manually (works without trigger colliders)
        if (playerShip != null)
        {
            float dist = Vector3.Distance(transform.position, playerShip.position);
            bool inRange = dist <= approachDistance;

            if (inRange && !playerInRange) OnPlayerEnter();
            if (!inRange && playerInRange) OnPlayerExit();

            playerInRange = inRange;
        }

        // Press E to open/close facts panel when in range
        if (playerInRange && Input.GetKeyDown(KeyCode.E))
        {
            if (panelOpen) ClosePanel();
            else OpenPanel();
        }
    }

    // ─── Proximity Events ─────────────────────────────────────────────────────

    private void OnPlayerEnter()
    {
        if (glowEffect != null) glowEffect.SetActive(true);
        Debug.Log($"Approaching {planetName}! Press E to learn more.");
        // You can also show a small "Press E" HUD hint here
    }

    private void OnPlayerExit()
    {
        if (glowEffect != null) glowEffect.SetActive(false);
        if (panelOpen) ClosePanel();
    }

    // ─── Facts Panel ──────────────────────────────────────────────────────────

    public void OpenPanel()
    {
        if (factsPanel == null) return;

        // Populate text fields
        if (planetNameText != null) planetNameText.text = planetName;
        if (fact1Text != null) fact1Text.text = fact1;
        if (fact2Text != null) fact2Text.text = fact2;
        if (fact3Text != null) fact3Text.text = fact3;
        if (funFactText != null) funFactText.text = "⭐ " + funFact;

        // Hide quiz button if already completed
        bool completed = GameManager.Instance != null &&
                         GameManager.Instance.IsPlanetCompleted(planetIndex);
        if (startQuizButton != null)
            startQuizButton.gameObject.SetActive(!completed);

        factsPanel.SetActive(true);
        panelOpen = true;

        // Unlock cursor so player can click buttons
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void ClosePanel()
    {
        if (factsPanel != null) factsPanel.SetActive(false);
        panelOpen = false;

        // Re-lock cursor for flying
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    // ─── Quiz Trigger ─────────────────────────────────────────────────────────

    private void OnStartQuiz()
    {
        ClosePanel();

        // Tell the QuizManager which planet we're quizzing
        QuizManager quiz = FindObjectOfType<QuizManager>();
        if (quiz != null)
        {
            quiz.StartQuiz(planetIndex);
        }
        else
        {
            Debug.LogWarning("QuizManager not found in scene!");
        }
    }

    // ─── Completion Badge ─────────────────────────────────────────────────────

    private void UpdateCompletedBadge()
    {
        if (alreadyCompletedBadge == null) return;
        bool completed = GameManager.Instance != null &&
                         GameManager.Instance.IsPlanetCompleted(planetIndex);
        alreadyCompletedBadge.SetActive(completed);
    }
}
