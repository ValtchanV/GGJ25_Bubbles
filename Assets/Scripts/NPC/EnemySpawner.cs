using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public GameObject[] enemyPrefabs; // Array of enemy prefabs to spawn
    public float spawnInterval = 3f;  // Time interval between spawns
    public float spawnDistance = 5f;  // Distance from the player to spawn enemies

    private Transform player; // Player's Transform
    private float spawnTimer;

    void Start()
    {
        // Find the player GameObject by its tag
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
        {
            player = playerObject.transform; // Cache the player's Transform
        }
        else
        {
            Debug.LogError("Player GameObject not found! Make sure the Player is tagged as 'Player'.");
        }

        spawnTimer = spawnInterval;
    }

    void Update()
    {
        if (player == null) return; // Exit if the player is not found

        spawnTimer -= Time.deltaTime;

        if (spawnTimer <= 0f)
        {
            SpawnEnemy();
            spawnTimer = spawnInterval;
        }
    }

    void SpawnEnemy()
    {
        if (enemyPrefabs.Length == 0) return;

        // Randomly select an enemy prefab
        int randomIndex = Random.Range(0, enemyPrefabs.Length);
        GameObject enemyPrefab = enemyPrefabs[randomIndex];

        // Calculate spawn position dynamically near the player
        Vector3 playerForwardDirection = player.transform.right; // Assuming 2D: right direction is forward
        Vector3 spawnPosition = player.position + playerForwardDirection.normalized * spawnDistance;

        // Add randomness to the spawn position to avoid fixed points
        spawnPosition += new Vector3(
            Random.Range(-1f, 1f), // Random horizontal offset
            Random.Range(-1f, 1f), // Random vertical offset
            0f
        );

        // Instantiate the enemy at the calculated position
        Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);
    }
}