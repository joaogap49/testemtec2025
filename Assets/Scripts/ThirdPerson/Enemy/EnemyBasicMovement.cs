using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyBasicMovement : MonoBehaviour
{
    public enum EnemyState { Patrol, Chase, Searching }
    public EnemyAttack enemyAttack;
    public EnemyState currentState = EnemyState.Patrol;

    public Transform[] patrolPoints;
    private int patrolIndex = 0;

    public Transform target;
    public float chaseDistance = 15f;
    public float lostTargetTime = 5f;
    private Vector3 lastSeenPosition;
    public Rigidbody rb;

    private float timerSinceLost = 0f;

    public float updateSpeed = 0.1f;
    private WaitForSeconds wait;
    public PlayerThird player;

    private GameObject zona;
    public NavMeshAgent agent;
    private Animator anim;

    private float viewRadius = 10f;
    private float viewAngle = 200f;
    public LayerMask playerMask;
    public LayerMask obstructionMask;

    private bool playerInSight = false;
    private bool hasReachedLastPosition = false;

    // ⚠️️ ADIÇÃO FUNDAMENTAL CONTRA JITTER
    private float turnSmoothSpeed = 8f; // valor alto para suavidade forte

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        player = GameObject.FindGameObjectWithTag("Player")?.GetComponent<PlayerThird>();
        if (player != null)
        {
            target = player.transform;
        }

        zona = GameObject.FindGameObjectWithTag("Zona");
        anim = GetComponentInChildren<Animator>();
        wait = new WaitForSeconds(updateSpeed);
        rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.interpolation = RigidbodyInterpolation.Interpolate;
            rb.collisionDetectionMode = CollisionDetectionMode.Discrete;
            rb.constraints = RigidbodyConstraints.FreezeRotation;
        }

        // ⚠️ AJUSTE NAVMESH QUE REMOVE JITTER DE ROTAÇÃO
        agent.updateRotation = false;
    }

    void Start()
    {
        StartCoroutine(StateMachine());
    }

    private IEnumerator StateMachine()
    {
        while (enabled)
        {
            switch (currentState)
            {
                case EnemyState.Chase:
                    if (!enemyAttack.isAttacking)
                    {
                        anim.SetInteger("state", 1);
                    }
                    Chase();
                    break;
            }
            yield return wait;
        }
    }

    private IEnumerator SearchAroundBeforePatrol()
    {
        yield return new WaitForSeconds(2f);
        currentState = EnemyState.Patrol;
    }

    void Update()
    {
        if (enemyAttack.isKnockBack || enemyAttack.isStunned)
        {
            if (agent != null && agent.enabled)
            {
                agent.isStopped = true;
            }
            return;
        }

        float distanceToPlayer = Vector3.Distance(transform.position, target.position);

        if (EnemyCanSeePlayer())
        {
            playerInSight = true;
            currentState = EnemyState.Chase;
            timerSinceLost = 0f;

            lastSeenPosition = target.position;
            hasReachedLastPosition = false;
        }

        // ⚠️ ROTATION SMOOTHING DO INIMIGO (Remove jitter visual)
        if (agent.velocity.sqrMagnitude > 0.1f)
        {
            Vector3 dir = agent.velocity.normalized;
            dir.y = 0;

            if (dir != Vector3.zero)
            {
                Quaternion targetRot = Quaternion.LookRotation(dir);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * turnSmoothSpeed);
            }
        }
    }

    void Chase()
    {
        if (enemyAttack.isKnockBack || enemyAttack.isStunned || target == null)
        {
            if (agent != null && agent.enabled)
            {
                agent.isStopped = true;
            }
            return;
        }

        if (!agent.isActiveAndEnabled) return;

        float distanceToCurrentDestination = Vector3.Distance(agent.destination, target.position);

        // Só atualiza destino quando realmente necessário
        if (distanceToCurrentDestination > 2.0f)
        {
            agent.SetDestination(target.position);
        }

        if (IsPlayerInZona())
        {
            agent.speed = player.moveSpeed + 1f;
        }
        else
        {
            agent.speed = player.SprintSpeed + 3f;
        }

        agent.isStopped = false;

        if (enemyAttack.isAttacking)
        {
            agent.speed = Mathf.Min(agent.speed, 2f);
        }
    }

    public bool IsPlayerInZona()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, 2f);
        foreach (var col in colliders)
        {
            if (col.CompareTag("Zona"))
                return true;
        }
        return false;
    }

    bool EnemyCanSeePlayer()
    {
        Vector3 directionToPlayer = (target.position - transform.position).normalized;
        float distanceToPlayer = Vector3.Distance(transform.position, target.position);

        if (distanceToPlayer < viewRadius)
        {
            float angleBetween = Vector3.Angle(transform.forward, directionToPlayer);

            if (angleBetween < viewAngle / 2f)
            {
                if (!Physics.Raycast(transform.position + Vector3.up, directionToPlayer, distanceToPlayer, obstructionMask))
                {
                    return true;
                }
            }
        }
        return false;
    }

    public void StopMovement()
    {
        if (agent != null)
        {
            agent.isStopped = true;
        }
    }

    public void ResumeMovement()
    {
        if (agent != null && agent.enabled)
        {
            agent.isStopped = false;
        }
    }
}
