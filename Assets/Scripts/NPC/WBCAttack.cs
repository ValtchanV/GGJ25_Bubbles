using UnityEngine;

public class WBCAttack : MonoBehaviour
{
    public float speed = 50f;
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
        if (player == null) return;

        Vector3 direction = (player.position - transform.position).normalized;
        rigidbody2D.MovePosition(transform.position + direction * speed * Time.deltaTime);

        speed += Time.deltaTime * 0.5f;
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