    using System.Collections;
using System.Collections.Generic;
using UnityEditor.ShaderGraph.Internal;
using UnityEngine;
using UnityEngine.AI;

// Script responsável pelo ataque do inimigo ao jogador.
public class EnemyAttack : MonoBehaviour, IHitable
{

    float attackRange = 2f;

    EnemyHealth enemyHealth;

    public bool isKnockBack;


    float attackCooldown = 0.5f;


    public int attackDamage;

 
    float windAttackUp = 0.3f;

   
    public bool isAttacking;


    private Transform player;

    private Animator anim;

   
    private float lastAttackTime = -999f;

   
    public Transform attackPoint;


    private int attackLayerIndex;

    private EnemyBasicMovement movement;

    public Rigidbody rb;

    
    void Awake()
    {
        
        player = GameObject.FindGameObjectWithTag("Player").transform;
        
        anim = GetComponentInChildren<Animator>();
        
        movement = GetComponent<EnemyBasicMovement>();
        
        attackLayerIndex = anim.GetLayerIndex("AttackLayer");

        rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
            rb.interpolation = RigidbodyInterpolation.Interpolate;
            rb.constraints = RigidbodyConstraints.FreezeRotation;
        }


        enemyHealth = GetComponent<EnemyHealth>();
    }

    
    void Update()
    {
        if (isKnockBack) return;
        
        if (movement.currentState != EnemyBasicMovement.EnemyState.Chase)
            return;

        
        float distance = Vector3.Distance(transform.position, player.position);
        
        bool isInAttackAnim = anim.GetCurrentAnimatorStateInfo(0).IsName("attack");

        
        if (distance <= attackRange && !isAttacking && Time.time >= lastAttackTime + attackCooldown)
        {
            StartCoroutine(PerformAttack());
            lastAttackTime = Time.time;
        }
    }

    
    IEnumerator PerformAttack()
    {

        if (enemyHealth.currentHealth > 0)
        {
            isAttacking = true;
            movement.StopMovement();
            
            StartCoroutine(SmoothLayerTransition(1.0f, 0.1f));
            
            anim.SetTrigger("attack");
            anim.SetInteger("state", 3);
            
            yield return new WaitForSeconds(windAttackUp);

            
            Vector3 attackPosition = transform.position + transform.forward * 1.5f;
            
            Collider[] hits = Physics.OverlapSphere(attackPosition, attackRange);

            Debug.Log("Colisores detectados: " + hits.Length);

            
            foreach (var hit in hits)
            {
                if (hit.CompareTag("Player"))
                {

                    Debug.Log("Acertou o jogador!");
                    // Busca o script de vida do jogador e aplica dano.
                    PlayerThird health = hit.GetComponent<PlayerThird>();
                    if (health != null)
                    {
                        health.TakeDamage(attackDamage);


                    }
                    break; 
                }
            }

            
            float attackDuration = 1 - windAttackUp;
            yield return new WaitForSeconds(attackDuration);

            
            yield return StartCoroutine(SmoothLayerTransition(0f, 0.2f));
            //anim.SetInteger("state", 4);
            isAttacking = false;
            movement.ResumeMovement();
        }

    }

    
    void OnAnimatorIK(int layerIndex)
    {
        
        if (layerIndex == attackLayerIndex && anim.GetLayerWeight(attackLayerIndex) > 0.5f)
        {
            
            anim.SetIKPosition(AvatarIKGoal.RightHand, player.position + Vector3.up * 1.5f);
            anim.SetIKPositionWeight(AvatarIKGoal.RightHand, 0.8f);
        }
    }

    
    IEnumerator SmoothLayerTransition(float targetWeight, float duration)
    {
        float startWeight = anim.GetLayerWeight(attackLayerIndex);
        float currentWeight = startWeight;
        float timer = 0f;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            currentWeight = Mathf.MoveTowards(currentWeight, targetWeight, Time.deltaTime / duration);
            anim.SetLayerWeight(attackLayerIndex, currentWeight);
            yield return null;
        }
        anim.SetLayerWeight(attackLayerIndex, targetWeight);
    }

    
    float GetAttackDuration()
    {
        AnimationClip[] clips = anim.runtimeAnimatorController.animationClips;
        foreach (AnimationClip clip in clips)
        {
            if (clip.name == "Cross Punch")
            {
                return clip.length;
            }
        }
        
        return 1f;
    }

    public void Execute(Transform knockbackSource, bool isPlayerAttack)
    {
        if (isPlayerAttack)
            GetKnockback(knockbackSource);
        Debug.Log("Gaaay");
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
        float forceMultiplier = 10f; 
        rb.AddForce(direction * forceMultiplier, ForceMode.Impulse);
        StartCoroutine(KnockbackEffect());
        StartCoroutine(EnableAgentAfterKnockBack());
    }

    IEnumerator KnockbackEffect()
    {
        var originalColor = GetComponentInChildren<Renderer>().material.color;
        GetComponentInChildren<Renderer>().material.color = Color.red;
        yield return new WaitForSeconds(0.5f);
        GetComponentInChildren<Renderer>().material.color = originalColor;
    }

    IEnumerator EnableAgentAfterKnockBack()
    {
        yield return new WaitUntil(() => rb.velocity.magnitude < 1f);
        yield return new WaitForSeconds(0.2f);

        if (movement.agent != null)
        {
            
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;

            
            if (NavMesh.SamplePosition(transform.position, out NavMeshHit hit, 2.0f, NavMesh.AllAreas))
            {
                movement.agent.Warp(hit.position);
            }
            else if (NavMesh.FindClosestEdge(transform.position, out hit, NavMesh.AllAreas))
            {
                movement.agent.Warp(hit.position);
            }
            else
            {
                movement.agent.Warp(transform.position);
            }

            
            movement.agent.enabled = true;
            movement.agent.isStopped = false;
        }

        isKnockBack = false;
    }
}