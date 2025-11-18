using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    [Header("Attack Settings")]
    [SerializeField] private float attackDistance = 3f;
    [SerializeField] private float attackCooldown = 0.3f;
    [SerializeField] private int attackDamage = 30;
    [SerializeField] private float windUpTime = 0.2f; // Reduzido para mais responsividade
    [SerializeField] private float attackOffset = 1.5f;
    [SerializeField] private LayerMask enemyLayer = 0;
    [SerializeField] private Rigidbody rb;
    [SerializeField] private Stun stun;

    [Header("Animation Settings")]
    [SerializeField] private float layerFadeInTime = 0.05f; // Muito rápido
    [SerializeField] private float layerFadeOutTime = 0.1f; // Muito rápido

    private PlayerThird playerThird;
    public Material slashMaterial;
    public Material glowMaterial;
    public float fadeSpeed = 2.0f;
    private float fadeValue = 0.0f;
    private int attackLayerIndex;
    private hitEffectScript hitScript;

    private int baseAttackDamage;

    [Header("References")]
    [SerializeField] private Animator anim;

    // runtime - simplificado
    private float lastAttackTime = -999f;
    private bool isAttacking = false;
    public CameraShake cam;

    // Input buffer simples
    private bool inputBuffered = false;
    private float inputBufferTime = 0.1f;
    private float inputBufferTimer = 0f;

    private void Awake()
    {
        if (anim == null)
            anim = GetComponent<Animator>();

        if (rb == null)
            rb = GetComponent<Rigidbody>();
        if (cam == null)
            cam = FindObjectOfType<CameraShake>();
        if (stun == null)
            stun = GetComponent<Stun>();
        if (playerThird == null)
            playerThird = GetComponent<PlayerThird>();
        if (hitScript == null)
            hitScript = FindObjectOfType<hitEffectScript>();

        attackLayerIndex = anim.GetLayerIndex("AttackLayer");
        baseAttackDamage = attackDamage;
    }

    public void ApplyAttackBonus(int levels)
    {
        attackDamage = baseAttackDamage + 4 * levels;
    }

    private void Update()
    {
        // Input Buffer simples
        if (Input.GetMouseButtonDown(0))
        {
            inputBuffered = true;
            inputBufferTimer = 0f;
        }

        // Processa input buffer
        if (inputBuffered)
        {
            inputBufferTimer += Time.deltaTime;
            
            if (inputBufferTimer > inputBufferTime)
            {
                inputBuffered = false;
            }
            else if (!isAttacking && Time.time >= lastAttackTime + attackCooldown)
            {
                inputBuffered = false;
                StartCoroutine(PerformAttack());
            }
        }

        // Atualiza fade do material
        if (fadeValue > 0)
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

        // Fade IN rápido da layer
        yield return StartCoroutine(SmoothLayerTransition(1.0f, layerFadeInTime));

        // Dispara animação IMEDIATAMENTE
        anim.SetTrigger("attack");
        FadeTrigger();

        Debug.Log("[PlayerAttack] Ataque iniciado");

        // Wind up reduzido
        yield return new WaitForSeconds(windUpTime);

        // Executa hit detection
        ExecuteAttackHit();

        // Espera um frame para garantir que a animação começou
        yield return null;

        // Aguarda um tempo mínimo antes de permitir próximo ataque
        float minAttackTime = 0.4f; // Ajuste conforme sua animação
        float elapsed = windUpTime;
        
        while (elapsed < minAttackTime)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }

        // Fade OUT rápido da layer
        yield return StartCoroutine(SmoothLayerTransition(0f, layerFadeOutTime));

        isAttacking = false;
        Debug.Log("[PlayerAttack] Ataque finalizado");
    }

    private void ExecuteAttackHit()
    {
        Vector3 center = transform.position + transform.forward * attackOffset;
        Collider[] hits;

        if (enemyLayer.value != 0)
            hits = Physics.OverlapSphere(center, attackDistance, enemyLayer.value, QueryTriggerInteraction.Ignore);
        else
            hits = Physics.OverlapSphere(center, attackDistance);

        HashSet<EnemyHealth> damaged = new HashSet<EnemyHealth>();

        foreach (var hit in hits)
        {
            if (hit.transform == transform) continue;
            bool isEnemy = hit.CompareTag("Enemy");
            bool isSpawner = hit.CompareTag("Spawn");

            EnemyHealth eh = hit.GetComponent<EnemyHealth>();
            EnemyBasicMovement enemyBasicMovement = hit.GetComponent<EnemyBasicMovement>();
            if (eh == null) eh = hit.GetComponentInParent<EnemyHealth>();
            if (eh == null) eh = hit.GetComponentInChildren<EnemyHealth>();
            EnemyAttack enemyAttack = hit.GetComponent<EnemyAttack>();

            if ((isEnemy || isSpawner) && eh != null && !damaged.Contains(eh))
            {
                damaged.Add(eh);

                IHitable hitable = hit.transform.GetComponent<IHitable>();
                if (hitable != null && hit.transform != transform)
                {
                    hitable.Execute(transform, true);
                }
                
                eh.TakeDamage(attackDamage);
                
                if (enemyAttack != null)
                {
                    enemyAttack.ApplyStun(1f);
                }
                
                hitScript.StartEffect(isAttacking, eh.hitSpawner);
                
                if (enemyBasicMovement != null)
                    enemyBasicMovement.currentState = EnemyBasicMovement.EnemyState.Chase;

                SpawnerLife spawnerLife = hit.GetComponent<SpawnerLife>();
                if (spawnerLife != null)
                {
                    spawnerLife.Execute(transform, true);
                }
            }
        }
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
        float timer = 0f;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            float currentWeight = Mathf.Lerp(startWeight, targetWeight, timer / duration);
            anim.SetLayerWeight(attackLayerIndex, currentWeight);
            yield return null;
        }
        anim.SetLayerWeight(attackLayerIndex, targetWeight);
    }
}