using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public GameObject[] enemyPrefabs;
    public float spawnInterval = 5f;
    public float spawnDistance = 0f;
    public int maxEnemies = 10;

    private float spawnTimer;
    private int currentEnemyCount = 0;

    void Start()
    {
        spawnTimer = spawnInterval;
    }

    void Update()
    {
        if (currentEnemyCount >= maxEnemies) return;

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

        Vector3 spawnPosition = transform.position + new Vector3(
            Random.Range(-spawnDistance, spawnDistance),
            Random.Range(-spawnDistance, spawnDistance),
            0f
        );

        Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);
        currentEnemyCount++;
    }

    public void OnEnemyDestroyed()
    {
        currentEnemyCount--;
    }
}