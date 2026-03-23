using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// GameManager — Singleton that tracks game state, planet progress, and scene transitions.
/// Attach to an empty GameObject named "GameManager" in your Main Menu scene.
/// Mark it with DontDestroyOnLoad so it persists across scenes.
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Planet Progress")]
    public int totalPlanets = 8;
    private bool[] planetsCompleted;

    [Header("Player Score")]
    public int totalScore = 0;
    public int questionsAnsweredCorrectly = 0;

    [Header("Scene Names (must match Build Settings)")]
    public string mainMenuScene = "MainMenu";
    public string solarSystemScene = "SolarSystem";
    public string winScene = "WinScreen";

    private void Awake()
    {
        // Singleton pattern — only one GameManager exists across all scenes
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        planetsCompleted = new bool[totalPlanets];
    }

    // ─── Scene Navigation ────────────────────────────────────────────────────

    public void StartGame()
    {
        SceneManager.LoadScene(solarSystemScene);
    }

    public void GoToMainMenu()
    {
        ResetProgress();
        SceneManager.LoadScene(mainMenuScene);
    }

    public void LoadPlanetScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    public void ReturnToSolarSystem()
    {
        SceneManager.LoadScene(solarSystemScene);
    }

    public void CheckWinCondition()
    {
        if (AllPlanetsCompleted())
        {
            SceneManager.LoadScene(winScene);
        }
        else
        {
            ReturnToSolarSystem();
        }
    }

    // ─── Progress Tracking ───────────────────────────────────────────────────

    /// <summary>Call this when a player finishes a planet's quiz.</summary>
    public void CompletePlanet(int planetIndex, int scoreEarned)
    {
        if (planetIndex < 0 || planetIndex >= totalPlanets) return;

        planetsCompleted[planetIndex] = true;
        totalScore += scoreEarned;
        Debug.Log($"Planet {planetIndex} completed! Total score: {totalScore}");
    }

    public bool IsPlanetCompleted(int planetIndex)
    {
        if (planetIndex < 0 || planetIndex >= totalPlanets) return false;
        return planetsCompleted[planetIndex];
    }

    public bool AllPlanetsCompleted()
    {
        foreach (bool completed in planetsCompleted)
            if (!completed) return false;
        return true;
    }

    public int GetCompletedCount()
    {
        int count = 0;
        foreach (bool completed in planetsCompleted)
            if (completed) count++;
        return count;
    }

    private void ResetProgress()
    {
        planetsCompleted = new bool[totalPlanets];
        totalScore = 0;
        questionsAnsweredCorrectly = 0;
    }

    // ─── Score ───────────────────────────────────────────────────────────────

    public void AddScore(int points)
    {
        totalScore += points;
    }

    public string GetScoreSummary()
    {
        return $"Planets Explored: {GetCompletedCount()}/{totalPlanets}\nTotal Score: {totalScore}";
    }
}
