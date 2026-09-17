using UnityEngine;

public class WandererNPC : MonoBehaviour
{
    [Header("Movement Settings")]
    public float maxSpeed = 4.0f;
    public float steeringForce = 8.0f;

    [Header("Wander Settings")]
    public float wanderRadius = 1.2f;
    public float wanderDistance = 2.0f;
    public float wanderJitter = 2.0f;

    [Header("Wall Avoidance Settings")]
    [Tooltip("Capa que representan las paredes/obstáculos.")]
    public LayerMask wallMask;
    [Tooltip("Distancia del raycast frontal.")]
    public float detectionDistance = 1.5f;
    [Tooltip("Ángulo de los raycasts diagonales (en grados).")]
    public float whiskerAngle = 30f;
    [Tooltip("Fuerza con la que rebota al detectar pared.")]
    public float avoidanceForce = 15.0f;

    private float wanderAngle;
    private Vector2 velocity;

    private void Start()
    {
        wanderAngle = Random.Range(0, 360f);
        velocity = transform.up * maxSpeed; // Empieza moviéndose hacia adelante
    }

    private void Update()
    {
        Vector2 currentDir = (velocity == Vector2.zero) ? (Vector2)transform.up : velocity.normalized;

        Vector2 avoidance = CalculateWallAvoidance(currentDir);

        Vector2 desiredSteering;

        if (avoidance != Vector2.zero)
        {

            desiredSteering = avoidance * avoidanceForce;
        }
        else
        {
            desiredSteering = CalculateWanderForce(currentDir);
        }
        velocity += desiredSteering * steeringForce * Time.deltaTime;

        if (velocity.magnitude > maxSpeed)
        {
            velocity = velocity.normalized * maxSpeed;
        }
        transform.Translate(velocity * Time.deltaTime, Space.World);
    }

    private Vector2 CalculateWanderForce(Vector2 currentDir)
    {
        Vector2 circleCenter = (Vector2)transform.position + currentDir * wanderDistance;

        wanderAngle += Random.Range(-wanderJitter, wanderJitter) * Time.deltaTime * Mathf.Deg2Rad;
        Vector2 displacement = new Vector2(Mathf.Cos(wanderAngle), Mathf.Sin(wanderAngle)) * wanderRadius;

        Vector2 targetPosition = circleCenter + displacement;
        return (targetPosition - (Vector2)transform.position).normalized;
    }

    private Vector2 CalculateWallAvoidance(Vector2 currentDir)
    {

        Vector2[] rayDirections = new Vector2[3];
        rayDirections[0] = currentDir; 
        rayDirections[1] = Quaternion.Euler(0, 0, whiskerAngle) * currentDir;  
        rayDirections[2] = Quaternion.Euler(0, 0, -whiskerAngle) * currentDir; /

        foreach (Vector2 dir in rayDirections)
        {
            RaycastHit2D hit = Physics2D.Raycast(transform.position, dir, detectionDistance, wallMask);

            if (hit.collider != null)
            {
                
                Vector2 reflectDir = Vector2.Reflect(dir, hit.normal);

                wanderAngle = Mathf.Atan2(reflectDir.y, reflectDir.x);

                return reflectDir.normalized;
            }
        }

        return Vector2.zero;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Vector2 currentDir = (velocity == Vector2.zero) ? (Vector2)transform.up : velocity.normalized;

        Gizmos.DrawRay(transform.position, currentDir * detectionDistance);
        Gizmos.DrawRay(transform.position, (Quaternion.Euler(0, 0, whiskerAngle) * currentDir) * detectionDistance);
        Gizmos.DrawRay(transform.position, (Quaternion.Euler(0, 0, -whiskerAngle) * currentDir) * detectionDistance);
    }
}
