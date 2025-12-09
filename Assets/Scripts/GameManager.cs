using UnityEngine;

public class GameManager : MonoBehaviour
{
    // ===== SINGLETON PATTERN =====
    // This makes GameManager accessible from anywhere: GameManager.Instance.whatever
    public static GameManager Instance { get; private set; }

    // ===== GAME STATE =====
    // What state is the game currently in?
    public enum GameState
    {
        Playing,    // Game is active, player can shoot and move
        Paused,     // Game is paused (ESC menu)
        GameOver,   // Player died
        Victory     // Player won (beat all waves)
    }

    public GameState currentState = GameState.Playing;

    // ===== WAVE SYSTEM =====
    [Header("Wave Settings")]
    public int currentWave = 1;           // What wave are we on?
    public int maxWaves = 10;             // How many waves to win the game?
    public int enemiesPerWave = 5;        // Base number of enemies per wave
    public float enemiesIncreasePerWave = 2f;  // Add this many enemies each wave

    // ===== KILL TRACKING =====
    [Header("Kill Tracking")]
    public int totalKills = 0;            // Total kills across all waves
    public int killsThisWave = 0;         // Kills in current wave
    public int enemiesRemainingThisWave = 0;  // How many enemies left to kill

    // ===== POINT TRACKING =====
    public int points = 0;

    // ===== WAVE COMPLETION TRACKING =====
    private bool isWaveCompleting = false;  // Prevents WaveComplete from being called multiple times

    // ===== REFERENCES =====
    [Header("References")]
    public WaveSpawner waveSpawner;       // Reference to the spawner script

    void Awake()
    {
        // SINGLETON SETUP: Only one GameManager can exist
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    void Start()
    {
        // Start the first wave when the game begins
        StartWave(currentWave);
    }

    void Update()
    {
        // Check for pause input (ESC key)
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause();
        }

        // Only update game logic if we're playing
        if (currentState == GameState.Playing)
        {
            // Check if wave is complete
            if (enemiesRemainingThisWave <= 0 && killsThisWave > 0)
            {
                WaveComplete();
            }
        }
    }

    // ===== WAVE MANAGEMENT =====

    public void StartWave(int waveNumber)
    {
        currentWave = waveNumber;
        killsThisWave = 0;
        isWaveCompleting = false;  // Reset the flag for the new wave

        // Calculate how many enemies for this wave
        // Example: Wave 1 = 5, Wave 2 = 7, Wave 3 = 9, etc.
        int enemiesToSpawn = Mathf.RoundToInt(enemiesPerWave + (waveNumber - 1) * enemiesIncreasePerWave);
        enemiesRemainingThisWave = enemiesToSpawn;

        Debug.Log("=== WAVE " + waveNumber + " START === Enemies to defeat: " + enemiesToSpawn);

        // Tell the spawner to spawn enemies
        if (waveSpawner != null)
        {
            waveSpawner.SpawnWave(enemiesToSpawn, waveNumber);
        }
    }

    void WaveComplete()
    {
        // Prevent this from being called multiple times per wave
        if (isWaveCompleting) return;
        isWaveCompleting = true;
        PointsTracker.instance.AddWavePoints();

        Debug.Log("=== WAVE " + currentWave + " COMPLETE ===");

        // Clean up all dead skeleton bodies
        CleanupDeadEnemies();

        // Check if we've beaten all waves
        if (currentWave >= maxWaves)
        {
            Victory();
        }
        else
        {
            // Move to next wave after a short delay
            Invoke("StartNextWave", 3f);  // 3 second delay between waves
        }
    }

    void StartNextWave()
    {
        StartWave(currentWave + 1);
    }

    void CleanupDeadEnemies()
    {
        // Find all game objects in the scene
        GameObject[] allObjects = GameObject.FindObjectsOfType<GameObject>();
        int cleanedUp = 0;

        foreach (GameObject obj in allObjects)
        {
            // Check if this is a dead skeleton (has "skeleton" in name and is untagged)
            // OR if it still has the Enemy tag (leftover alive enemies)
            if (obj.name.ToLower().Contains("skeleton") && obj.tag == "Untagged")
            {
                Destroy(obj);
                cleanedUp++;
            }
        }

        Debug.Log("Cleaned up " + cleanedUp + " dead skeleton bodies");
    }

    // ===== KILL TRACKING =====

    public void RegisterKill()
    {
        // Called when an enemy dies (EnemyHealth will call this)
        killsThisWave++;
        totalKills++;
        enemiesRemainingThisWave--;
        PointsTracker.instance.AddKillPoints();

        Debug.Log("Enemy killed! Kills this wave: " + killsThisWave + " | Remaining: " + enemiesRemainingThisWave);
    }

    // ===== GAME STATE MANAGEMENT =====

    void TogglePause()
    {
        if (currentState == GameState.Playing)
        {
            Pause();
        }
        else if (currentState == GameState.Paused)
        {
            Resume();
        }
    }

    public void Pause()
    {
        currentState = GameState.Paused;
        Time.timeScale = 0f;  // Freeze the game
        Debug.Log("Game Paused");
        // TODO: Show pause menu UI
    }

    public void Resume()
    {
        currentState = GameState.Playing;
        Time.timeScale = 1f;  // Unfreeze the game
        Debug.Log("Game Resumed");
        // TODO: Hide pause menu UI
    }

    public void GameOver()
    {
        if (currentState == GameState.GameOver) return;  // Already game over

        currentState = GameState.GameOver;
        Time.timeScale = 0f;  // Freeze the game
        Debug.Log("=== GAME OVER === Total Kills: " + totalKills);

        // Show game over screen
        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowGameOver();
        }
    }

    public void Victory()
    {
        if (currentState == GameState.Victory) return;  // Already won

        currentState = GameState.Victory;
        Time.timeScale = 1f;  // Keep time running for victory animations
        Debug.Log("=== VICTORY! === You beat all " + maxWaves + " waves! Total Kills: " + totalKills);

        // Show victory screen
        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowVictory();
        }
    }

    // ===== PUBLIC GETTERS (for UI to access) =====

    public int GetCurrentWave()
    {
        return currentWave;
    }

    public int GetTotalKills()
    {
        return totalKills;
    }

    public int GetKillsThisWave()
    {
        return killsThisWave;
    }

    public int GetEnemiesRemaining()
    {
        return enemiesRemainingThisWave;
    }

    public bool IsPlaying()
    {
        return currentState == GameState.Playing;
    }
}
