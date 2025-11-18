using UnityEngine;
using UnityEngine.AI;

public class SmoothAgentMovement : MonoBehaviour
{
    private NavMeshAgent agent;
    private Vector3 lastPosition;
    private Vector3 smoothVelocity;
    private bool isInitialized = false;

    [Header("Suavização Avançada")]
    public float positionSmoothTime = 0.1f;
    public float rotationSmoothTime = 0.05f;
    public bool useSmoothMovement = true;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        lastPosition = transform.position;

        // Configurações AGGRESSIVAS para movimento suave
        if (agent != null)
        {
            agent.acceleration = 200f;           // MUITO mais rápido
            agent.angularSpeed = 1080f;          // Giro instantâneo quase
            agent.autoBraking = false;           // CRÍTICO: sem paradas bruscas
            agent.autoRepath = true;
            agent.stoppingDistance = 0.1f;       // Para mais perto

            // Tenta evitar o "recalculando path"
            agent.obstacleAvoidanceType = ObstacleAvoidanceType.NoObstacleAvoidance;
        }

        isInitialized = true;
    }

    void Update()
    {
        if (!isInitialized || agent == null || !agent.enabled) return;

        // SUAVIZAÇÃO DE POSIÇÃO MANUAL
        if (useSmoothMovement && agent.hasPath)
        {
            // Interpolação suave entre posições
            transform.position = Vector3.SmoothDamp(
                transform.position,
                agent.nextPosition,
                ref smoothVelocity,
                positionSmoothTime
            );
        }

        // SUAVIZAÇÃO DE ROTAÇÃO MANUAL
        if (agent.velocity.sqrMagnitude > 0.1f)
        {
            Vector3 direction = agent.velocity.normalized;
            direction.y = 0;

            if (direction != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(
                    transform.rotation,
                    targetRotation,
                    rotationSmoothTime * Time.deltaTime * 10f
                );
            }
        }

        lastPosition = transform.position;
    }

    void OnDrawGizmos()
    {
        if (agent != null && agent.hasPath)
        {
            // Debug visual do path
            Gizmos.color = Color.red;
            for (int i = 0; i < agent.path.corners.Length - 1; i++)
            {
                Gizmos.DrawLine(agent.path.corners[i], agent.path.corners[i + 1]);
            }
        }
    }
}