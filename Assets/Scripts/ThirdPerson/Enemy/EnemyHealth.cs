using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class EnemyHealth : MonoBehaviour
{
    [SerializeField] private float maxHealth = 30f;
    public float currentHealth;

    private Animator animator;
    public EnemyBasicMovement enemyBasicMovement;
    public EnemyAttack enemyAttack;

    public delegate void EnemyDeath(EnemyHealth enemy);
    public static event EnemyDeath OnEnemyDeath;

    private void Awake()
    {
        animator = GetComponentInChildren<Animator>();
        enemyBasicMovement = GetComponent<EnemyBasicMovement>();
        enemyAttack = GetComponent<EnemyAttack>();
    }

    private void OnEnable()
    {
        // Sempre que o inimigo for ativado (spawn novo ou respawn), vida é resetada
        currentHealth = maxHealth;
    }

    public void TakeDamage(float damage)
    {
        if (currentHealth <= 0) return;

        currentHealth -= damage;

        // Animação de reação
        int randomReaction = Random.Range(1, 3);
        animator.SetInteger("attackReceiver", randomReaction);
        animator.SetTrigger("attacked");

        if (currentHealth <= 0)
        {
            Die();
            OnEnemyDeath?.Invoke(this);
        }
    }

    private void Die()
    {
        enemyBasicMovement.enabled = false;
        if (enemyAttack != null)
            enemyAttack.enabled = false;

        // Desativa colisores e Rigidbody para o corpo não atrapalhar
        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = false;

        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null) rb.isKinematic = true;

        animator.ResetTrigger("attacked");
        animator.SetTrigger("death");

    }

    private IEnumerator DisableAfterDeath()
    {
        yield return new WaitForSeconds(3f); // tempo da animação de morte
        gameObject.SetActive(false);
    }

    public void Respawn(Vector3 position)
    {
        NavMeshAgent agent = GetComponent<NavMeshAgent>();
        if (agent != null)
        {
            agent.enabled = true;
            agent.ResetPath();
        }
        currentHealth = maxHealth;
        
        transform.position = position;

        // Reativa scripts
        enemyBasicMovement.enabled = true;
        if (enemyAttack != null)
        {
            enemyAttack.enabled = true;
            enemyAttack.isKnockBack = false;
        }
            

        // Reativa colisor e Rigidbody
        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = true;

        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null) rb.isKinematic = false;

        animator.ResetTrigger("death");
        //animator.Play("Idle");
    }
}
