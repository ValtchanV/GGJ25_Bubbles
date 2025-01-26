using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public GameObject[] enemyPrefabs;
    public float spawnInterval = 3f;
    public float spawnDistance = 5f;

    private float spawnTimer;

    void Start()
    {
        spawnTimer = spawnInterval;
    }

    void Update()
    {
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
    }
}