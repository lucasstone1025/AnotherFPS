using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    // ===== SINGLETON =====
    public static UIManager Instance { get; private set; }

    // ===== WAVE DISPLAY (Bottom Left) =====
    [Header("Wave Display")]
    public TextMeshProUGUI waveText;  // Text showing "Wave 1", "Wave 2", etc.

    // ===== KILLS DISPLAY (Bottom Left, next to wave) =====
    [Header("Kills Display")]
    public TextMeshProUGUI killsText;  // Text showing kill count
    public Image killsIcon;            // Skull icon (optional)

    // ===== GAME OVER / VICTORY SCREENS =====
    [Header("Game Over Screen")]
    public GameObject gameOverPanel;   // Panel to show when player dies
    public TextMeshProUGUI gameOverText;  // "GAME OVER" text
    public TextMeshProUGUI finalStatsText;  // "Wave 3 | 15 Kills"

    [Header("Victory Screen")]
    public GameObject victoryPanel;    // Panel to show when player wins
    public TextMeshProUGUI victoryText;  // "VICTORY!" text
    public TextMeshProUGUI victoryStatsText;  // "You beat all 10 waves! Total Kills: 50"

    void Awake()
    {
        // Singleton setup
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // Hide game over and victory screens at start
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (victoryPanel != null) victoryPanel.SetActive(false);
    }

    void Update()
    {
        // Update UI every frame with current game state
        if (GameManager.Instance != null)
        {
            UpdateWaveDisplay();
            UpdateKillsDisplay();
        }
    }

    // ===== WAVE DISPLAY =====

    void UpdateWaveDisplay()
    {
        if (waveText != null)
        {
            int currentWave = GameManager.Instance.GetCurrentWave();
            waveText.text = "Wave " + currentWave;
        }
    }

    // ===== KILLS DISPLAY =====

    void UpdateKillsDisplay()
    {
        if (killsText != null)
        {
            int totalKills = GameManager.Instance.GetTotalKills();
            killsText.text = totalKills.ToString();
        }
    }

    // ===== GAME OVER SCREEN =====

    public void ShowGameOver()
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);

            if (gameOverText != null)
            {
                gameOverText.text = "GAME OVER";
            }

            if (finalStatsText != null && GameManager.Instance != null)
            {
                int wave = GameManager.Instance.GetCurrentWave();
                int kills = GameManager.Instance.GetTotalKills();
                finalStatsText.text = "Wave " + wave + " | " + kills + " Kills";
            }
        }
    }

    // ===== VICTORY SCREEN =====

    public void ShowVictory()
    {
        if (victoryPanel != null)
        {
            victoryPanel.SetActive(true);

            if (victoryText != null)
            {
                victoryText.text = "VICTORY!";
            }

            if (victoryStatsText != null && GameManager.Instance != null)
            {
                int waves = GameManager.Instance.maxWaves;
                int kills = GameManager.Instance.GetTotalKills();
                victoryStatsText.text = "You beat all " + waves + " waves!\nTotal Kills: " + kills;
            }
        }
    }
}
