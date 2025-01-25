using UnityEngine;

public class RandomEnemySize : MonoBehaviour
{
    public float minScaleFactor = 0.3f;
    public float maxScaleFactor = 0.6f;

    void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            float playerSize = player.transform.localScale.x;

            float randomScaleFactor = Random.Range(minScaleFactor, maxScaleFactor);
            float enemySize = playerSize * randomScaleFactor;

            transform.localScale = new Vector3(enemySize, enemySize, 1);
        }
        else
        {
            Debug.LogError("Player GameObject not found. Ensure the player is tagged as 'Player'.");
        }
    }
}
