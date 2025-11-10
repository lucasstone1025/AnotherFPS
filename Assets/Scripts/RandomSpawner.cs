using UnityEngine;

public class RandomSpawner : MonoBehaviour
{
    public GameObject cubePrefab;
    public float spawnInterval = 5f;   // time between spawns
    public float clearRadius = 1.25f;  // how far the object must move from its spawn point to free the spot

    private float timer;

    // Five possible spawn positions (set here or expose publicly to edit in Inspector)
    private Vector3[] spawnPositions = new Vector3[5];

    // Track what’s currently at each spot (null = free)
    private GameObject[] occupants;

    void Start()
    {
        // Define your five spawn positions here (change as needed)
        spawnPositions[0] = new Vector3(-5f, 2f, -5f);
        spawnPositions[1] = new Vector3( 5f, 2f, -5f);
        spawnPositions[2] = new Vector3(-5f, 2f,  5f);
        spawnPositions[3] = new Vector3( 5f, 2f,  5f);
        spawnPositions[4] = new Vector3(  0f, 2f,   0f);

        occupants = new GameObject[spawnPositions.Length];
        timer = spawnInterval;
    }

    void Update()
    {
        // free any spots where the occupant moved away or was destroyed
        RefreshOccupancy();

        timer -= Time.deltaTime;
        if (timer <= 0f)
        {
            TrySpawn();
            timer = spawnInterval;
        }
    }

    void TrySpawn()
    {
        // collect indices of free spots
        int[] free = GetFreeIndices();
        if (free.Length == 0) return; // nothing free right now

        // choose one free spot at random
        int idx = free[Random.Range(0, free.Length)];

        // spawn
        GameObject obj = Instantiate(cubePrefab, spawnPositions[idx], Quaternion.identity);

        // ensure it can be pushed when shot
        if (!obj.TryGetComponent<Rigidbody>(out var rb))
            rb = obj.AddComponent<Rigidbody>();

        occupants[idx] = obj;
    }

    int[] GetFreeIndices()
    {
        int count = 0;
        for (int i = 0; i < occupants.Length; i++)
        {
            if (IsSpotFree(i)) count++;
        }
        int[] result = new int[count];
        int j = 0;
        for (int i = 0; i < occupants.Length; i++)
        {
            if (IsSpotFree(i)) result[j++] = i;
        }
        return result;
    }

    bool IsSpotFree(int i)
    {
        var obj = occupants[i];
        if (obj == null) return true; // destroyed → free

        // if the object moved beyond clearRadius from its spawn point, free the spot
        float d = Vector3.Distance(obj.transform.position, spawnPositions[i]);
        return d > clearRadius;
    }

    void RefreshOccupancy()
    {
        for (int i = 0; i < occupants.Length; i++)
        {
            var obj = occupants[i];
            if (obj == null) continue; // already free
            // if destroyed or moved out, mark free by nulling (so GetFreeIndices() sees it)
            if (Vector3.Distance(obj.transform.position, spawnPositions[i]) > clearRadius)
            {
                occupants[i] = null;
            }
        }
    }
}
