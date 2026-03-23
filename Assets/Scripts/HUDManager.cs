using UnityEngine;
using TMPro;
using UnityEngine.UI;

/// <summary>
/// HUDManager — Controls the in-game heads-up display.
/// Shows: speed, boost cooldown, planets completed, proximity hint ("Press E").
/// Attach to an empty GameObject in your SolarSystem scene.
/// </summary>
public class HUDManager : MonoBehaviour
{
    [Header("HUD Text Elements")]
    public TextMeshProUGUI speedText;
    public TextMeshProUGUI boostText;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI progressText;        // "Planets: 3/8"
    public TextMeshProUGUI proximityHintText;   // "Press E to explore Mars"
    public TextMeshProUGUI planetNameBanner;    // Big text that fades in/out

    [Header("Boost Bar")]
    public Image boostBarFill;                  // Image with Fill Amount

    [Header("References")]
    public ShipController ship;

    // Internal
    private float bannerTimer = 0f;
    private string currentNearbyPlanet = "";

    private void Update()
    {
        UpdateShipHUD();
        UpdateProgressHUD();
        UpdateBanner();
    }

    // ─── Ship Stats ───────────────────────────────────────────────────────────

    private void UpdateShipHUD()
    {
        if (ship == null) return;

        if (speedText != null)
            speedText.text = $"Speed: {ship.CurrentSpeed():F0}";

        if (boostText != null)
        {
            if (ship.IsBoostReady())
                boostText.text = "BOOST: Ready ✅";
            else
                boostText.text = $"BOOST: {ship.BoostCooldownRemaining():F1}s";
        }

        if (boostBarFill != null)
        {
            float fill = ship.IsBoostReady() ? 1f :
                         1f - (ship.BoostCooldownRemaining() / 5f); // 5 = boostCooldown
            boostBarFill.fillAmount = Mathf.Clamp01(fill);
        }
    }

    // ─── Progress ─────────────────────────────────────────────────────────────

    private void UpdateProgressHUD()
    {
        if (GameManager.Instance == null) return;

        int completed = GameManager.Instance.GetCompletedCount();
        int total     = GameManager.Instance.totalPlanets;

        if (progressText != null)
            progressText.text = $"🪐 {completed}/{total} Planets";

        if (scoreText != null)
            scoreText.text = $"⭐ {GameManager.Instance.totalScore}";
    }

    // ─── Proximity Hint ───────────────────────────────────────────────────────

    public void ShowProximityHint(string planetName)
    {
        currentNearbyPlanet = planetName;
        if (proximityHintText != null)
        {
            proximityHintText.text = $"Press E to explore {planetName}";
            proximityHintText.gameObject.SetActive(true);
        }
    }

    public void HideProximityHint()
    {
        currentNearbyPlanet = "";
        if (proximityHintText != null)
            proximityHintText.gameObject.SetActive(false);
    }

    // ─── Planet Name Banner ───────────────────────────────────────────────────

    public void ShowPlanetBanner(string planetName, float duration = 2.5f)
    {
        if (planetNameBanner == null) return;
        planetNameBanner.text = planetName;
        planetNameBanner.gameObject.SetActive(true);
        bannerTimer = duration;
    }

    private void UpdateBanner()
    {
        if (planetNameBanner == null || !planetNameBanner.gameObject.activeSelf) return;

        bannerTimer -= Time.deltaTime;

        // Fade out in last 0.5s
        float alpha = Mathf.Clamp01(bannerTimer / 0.5f);
        Color c = planetNameBanner.color;
        c.a = alpha;
        planetNameBanner.color = c;

        if (bannerTimer <= 0f)
            planetNameBanner.gameObject.SetActive(false);
    }
}
