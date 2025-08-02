    using System.Collections;
using System.Collections.Generic;
using UnityEditor.ShaderGraph.Internal;
using UnityEngine;

// Script respons�vel pelo ataque do inimigo ao jogador.
public class EnemyAttack : MonoBehaviour
{
    // Dist�ncia m�xima para atacar o jogador.
    float attackRange = 2f;

    // Tempo m�nimo entre ataques consecutivos.
    float attackCooldown = 0.5f;

    // Dano causado ao jogador por ataque.
    public int attackDamage;

    // Tempo de prepara��o antes do ataque (delay para sincronizar com anima��o).
    float windAttackUp = 0.3f;

    // Indica se o inimigo est� atualmente atacando.
    public bool isAttacking;

    // Refer�ncia ao transform do jogador.
    private Transform player;

    // Refer�ncia ao Animator do inimigo.
    private Animator anim;

    // Armazena o tempo do �ltimo ataque para controlar o cooldown.
    private float lastAttackTime = -999f;

    // Ponto de origem do ataque (pode ser usado para efeitos ou checagem de colis�o).
    public Transform attackPoint;

    // �ndice da camada de anima��o de ataque.
    private int attackLayerIndex;

    // Refer�ncia ao script de movimento do inimigo.
    private EnemyBasicMovement movement;

    // Inicializa��o das refer�ncias.
    void Awake()
    {
        // Busca o jogador pela tag "Player" e pega seu transform.
        player = GameObject.FindGameObjectWithTag("Player").transform;
        // Busca o Animator no filho do inimigo.
        anim = GetComponentInChildren<Animator>();
        // Busca o script de movimento do inimigo.
        movement = GetComponent<EnemyBasicMovement>();
        // Pega o �ndice da camada de anima��o chamada "AttackLayer".
        attackLayerIndex = anim.GetLayerIndex("AttackLayer");
    }

    // Atualiza��o a cada frame.
    void Update()
    {
        // S� ataca se o inimigo estiver perseguindo o jogador.
        if (movement.currentState != EnemyBasicMovement.EnemyState.Chase)
            return;

        // Calcula a dist�ncia at� o jogador.
        float distance = Vector3.Distance(transform.position, player.position);
        // Verifica se est� na anima��o de ataque.
        bool isInAttackAnim = anim.GetCurrentAnimatorStateInfo(0).IsName("attack");

        // Se o jogador est� ao alcance, n�o est� atacando e o cooldown passou, inicia o ataque.
        if (distance <= attackRange && !isAttacking && Time.time >= lastAttackTime + attackCooldown)
        {
            StartCoroutine(PerformAttack());
            lastAttackTime = Time.time;
        }
    }

    // Coroutine que executa o ataque do inimigo.
    IEnumerator PerformAttack()
    {
        isAttacking = true;

        // Faz a transi��o suave para a camada de ataque na anima��o.
        StartCoroutine(SmoothLayerTransition(1.0f, 0.1f));
        // Dispara o trigger de ataque na anima��o.
        anim.SetTrigger("attack");

        // Aguarda o tempo de prepara��o do ataque (sincroniza com a anima��o).
        yield return new WaitForSeconds(windAttackUp);

        // Calcula a posi��o do ataque � frente do inimigo.
        Vector3 attackPosition = transform.position + transform.forward * 1.5f;
        // Detecta todos os colliders ao redor do ponto de ataque.
        Collider[] hits = Physics.OverlapSphere(attackPosition, attackRange);

        Debug.Log("Colisores detectados: " + hits.Length);

        // Verifica se algum dos colliders � o jogador.
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
                break; // S� ataca uma vez por ciclo.
            }
        }

        // Aguarda o restante da dura��o da anima��o de ataque.
        float attackDuration = GetAttackDuration() - windAttackUp;
        yield return new WaitForSeconds(attackDuration);

        // Faz a transi��o suave para sair da camada de ataque.
        yield return StartCoroutine(SmoothLayerTransition(0f, 0.2f));
        isAttacking = false;
    }

    // Controla o IK (Inverse Kinematics) da m�o do inimigo durante o ataque.
    void OnAnimatorIK(int layerIndex)
    {
        // S� aplica IK se estiver na camada de ataque e o peso da camada for suficiente.
        if (layerIndex == attackLayerIndex && anim.GetLayerWeight(attackLayerIndex) > 0.5f)
        {
            // Move a m�o direita do inimigo em dire��o ao jogador (para anima��o mais realista).
            anim.SetIKPosition(AvatarIKGoal.RightHand, player.position + Vector3.up * 1.5f);
            anim.SetIKPositionWeight(AvatarIKGoal.RightHand, 0.8f);
        }
    }

    // Faz a transi��o suave do peso da camada de anima��o de ataque.
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

    // Retorna a dura��o da anima��o de ataque ("Cross Punch").
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
        // Valor padr�o caso n�o encontre a anima��o.
        return 1f;
    }
}