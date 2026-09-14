using UnityEngine;

public class ChaserNPC : MonoBehaviour
{
    [Header("Configuración General")]
    public Transform playerTransform;
    public float maxSpeed = 5f;
    public float maxForce = 10f;
    public float detectionRadius = 6f;
    public float arriveRadius = 1.5f;

    [Header("Wander Settings")]
    public float wanderCircleDistance = 2f;
    public float wanderCircleRadius = 1f;
    public float wanderJitter = 1f;

    [Header("Detección de Paredes")]
    public LayerMask wallLayer;
    public float wallCheckDistance = 1.2f;
    public float wallAvoidanceWeight = 2.5f;
    public float sideRayAngle = 35f;

    private Vector2 currentVelocity;
    private Vector2 wanderTarget;
    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        float angle = Random.value * Mathf.PI * 2;
        wanderTarget = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * wanderCircleRadius;
    }

    private void FixedUpdate()
    {
        float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);
        Vector2 steeringForce = Vector2.zero;


        Vector2 wallAvoidanceForce = AvoidWalls();

        if (wallAvoidanceForce != Vector2.zero)
        {

            steeringForce = wallAvoidanceForce * wallAvoidanceWeight;

            currentVelocity *= 0.5f;
        }
        else
        {

            if (distanceToPlayer <= detectionRadius)
            {
                if (distanceToPlayer <= arriveRadius)
                {
                    steeringForce = Arrive(playerTransform.position);
                }
                else
                {
                    steeringForce = Seek(playerTransform.position);
                }
            }
            else
            {
                steeringForce = Wander();
            }
        }

        ApplySteering(steeringForce);
    }

    private Vector2 Seek(Vector2 target)
    {
        Vector2 desiredVelocity = (target - (Vector2)transform.position).normalized;
        return desiredVelocity - currentVelocity;
    }

    private Vector2 Arrive(Vector2 target)
    {
        Vector2 targetOffset = target - (Vector2)transform.position;
        float distance = targetOffset.magnitude;
        float rampedSpeed = maxSpeed * (distance / arriveRadius);
        float clippedSpeed = Mathf.Min(rampedSpeed, maxSpeed);

        Vector2 desiredVelocity = (distance > 0.001f) ? (target / distance) * clippedSpeed : Vector2.zero;
        return desiredVelocity - currentVelocity;
    }

    private Vector2 Wander()
    {

        wanderTarget += new Vector2(
            Random.Range(-1f, 1f) * wanderJitter,
            Random.Range(-1f, 1f) * wanderJitter
        );
        wanderTarget.Normalize();
        wanderTarget *= wanderCircleRadius;


        Vector2 circleCenter = (Vector2)transform.position + currentVelocity.normalized * wanderCircleDistance;
        Vector2 worldTarget = circleCenter + wanderTarget;

        return Seek(worldTarget);
    }

    private void ApplySteering(Vector2 force)
    {

        force = Vector2.ClampMagnitude(force, maxForce);

        currentVelocity = Vector2.ClampMagnitude(currentVelocity + force * Time.fixedDeltaTime, maxSpeed);
        rb.linearVelocity = currentVelocity;
    }

    private void OnDrawGizmosSelected()
    {

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, arriveRadius);


        if (Application.isPlaying && currentVelocity.sqrMagnitude > 0.01f)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(transform.position, (Vector2)transform.position + currentVelocity.normalized * wallCheckDistance);
        }

    }

    private Vector2 AvoidWalls()
    {
        if (currentVelocity.sqrMagnitude < 0.01f) return Vector2.zero;

        Vector2 moveDir = currentVelocity.normalized;

        Vector2[] rayDirections = new Vector2[3];
        rayDirections[0] = moveDir;
        rayDirections[1] = Quaternion.Euler(0, 0, sideRayAngle) * moveDir;
        rayDirections[2] = Quaternion.Euler(0, 0, -sideRayAngle) * moveDir;

        foreach (Vector2 dir in rayDirections)
        {
            RaycastHit2D hit = Physics2D.Raycast(transform.position, dir, wallCheckDistance, wallLayer);

            if (hit.collider != null)
            {

                Vector2 reflectDir = Vector2.Reflect(moveDir, hit.normal);

                Vector2 desiredVelocity = reflectDir.normalized * maxSpeed;

                return desiredVelocity - currentVelocity;
            }
        }

        return Vector2.zero;
    }

}
