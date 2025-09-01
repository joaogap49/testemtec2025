using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class EnemyAttack : MonoBehaviour, IHitable
{
    [SerializeField] private float attackRange = 2f;
    [SerializeField] private float attackCooldown = 0.5f;
    [SerializeField] private int attackDamage = 10;
    [SerializeField] private float windAttackUp = 0.3f;

    private EnemyHealth enemyHealth;
    private EnemyBasicMovement movement;
    private Animator anim;
    private Transform player;
    private Rigidbody rb;

    public bool isAttacking;
    public bool isKnockBack;
    private float lastAttackTime = -999f;
    private int attackLayerIndex;

    private void Awake()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        anim = GetComponentInChildren<Animator>();
        movement = GetComponent<EnemyBasicMovement>();
        rb = GetComponent<Rigidbody>();
        enemyHealth = GetComponent<EnemyHealth>();

        attackLayerIndex = anim.GetLayerIndex("AttackLayer");

        if (rb != null)
        {
            rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
            rb.interpolation = RigidbodyInterpolation.Interpolate;
            rb.constraints = RigidbodyConstraints.FreezeRotation;
        }
    }

    private void Update()
    {
        if (isKnockBack || enemyHealth.currentHealth <= 0) return;

        if (movement.currentState != EnemyBasicMovement.EnemyState.Chase) return;

        float distance = Vector3.Distance(transform.position, player.position);

        if (distance <= attackRange && !isAttacking && Time.time >= lastAttackTime + attackCooldown)
        {
            StartCoroutine(PerformAttack());
            lastAttackTime = Time.time;
        }
    }

    private IEnumerator PerformAttack()
    {
        if (enemyHealth.currentHealth <= 0) yield break;

        isAttacking = true;
        movement.StopMovement();

        StartCoroutine(SmoothLayerTransition(1.0f, 0.1f));

        anim.SetTrigger("attack");
        anim.SetInteger("state", 3);

        yield return new WaitForSeconds(windAttackUp);

        Vector3 attackPosition = transform.position + transform.forward * 1.5f;
        Collider[] hits = Physics.OverlapSphere(attackPosition, attackRange);

        foreach (var hit in hits)
        {
            if (hit.CompareTag("Player"))
            {
                PlayerThird health = hit.GetComponent<PlayerThird>();
                if (health != null)
                {
                    health.TakeDamage(attackDamage);
                }
                break;
            }
        }

        yield return new WaitForSeconds(1f - windAttackUp);

        yield return StartCoroutine(SmoothLayerTransition(0f, 0.2f));
        isAttacking = false;
        movement.ResumeMovement();
    }

    private IEnumerator SmoothLayerTransition(float targetWeight, float duration)
    {
        float startWeight = anim.GetLayerWeight(attackLayerIndex);
        float timer = 0f;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            float currentWeight = Mathf.MoveTowards(startWeight, targetWeight, timer / duration);
            anim.SetLayerWeight(attackLayerIndex, currentWeight);
            yield return null;
        }
        anim.SetLayerWeight(attackLayerIndex, targetWeight);
    }

    public void Execute(Transform knockbackSource, bool isPlayerAttack)
    {
        if (isPlayerAttack)
            GetKnockback(knockbackSource);
    }

    public void GetKnockback(Transform knockbackSource)
    {
        if (isKnockBack) return;
        isKnockBack = true;

        if (movement.agent != null)
        {
            movement.agent.isStopped = true;
            movement.agent.enabled = false;
        }

        Vector3 direction = (transform.position - knockbackSource.position).normalized;
        direction.y = 0.3f;
        rb.AddForce(direction * 10f, ForceMode.Impulse);

        StartCoroutine(KnockbackEffect());
        StartCoroutine(EnableAgentAfterKnockBack());
    }

    private IEnumerator KnockbackEffect()
    {
        var renderer = GetComponentInChildren<Renderer>();
        var originalColor = renderer.material.color;
        renderer.material.color = Color.red;
        yield return new WaitForSeconds(0.5f);
        renderer.material.color = originalColor;
    }

    private IEnumerator EnableAgentAfterKnockBack()
    {
        yield return new WaitUntil(() => rb.velocity.magnitude < 1f);
        yield return new WaitForSeconds(0.2f);

        if (movement.agent != null)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;

            if (NavMesh.SamplePosition(transform.position, out NavMeshHit hit, 2.0f, NavMesh.AllAreas))
                movement.agent.Warp(hit.position);

            movement.agent.enabled = true;
            movement.agent.isStopped = false;
        }

        isKnockBack = false;
    }
}
