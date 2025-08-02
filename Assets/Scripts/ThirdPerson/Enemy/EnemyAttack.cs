    using System.Collections;
using System.Collections.Generic;
using UnityEditor.ShaderGraph.Internal;
using UnityEngine;

// Script responsável pelo ataque do inimigo ao jogador.
public class EnemyAttack : MonoBehaviour
{
    // Distância máxima para atacar o jogador.
    float attackRange = 2f;

    // Tempo mínimo entre ataques consecutivos.
    float attackCooldown = 0.5f;

    // Dano causado ao jogador por ataque.
    public int attackDamage;

    // Tempo de preparação antes do ataque (delay para sincronizar com animação).
    float windAttackUp = 0.3f;

    // Indica se o inimigo está atualmente atacando.
    public bool isAttacking;

    // Referência ao transform do jogador.
    private Transform player;

    // Referência ao Animator do inimigo.
    private Animator anim;

    // Armazena o tempo do último ataque para controlar o cooldown.
    private float lastAttackTime = -999f;

    // Ponto de origem do ataque (pode ser usado para efeitos ou checagem de colisão).
    public Transform attackPoint;

    // Índice da camada de animação de ataque.
    private int attackLayerIndex;

    // Referência ao script de movimento do inimigo.
    private EnemyBasicMovement movement;

    // Inicialização das referências.
    void Awake()
    {
        // Busca o jogador pela tag "Player" e pega seu transform.
        player = GameObject.FindGameObjectWithTag("Player").transform;
        // Busca o Animator no filho do inimigo.
        anim = GetComponentInChildren<Animator>();
        // Busca o script de movimento do inimigo.
        movement = GetComponent<EnemyBasicMovement>();
        // Pega o índice da camada de animação chamada "AttackLayer".
        attackLayerIndex = anim.GetLayerIndex("AttackLayer");
    }

    // Atualização a cada frame.
    void Update()
    {
        // Só ataca se o inimigo estiver perseguindo o jogador.
        if (movement.currentState != EnemyBasicMovement.EnemyState.Chase)
            return;

        // Calcula a distância até o jogador.
        float distance = Vector3.Distance(transform.position, player.position);
        // Verifica se está na animação de ataque.
        bool isInAttackAnim = anim.GetCurrentAnimatorStateInfo(0).IsName("attack");

        // Se o jogador está ao alcance, não está atacando e o cooldown passou, inicia o ataque.
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

        // Faz a transição suave para a camada de ataque na animação.
        StartCoroutine(SmoothLayerTransition(1.0f, 0.1f));
        // Dispara o trigger de ataque na animação.
        anim.SetTrigger("attack");

        // Aguarda o tempo de preparação do ataque (sincroniza com a animação).
        yield return new WaitForSeconds(windAttackUp);

        // Calcula a posição do ataque à frente do inimigo.
        Vector3 attackPosition = transform.position + transform.forward * 1.5f;
        // Detecta todos os colliders ao redor do ponto de ataque.
        Collider[] hits = Physics.OverlapSphere(attackPosition, attackRange);

        Debug.Log("Colisores detectados: " + hits.Length);

        // Verifica se algum dos colliders é o jogador.
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
                break; // Só ataca uma vez por ciclo.
            }
        }

        // Aguarda o restante da duração da animação de ataque.
        float attackDuration = GetAttackDuration() - windAttackUp;
        yield return new WaitForSeconds(attackDuration);

        // Faz a transição suave para sair da camada de ataque.
        yield return StartCoroutine(SmoothLayerTransition(0f, 0.2f));
        isAttacking = false;
    }

    // Controla o IK (Inverse Kinematics) da mão do inimigo durante o ataque.
    void OnAnimatorIK(int layerIndex)
    {
        // Só aplica IK se estiver na camada de ataque e o peso da camada for suficiente.
        if (layerIndex == attackLayerIndex && anim.GetLayerWeight(attackLayerIndex) > 0.5f)
        {
            // Move a mão direita do inimigo em direção ao jogador (para animação mais realista).
            anim.SetIKPosition(AvatarIKGoal.RightHand, player.position + Vector3.up * 1.5f);
            anim.SetIKPositionWeight(AvatarIKGoal.RightHand, 0.8f);
        }
    }

    // Faz a transição suave do peso da camada de animação de ataque.
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

    // Retorna a duração da animação de ataque ("Cross Punch").
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
        // Valor padrão caso não encontre a animação.
        return 1f;
    }
}