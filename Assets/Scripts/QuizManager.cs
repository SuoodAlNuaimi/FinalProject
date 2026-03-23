using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// QuizManager — Runs a multiple-choice quiz for each planet.
/// Attach to a persistent empty GameObject or the GameManager object.
///
/// Setup:
///   1. Create a Quiz UI Panel with: question text, 4 answer buttons, feedback text, score text.
///   2. Assign all UI references below in the Inspector.
///   3. The planet questions are defined right here in code — easy to edit!
/// </summary>
public class QuizManager : MonoBehaviour
{
    [Header("UI References")]
    public GameObject quizPanel;
    public TextMeshProUGUI questionText;
    public TextMeshProUGUI progressText;      // "Question 2 of 3"
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI feedbackText;      // "✅ Correct!" or "❌ Try again"
    public Button[] answerButtons;            // Assign exactly 4 buttons
    public TextMeshProUGUI[] answerTexts;     // Text component on each button
    public Button nextButton;                 // "Next Question" button
    public Button finishButton;              // "Finish & Continue" (shown at end)

    [Header("Settings")]
    public int pointsPerCorrectAnswer = 10;
    public float feedbackDelay = 1.2f;        // Seconds to show feedback before moving on

    // Colors
    private Color correctColor   = new Color(0.2f, 0.8f, 0.3f);   // Green
    private Color wrongColor     = new Color(0.9f, 0.2f, 0.2f);    // Red
    private Color defaultColor   = new Color(0.2f, 0.4f, 0.8f);    // Blue

    // State
    private int currentPlanetIndex = -1;
    private int currentQuestionIndex = 0;
    private int sessionScore = 0;
    private List<Question> currentQuestions;
    private bool answerLocked = false;

    // ─── Question Database ────────────────────────────────────────────────────

    [System.Serializable]
    public class Question
    {
        public string questionText;
        public string[] answers;       // Always 4 answers
        public int correctIndex;       // Index of the correct answer (0–3)
    }

