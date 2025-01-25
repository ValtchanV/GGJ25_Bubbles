using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public GameObject[] enemyPrefabs;
    public float spawnInterval = 3f;
    public float spawnDistance = 5f;

    private Transform player;
    private float spawnTimer;

    private Vector2 lastPlayerPosition;
    private Vector2 lastMovementDirection = Vector2.left;

    void Start()
    {
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
        {
            player = playerObject.transform;
            lastPlayerPosition = player.position;
        }
        spawnTimer = spawnInterval;

        SpawnEnemy();
    }

    void Update()
    {
        if (player == null) return;

        Vector2 currentPlayerPosition = player.position;
        Vector2 movementDirection = (currentPlayerPosition - lastPlayerPosition).normalized;

        if (movementDirection != Vector2.zero)
        {
            lastMovementDirection = movementDirection;
        }

        lastPlayerPosition = currentPlayerPosition;

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

        int randomIndex = Random.Range(0, enemyPrefabs.Length);
        GameObject enemyPrefab = enemyPrefabs[randomIndex];

        Vector3 spawnPosition = player.position + (Vector3)lastMovementDirection.normalized * spawnDistance;

        spawnPosition += new Vector3(
            Random.Range(-0.5f, 0.5f),
            Random.Range(-0.5f, 0.5f),
            0f
        );

        Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);
    }
}