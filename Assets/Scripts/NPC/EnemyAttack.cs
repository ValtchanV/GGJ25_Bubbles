using UnityEngine;

public class EnemyAttack : MonoBehaviour
{
    public float speed = 30f;
    private Rigidbody2D rigidbody2D;
    private SpriteRenderer spriteRenderer;
    private Transform player;
    private GameManager gameManager;

    void Awake()
    {
        rigidbody2D = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        gameManager = GameManager.GetGameManager();
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
        if (player == null || !IsTouchingIntestineWall()) return;

        if (player.position.x < transform.position.x)
            spriteRenderer.flipX = false;
        else
            spriteRenderer.flipX = true;

        Vector3 direction = (player.position - transform.position).normalized;
        rigidbody2D.MovePosition(transform.position + direction * speed * Time.deltaTime);

        speed += Time.deltaTime * 0.5f;
    }

    private bool IsTouchingIntestineWall()
    {
        ContactPoint2D[] contacts = new ContactPoint2D[10];
        int contactCount = rigidbody2D.GetContacts(contacts);

        for (int i = 0; i < contactCount; i++)
        {
            if (contacts[i].collider.CompareTag("IntestineWall"))
            {
                return true;
            }
        }

        return false;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            Debug.Log("Enemy attacked the player!");
            gameManager.PlayerHitPoints--;
            Destroy(gameObject);
        }
    }
}