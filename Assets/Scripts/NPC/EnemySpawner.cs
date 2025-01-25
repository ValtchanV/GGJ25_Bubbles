using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public GameObject[] enemyPrefabs;
    public float spawnInterval = 3f;
    public float spawnDistance = 5f;
    public float activationRange = 30f; // Distance within which the spawner will activate

    private Transform player;
    private float spawnTimer;

    void Start()
    {
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
        {
            player = playerObject.transform;
        }
        else
        {
            Debug.LogError("Player GameObject not found. Make sure the player is tagged 'Player'.");
        }

        spawnTimer = spawnInterval;

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        Debug.Log(distanceToPlayer);
    }

    void Update()
    {
        if (player == null) return;

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if (distanceToPlayer <= activationRange)
        {
            spawnTimer -= Time.deltaTime;

            if (spawnTimer <= 0f)
            {
                SpawnEnemy();
                spawnTimer = spawnInterval;
            }
        }
    }

    void SpawnEnemy()
    {
        if (enemyPrefabs.Length == 0) return;

        int randomIndex = Random.Range(0, enemyPrefabs.Length);
        GameObject enemyPrefab = enemyPrefabs[randomIndex];

        Vector3 spawnPosition = transform.position + new Vector3(
            Random.Range(-spawnDistance, spawnDistance),
            Random.Range(-spawnDistance, spawnDistance),
            0f
        );

        Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);
    }
}