using UnityEngine;

public class EnemyAttack : MonoBehaviour
{
    public float speed = 6f;
    private Rigidbody2D rigidbody2D;
    private SpriteRenderer spriteRenderer;
    private Transform player;
    private bool hasTouchedIntestineWalls = false;

    void Awake()
    {
        rigidbody2D = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Start()
    {
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
        {
            player = playerObject.transform;
        }
    }

    void Update()
    {
        if (player.position.x < transform.position.x)
            spriteRenderer.flipX = false;
        else
            spriteRenderer.flipX = true;

        if (player == null || !hasTouchedIntestineWalls) return;

        Vector3 direction = (player.position - transform.position).normalized;
        rigidbody2D.MovePosition(transform.position + direction * speed * Time.deltaTime);

        speed += Time.deltaTime * 0.2f;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("IntestineWall"))
        {
            hasTouchedIntestineWalls = true;
        }

        if (collision.gameObject.CompareTag("Player"))
        {
            Debug.Log("Enemy attacked the player!");

            // PlayerHealth playerHealth = collision.gameObject.GetComponent<PlayerHealth>();
            // if (playerHealth != null)
            // {
            //     playerHealth.TakeDamage(10);
            // }

            Destroy(gameObject);
        }
    }
}