    // All 8 planets, 3 questions each — kid-friendly language!
    private Dictionary<int, List<Question>> planetQuestions = new Dictionary<int, List<Question>>()
    {
        // 0 — Mercury
        { 0, new List<Question> {
            new Question { questionText = "Mercury is the _____ planet from the Sun.",
                answers = new[]{"First","Second","Third","Fourth"}, correctIndex = 0 },
            new Question { questionText = "How many moons does Mercury have?",
                answers = new[]{"0","1","2","4"}, correctIndex = 0 },
            new Question { questionText = "Mercury is the _____ planet in our solar system.",
                answers = new[]{"Biggest","Smallest","Coldest","Flattest"}, correctIndex = 1 },
        }},
        // 1 — Venus
        { 1, new List<Question> {
            new Question { questionText = "Venus is the _____ planet in the solar system.",
                answers = new[]{"Hottest","Coldest","Fastest","Largest"}, correctIndex = 0 },
            new Question { questionText = "How many moons does Venus have?",
                answers = new[]{"3","1","0","2"}, correctIndex = 2 },
            new Question { questionText = "Venus spins in which direction compared to most planets?",
                answers = new[]{"Same direction","Backwards","Sideways","It doesn't spin"}, correctIndex = 1 },
        }},
        // 2 — Earth
        { 2, new List<Question> {
            new Question { questionText = "What is the name of Earth's moon?",
                answers = new[]{"Titan","Luna","Phobos","Europa"}, correctIndex = 1 },
            new Question { questionText = "How much of Earth is covered by water?",
                answers = new[]{"About 25%","About 50%","About 71%","About 90%"}, correctIndex = 2 },
            new Question { questionText = "Earth is the _____ planet from the Sun.",
                answers = new[]{"Second","Third","Fourth","Fifth"}, correctIndex = 1 },
        }},
        // 3 — Mars
        { 3, new List<Question> {
            new Question { questionText = "What color is Mars?",
                answers = new[]{"Blue","Yellow","Red","Purple"}, correctIndex = 2 },
            new Question { questionText = "How many moons does Mars have?",
                answers = new[]{"0","1","2","4"}, correctIndex = 2 },
            new Question { questionText = "The biggest volcano in the solar system is on Mars. What is it called?",
                answers = new[]{"Mount Everest","Olympus Mons","Valles Marineris","Caloris Basin"}, correctIndex = 1 },
        }},
        // 4 — Jupiter
        { 4, new List<Question> {
            new Question { questionText = "Jupiter is the _____ planet in the solar system.",
                answers = new[]{"Hottest","Biggest","Fastest","Oldest"}, correctIndex = 1 },
            new Question { questionText = "What is the famous giant storm on Jupiter called?",
                answers = new[]{"The Blue Eye","The Great Red Spot","The Super Swirl","The Mega Cyclone"}, correctIndex = 1 },
            new Question { questionText = "About how many Earths could fit inside Jupiter?",
                answers = new[]{"10","100","1,000","1,300"}, correctIndex = 3 },
        }},
        // 5 — Saturn
        { 5, new List<Question> {
            new Question { questionText = "What is Saturn famous for?",
                answers = new[]{"Its giant storm","Its beautiful rings","Its many craters","Its bright colour"}, correctIndex = 1 },
            new Question { questionText = "Saturn's rings are made mostly of what?",
                answers = new[]{"Rock and dust","Ice and rock","Gas and clouds","Metal and fire"}, correctIndex = 1 },
            new Question { questionText = "What is Saturn's largest moon called?",
                answers = new[]{"Europa","Ganymede","Titan","Callisto"}, correctIndex = 2 },
        }},
        // 6 — Uranus
        { 6, new List<Question> {
            new Question { questionText = "Uranus is unique because it spins on its side. This means it is very _____.",
                answers = new[]{"Fast","Tilted","Small","Cold"}, correctIndex = 1 },
            new Question { questionText = "What colour does Uranus appear from space?",
                answers = new[]{"Red","Yellow","Blue-green","Orange"}, correctIndex = 2 },
            new Question { questionText = "Uranus is classified as what type of planet?",
                answers = new[]{"Rocky planet","Ice giant","Gas giant","Dwarf planet"}, correctIndex = 1 },
        }},
        // 7 — Neptune
        { 7, new List<Question> {
            new Question { questionText = "Neptune is the _____ planet from the Sun.",
                answers = new[]{"Sixth","Seventh","Eighth","Ninth"}, correctIndex = 2 },
            new Question { questionText = "Neptune has the fastest what in the solar system?",
                answers = new[]{"Moons","Winds","Rotation","Orbit"}, correctIndex = 1 },
            new Question { questionText = "What is the name of Neptune's largest moon?",
                answers = new[]{"Triton","Titan","Oberon","Nereid"}, correctIndex = 0 },
        }},
    };

    // ─── Lifecycle ────────────────────────────────────────────────────────────

    private void Start()
    {
        if (quizPanel != null) quizPanel.SetActive(false);
        if (nextButton != null) nextButton.onClick.AddListener(NextQuestion);
        if (finishButton != null) finishButton.onClick.AddListener(FinishQuiz);

        // Wire up answer buttons
        for (int i = 0; i < answerButtons.Length; i++)
        {
            int index = i; // Capture for closure
            answerButtons[i].onClick.AddListener(() => OnAnswerSelected(index));
        }
    }

    // ─── Public API ───────────────────────────────────────────────────────────

    public void StartQuiz(int planetIndex)
    {
        if (!planetQuestions.ContainsKey(planetIndex))
        {
            Debug.LogWarning($"No questions found for planet index {planetIndex}");
            return;
        }

        currentPlanetIndex    = planetIndex;
        currentQuestionIndex  = 0;
        sessionScore          = 0;
        currentQuestions      = planetQuestions[planetIndex];

        quizPanel.SetActive(true);
        DisplayQuestion();
    }

    // ─── Question Flow ────────────────────────────────────────────────────────

