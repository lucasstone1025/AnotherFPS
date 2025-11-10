using UnityEngine;

public class RandomSpawner : MonoBehaviour
{
    public GameObject cubePrefab;
    public float spawnInterval = 5f; // time between spawns
    private float timer;

    // Five possible spawn positions
    private Vector3[] spawnPositions = new Vector3[5];

    void Start()
    {
        // Define your five spawn positions here (you can change these to fit your scene)
        spawnPositions[0] = new Vector3(-10f, 5f, -10f);
        spawnPositions[1] = new Vector3(10f, 5f, -10f);
        spawnPositions[2] = new Vector3(-10f, 5f, 10f);
        spawnPositions[3] = new Vector3(10f, 5f, 10f);
        spawnPositions[4] = new Vector3(0f, 5f, 0f);

        timer = spawnInterval;
    }

    void Update()
    {
        timer -= Time.deltaTime;

        if (timer <= 0f)
        {
            // Choose one of the 5 positions at random
            int index = Random.Range(0, spawnPositions.Length);

            Instantiate(cubePrefab, spawnPositions[index], Quaternion.identity);

            // Reset timer
            timer = spawnInterval;
        }
    }
}
