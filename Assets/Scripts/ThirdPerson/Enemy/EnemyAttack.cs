using System.Collections;
using System.Collections.Generic;
using UnityEditor.ShaderGraph.Internal;
using UnityEngine;

public class EnemyAttack : MonoBehaviour
{
    // Start is called before the first frame update
    float attackRange = 2f;
    float attackCooldown = 0.5f;
    public int attackDamage;
    float windAttackUp = 0.3f;
    public bool isAttacking;
    private Transform player;
    private Animator anim;
    private float lastAttackTime = -999f;   //?
    public Transform attackPoint;
    private int attackLayerIndex;

    private EnemyBasicMovement movement;

    void Awake()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform; // porque transform e nao getcomponent<transform>(); ?
        anim = GetComponentInChildren<Animator>();
        movement = GetComponent<EnemyBasicMovement>();
        attackLayerIndex = anim.GetLayerIndex("AttackLayer");
    }

    // Update is called once per frame
    void Update()
    {
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
    isAttacking = true;
    
    StartCoroutine(SmoothLayerTransition(1.0f, 0.1f));
    anim.SetTrigger("attack");

        // Aguarda o início da animação
    yield return new WaitForSeconds(windAttackUp); 


       
    Vector3 attackPosition = transform.position + transform.forward * 1.5f; 
    Collider[] hits = Physics.OverlapSphere(attackPosition, attackRange);
    
    Debug.Log("Colisores detectados: " + hits.Length); 
    
    foreach (var hit in hits)
    {
        if (hit.CompareTag("Player"))
        {
            Debug.Log("Acertou o jogador!"); 
            PlayerHealth health = hit.GetComponent<PlayerHealth>();
            if (health != null)
            {
                health.TakeDamage(attackDamage);
            }
            break;
        }
    }
    float attackDuration = GetAttackDuration() - windAttackUp;
    
    yield return new WaitForSeconds(attackDuration);

        yield return StartCoroutine(SmoothLayerTransition(0f, 0.2f));
        isAttacking = false;
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
            if(clip.name == "Cross Punch")
            {
                return clip.length;
            }
        }
        return 1f;
    }

}