    private void DisplayQuestion()
    {
        if (currentQuestionIndex >= currentQuestions.Count)
        {
            ShowResults();
            return;
        }

        answerLocked = false;
        Question q = currentQuestions[currentQuestionIndex];

        questionText.text = q.questionText;
        progressText.text = $"Question {currentQuestionIndex + 1} of {currentQuestions.Count}";
        scoreText.text    = $"Score: {sessionScore}";
        feedbackText.text = "";

        // Populate and reset answer buttons
        for (int i = 0; i < answerButtons.Length; i++)
        {
            answerTexts[i].text = q.answers[i];
            SetButtonColor(answerButtons[i], defaultColor);
            answerButtons[i].interactable = true;
        }

        if (nextButton != null) nextButton.gameObject.SetActive(false);
        if (finishButton != null) finishButton.gameObject.SetActive(false);
    }

    private void OnAnswerSelected(int selectedIndex)
    {
        if (answerLocked) return;
        answerLocked = true;

        Question q = currentQuestions[currentQuestionIndex];
        bool correct = selectedIndex == q.correctIndex;

        // Color feedback
        SetButtonColor(answerButtons[selectedIndex], correct ? correctColor : wrongColor);
        if (!correct)
            SetButtonColor(answerButtons[q.correctIndex], correctColor); // Show correct answer

        // Disable all buttons
        foreach (var btn in answerButtons) btn.interactable = false;

        if (correct)
        {
            sessionScore += pointsPerCorrectAnswer;
            feedbackText.text = "✅ Correct! Great job!";
            feedbackText.color = correctColor;
            if (GameManager.Instance != null)
                GameManager.Instance.questionsAnsweredCorrectly++;
        }
        else
        {
            feedbackText.text = $"❌ Not quite! The answer was: {q.answers[q.correctIndex]}";
            feedbackText.color = wrongColor;
        }

        scoreText.text = $"Score: {sessionScore}";

        // Auto-advance after delay
        StartCoroutine(AutoAdvance());
    }

    private IEnumerator AutoAdvance()
    {
        yield return new WaitForSeconds(feedbackDelay);

        currentQuestionIndex++;

        if (currentQuestionIndex >= currentQuestions.Count)
            ShowResults();
        else
            DisplayQuestion();
    }

    private void NextQuestion()
    {
        currentQuestionIndex++;
        DisplayQuestion();
    }

    // ─── Results ──────────────────────────────────────────────────────────────

    private void ShowResults()
    {
        int total = currentQuestions.Count * pointsPerCorrectAnswer;
        questionText.text = $"🎉 Quiz Complete!\n\nYou scored {sessionScore} out of {total}!";

        string emoji = sessionScore == total ? "🌟 Perfect score!" :
                       sessionScore >= total / 2 ? "👍 Well done!" : "Keep exploring!";
        feedbackText.text = emoji;
        feedbackText.color = Color.white;

        // Hide answer buttons
        foreach (var btn in answerButtons) btn.gameObject.SetActive(false);

        if (nextButton != null) nextButton.gameObject.SetActive(false);
        if (finishButton != null) finishButton.gameObject.SetActive(true);

        // Register completion with GameManager
        if (GameManager.Instance != null)
            GameManager.Instance.CompletePlanet(currentPlanetIndex, sessionScore);
    }

    private void FinishQuiz()
    {
        quizPanel.SetActive(false);

        // Restore answer buttons for next time
        foreach (var btn in answerButtons) btn.gameObject.SetActive(true);

        // Return to solar system or trigger win screen
        if (GameManager.Instance != null)
            GameManager.Instance.CheckWinCondition();
    }

    // ─── Helpers ──────────────────────────────────────────────────────────────

    private void SetButtonColor(Button btn, Color color)
    {
        ColorBlock cb = btn.colors;
        cb.normalColor      = color;
        cb.highlightedColor = color * 1.1f;
        cb.pressedColor     = color * 0.9f;
        btn.colors = cb;
    }
}
