using UnityEngine;
using System.Collections;

public class WaveSpawner : MonoBehaviour
{
    // ===== WHAT TO SPAWN =====
    [Header("Spawn Settings")]
    public GameObject enemyPrefab;        // The skeleton prefab to spawn
    public Transform[] spawnPoints;       // Array of spawn locations
    public float spawnDelay = 1f;         // Delay between each enemy spawn (seconds)

    // ===== DIFFICULTY SCALING =====
    [Header("Difficulty Scaling")]
    public float healthMultiplierPerWave = 1.2f;  // Multiply enemy health by this each wave
    // Example: Wave 1 = 40 HP, Wave 2 = 48 HP, Wave 3 = 57.6 HP, etc.

    // ===== SPAWN MANAGEMENT =====
    private int enemiesSpawned = 0;
    private int enemiesToSpawn = 0;
    private int currentWaveNumber = 1;
    private bool isSpawning = false;

    // ===== PUBLIC METHODS =====

    public void SpawnWave(int numberOfEnemies, int waveNumber)
    {
        // Called by GameManager to start spawning a wave
        if (isSpawning) return;  // Don't start a new wave if already spawning

        // Validation checks
        if (enemyPrefab == null)
        {
            Debug.LogError("Enemy Prefab is not assigned to WaveSpawner!");
            return;
        }

        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            Debug.LogError("No spawn points assigned to WaveSpawner!");
            return;
        }

        // Set up wave
        enemiesToSpawn = numberOfEnemies;
        currentWaveNumber = waveNumber;
        enemiesSpawned = 0;

        // Start spawning enemies one by one
        StartCoroutine(SpawnEnemiesCoroutine());
    }

    // ===== SPAWNING LOGIC =====

    private IEnumerator SpawnEnemiesCoroutine()
    {
        isSpawning = true;

        // Spawn enemies one at a time with a delay
        while (enemiesSpawned < enemiesToSpawn)
        {
            SpawnEnemy();
            enemiesSpawned++;

            // Wait before spawning next enemy
            yield return new WaitForSeconds(spawnDelay);
        }

        isSpawning = false;
        Debug.Log("Wave " + currentWaveNumber + " spawn complete! Spawned " + enemiesSpawned + " enemies.");
    }

    void SpawnEnemy()
    {
        // Pick a random spawn point
        Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];

        // Spawn the enemy at that location
        GameObject enemy = Instantiate(enemyPrefab, spawnPoint.position, spawnPoint.rotation);

        // Scale enemy health based on wave number
        EnemyHealth enemyHealth = enemy.GetComponent<EnemyHealth>();
        if (enemyHealth != null)
        {
            // Calculate scaled health: base health * (multiplier ^ (wave - 1))
            // Wave 1: 40 * 1.2^0 = 40
            // Wave 2: 40 * 1.2^1 = 48
            // Wave 3: 40 * 1.2^2 = 57.6
            float scaledHealth = enemyHealth.maxHealth * Mathf.Pow(healthMultiplierPerWave, currentWaveNumber - 1);
            enemyHealth.maxHealth = scaledHealth;
        }

        Debug.Log("Spawned enemy " + (enemiesSpawned + 1) + "/" + enemiesToSpawn + " at " + spawnPoint.name);
    }

    // ===== HELPER METHODS =====

    public bool IsSpawning()
    {
        return isSpawning;
    }

    // ===== VISUAL HELPER (Shows spawn points in editor) =====

    void OnDrawGizmos()
    {
        // Draw spheres at spawn points in the editor for easy visualization
        if (spawnPoints != null)
        {
            Gizmos.color = Color.red;
            foreach (Transform spawnPoint in spawnPoints)
            {
                if (spawnPoint != null)
                {
                    Gizmos.DrawWireSphere(spawnPoint.position, 1f);
                }
            }
        }
    }
}
