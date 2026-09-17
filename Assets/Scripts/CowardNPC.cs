using UnityEngine;

public class CowardNPC : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 3f;
    public float fleeSpeed = 5f;
    public float detectionRadius = 4f; 
    
    [Header("Wander Settings")]
    public float changeDirectionInterval = 2f;
    private float wanderTimer;
    private Vector2 wanderDirection;

    private Transform playerTransform;
    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
        }
        else
        {
            playerTransform = GameObject.Find("Player")?.transform;
        }
        WanderNewDirection();
    }
    void Update()
    {
        if (playerTransform == null) return;

        float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);

        if (distanceToPlayer <= detectionRadius)
        {
            Vector2 fleeDirection = (transform.position - playerTransform.position).normalized;
            rb.linearVelocity = fleeDirection * fleeSpeed;

            if (fleeDirection != Vector2.zero)
            {
                float angle = Mathf.Atan2(fleeDirection.y, fleeDirection.x) * Mathf.Rad2Deg;
            }
        }
        else
        {
            wanderTimer -= Time.deltaTime;
            if (wanderTimer <= 0f)
            {
                WanderNewDirection();
                wanderTimer = changeDirectionInterval;
            }
            rb.linearVelocity = wanderDirection * moveSpeed;
        }
    }

    void WanderNewDirection()
    {
        float randomAngle = Random.Range(0f, 360f);
        wanderDirection = new Vector2(Mathf.Cos(randomAngle * Mathf.Deg2Rad), Mathf.Sin(randomAngle * Mathf.Deg2Rad)).normalized;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}