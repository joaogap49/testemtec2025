using System.Collections;
using System.Collections.Generic;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using static EnemyHealth;

public class PlayerAttack : MonoBehaviour
{
    [Header("Attack Settings")]
    [SerializeField] private float attackDistance = 3f;
    [SerializeField] private float attackCooldown = 0.0f;
    [SerializeField] private int attackDamage = 30;
    [SerializeField] private float windUpTime = 0.3f;
    [SerializeField] private float attackOffset = 1.5f; // onde a esfera é centrada à frente do jogador
    [SerializeField] private LayerMask enemyLayer = 0;  // selecione a layer dos inimigos (opcional)
    [SerializeField] private Rigidbody rb;
    [SerializeField] private Stun stun;
    private PlayerThird playerThird;
    public Material slashMaterial;
    public Material glowMaterial;
    public float fadeSpeed = 2.0f;
    private float fadeValue = 0.0f;
    private int attackLayerIndex;

    [Header("References")]
    [SerializeField] private Animator anim; // assign no Inspector ou será procurado no Awake

    // runtime
    private float lastAttackTime = -999f;
    private bool isAttacking = false;
    public CameraShake cam;
    private void Awake()
    {
        if (anim == null)
            anim = GetComponent<Animator>();

        if (anim == null)
            Debug.LogWarning("PlayerAttack: Animator não encontrado. Assigne-o no Inspector.");
        if(rb == null)
        {
            rb = GetComponent<Rigidbody>();
        }
        if(cam == null)
        {
            cam = FindObjectOfType<CameraShake>();
        }
        if(stun == null)
        {
            stun = GetComponent<Stun>();
        }
        if(playerThird == null)
        {
            playerThird = GetComponent<PlayerThird>();
        }
        
        attackLayerIndex = anim.GetLayerIndex("AttackLayer");
    }

    private void Update()
    {
        // DEBUG: mostra quando o clique foi detectado e se o cooldown permite
        if (Input.GetMouseButtonDown(0))
        {
            Debug.Log($"[PlayerAttack] Mouse0 pressed — isAttacking={isAttacking} timeOK={(Time.time >= lastAttackTime + attackCooldown)}");
        }

        // Use GetMouseButtonDown para responder ao clique uma vez
        if (Input.GetMouseButtonDown(0) && !isAttacking && Time.time >= lastAttackTime + attackCooldown)
        {

                StartCoroutine(PerformAttack());
                
        }
        if(fadeValue > 0)
        {
            fadeValue -= Time.deltaTime * fadeSpeed;
            fadeValue = Mathf.Clamp01(fadeValue);
            slashMaterial.SetFloat("_slashFase", fadeValue);
            glowMaterial.SetFloat("_slashFase", fadeValue);
        }
    }

    private IEnumerator PerformAttack()
    {
        
        
        isAttacking = true;
        lastAttackTime = Time.time;
        StartCoroutine(SmoothLayerTransition(1.0f, 0.1f));
        anim.SetTrigger("attack");
        FadeTrigger();
        anim.SetInteger("attackNumber", Random.Range(1, 3));// -> **verifique o nome exato do parâmetro no Animator**
        Debug.Log("[PlayerAttack] Trigger 'attack' set on Animator");
        

        // Espera o wind up (sincronizar com o frame de impacto da animação)
        yield return new WaitForSeconds(windUpTime);


        if(!isAttacking)
        {
            yield break;
        }
        // Centro do ataque (frente do jogador)
        Vector3 center = transform.position + transform.forward * attackOffset;

        // Se enemyLayer == 0 usamos todos os colliders; caso contrário filtramos pela layer
        Collider[] hits;
        if (enemyLayer.value != 0)
            hits = Physics.OverlapSphere(center, attackDistance, enemyLayer.value, QueryTriggerInteraction.Ignore);
        else
            hits = Physics.OverlapSphere(center, attackDistance);

        
        Debug.Log($"[PlayerAttack] OverlapSphere encontrou {hits.Length} colliders.");

        // Evitar aplicar dano duplicado ao mesmo EnemyHealth (hashset)
        HashSet<EnemyHealth> damaged = new HashSet<EnemyHealth>();

        foreach (var hit in hits)
        {
            if (hit.transform == transform) continue;
            bool isEnemy = hit.CompareTag("Enemy");
            bool isSpawner = hit.CompareTag("Spawn");
            // tenta encontrar EnemyHealth no próprio collider, nos pais ou nos filhos
            EnemyHealth eh = hit.GetComponent<EnemyHealth>();
            EnemyBasicMovement enemyBasicMovement = hit.GetComponent<EnemyBasicMovement>();
            if (eh == null) eh = hit.GetComponentInParent<EnemyHealth>();
            if (eh == null) eh = hit.GetComponentInChildren<EnemyHealth>();
            EnemyAttack enemyAttack = hit.GetComponent<EnemyAttack>();
            

            if(isEnemy || isSpawner)
            {
                if (eh != null && !damaged.Contains(eh))
                {
                    damaged.Add(eh);

                    IHitable hitable = hit.transform.GetComponent<IHitable>();
                    if (hitable != null && hit.transform != transform) // evita dar knockback em si mesmo
                    {
                        hitable.Execute(transform, true);
                        //Vector3 knockBackDir = (transform.position - hit.transform.position).normalized;
                        //playerThird.ApplyKnockBack(knockBackDir);

                    }
                    eh.TakeDamage(attackDamage);
                    if (enemyAttack != null)
                    {
                        enemyAttack.ApplyStun(1f);
                    }

                    enemyBasicMovement.currentState = EnemyBasicMovement.EnemyState.Chase;

                    Debug.Log($"[PlayerAttack] Aplicou {attackDamage} de dano em '{eh.gameObject.name}'. HP agora = {eh.currentHealth}");
                }
                else if (eh == null)
                {
                    Debug.Log($"[PlayerAttack] Collider '{hit.name}' não possui EnemyHealth.");
                }
                SpawnerLife spawnerLife = hit.GetComponent<SpawnerLife>();
               
                if (spawnerLife != null)
                {
                    spawnerLife.Execute(transform, true);
                }
            }
            
        }
        
        // Pequena folga (opcional)
        //yield return new WaitForSeconds(anim.GetCurrentAnimatorStateInfo(0).length);
        //yield return null;
        yield return StartCoroutine(SmoothLayerTransition(0f, .1f));
        isAttacking = false;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Vector3 center = transform.position + transform.forward * attackOffset;
        Gizmos.DrawWireSphere(center, attackDistance);
    }
    private void FadeTrigger()
    {
        fadeValue = 1f;
        slashMaterial.SetFloat("_slashFase", fadeValue);
        glowMaterial.SetFloat("_slashFase", fadeValue);
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

   
}
