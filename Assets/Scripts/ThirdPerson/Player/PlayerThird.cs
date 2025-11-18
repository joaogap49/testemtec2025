using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

// Script responsável pelo controle do personagem em terceira pessoa, incluindo movimentação, rotação e integração com o sistema de stamina.
// Agora implementa IShopCustomer para permitir comprar upgrades e aplicar efeitos nos stats.
public class PlayerThird : MonoBehaviour, IShopCustomer
{
    [SerializeField] private Animator animator; // Referência ao Animator para controlar animações.
    private bool isWalking; // Indica se o personagem está andando.
    private bool isJumping; // Indica se o personagem está pulando.
    private bool isDamaged; // Indica se o personagem tomou dano.
    [SerializeField] private bool isGrounded; // Indica se o personagem está no chão.
    private bool isSprinting; // Indica se o personagem está correndo.
    private Rigidbody rb; // Referência ao Rigidbody para movimentação física.
    Stun stun;
    EnemyHealth enemyHealth;
    public bool isStunned = false;
    public float moveSpeed = 24f; // Velocidade normal de movimento.
    public float SprintSpeed = 50f; // Velocidade ao correr.
    float rotateSpeed; // Velocidade de rotação.
    public int maxHealth = 100; // Vida máxima do personagem.
    public int currentHealth; // Vida atual do personagem.
    public PlayerHealth healthBar; // Referência à barra de vida do personagem.
    [Header("Blood Effect")]
    public GameObject bloodPrefab;
    public Transform bloodSpawnPoint;
    public VisualEffect visualEffect;
    private PlayerAnimator playerAnimator;

    // Valores base (armazenam os valores iniciais para aplicar bônus acumulativos)
    private int baseMaxHealth;
    private float baseMoveSpeed;
    private float baseSprintSpeed;

    public bool cubeIsGrounded = true; // Indica se o cubo (personagem) está no chão.

    private Stamina stamina; // Referência ao script de stamina.
    private PlayerAttack playerAttack; // Referência ao script de ataque para aplicar bônus de dano.

    // Inicialização das referências.
    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        if (bloodPrefab != null)
            visualEffect = bloodPrefab.GetComponent<VisualEffect>();
    }

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        stun = GetComponent<Stun>();
        stamina = FindObjectOfType<Stamina>();
        playerAttack = GetComponent<PlayerAttack>();

        if(bloodSpawnPoint == null)
        {
            bloodSpawnPoint = transform;
        }

        // Guarda os valores base para cálculo de bônus por nível
        baseMaxHealth = maxHealth;
        baseMoveSpeed = moveSpeed;
        baseSprintSpeed = SprintSpeed;

        // Aplica níveis de upgrade já adquiridos (caso existam)
        ApplyUpgrades();

        // Inicializa vida atual e HUD
        if (currentHealth <= 0)
            currentHealth = maxHealth;
        if (healthBar != null)
            healthBar.SetMaxHealth(maxHealth);
        if(playerAnimator == null)
        {
            playerAnimator = GetComponent<PlayerAnimator>();
        }

        // LOG: Exibe os níveis atuais e os valores resultantes dos stats
        int forcaLevel = Upgrades.GetLevel(Upgrades.UpgradeType.Forca);
        int defesaLevel = Upgrades.GetLevel(Upgrades.UpgradeType.Defesa);
        int estaminaLevel = Upgrades.GetLevel(Upgrades.UpgradeType.Estamina);
        int velocidadeLevel = Upgrades.GetLevel(Upgrades.UpgradeType.Velocidade);

        Debug.Log($"PlayerThird - Níveis de upgrades: Força={forcaLevel}, Defesa={defesaLevel}, Estamina={estaminaLevel}, Velocidade={velocidadeLevel}");
        Debug.Log($"PlayerThird - Stats atuais: maxHealth={maxHealth}, currentHealth={currentHealth}, attackDamage={(playerAttack != null ? playerAttack.GetType().GetField("attackDamage", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.GetValue(playerAttack) : "N/A")}, maxStamina={(stamina != null ? stamina.GetType().GetField("maxStamina", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.GetValue(stamina) : "N/A")}, moveSpeed={moveSpeed}, SprintSpeed={SprintSpeed}");

        Debug.Log("Rigidbody inicializado no Awake: " + rb);
    }

    // Aplica os efeitos dos upgrades nos atributos do jogador. Deve ser chamado no Start e após cada compra.
    private void ApplyUpgrades()
    {
        // Níveis por tipo de upgrade
        int forcaLevel = Upgrades.GetLevel(Upgrades.UpgradeType.Forca);
        int defesaLevel = Upgrades.GetLevel(Upgrades.UpgradeType.Defesa);
        int estaminaLevel = Upgrades.GetLevel(Upgrades.UpgradeType.Estamina);
        int velocidadeLevel = Upgrades.GetLevel(Upgrades.UpgradeType.Velocidade);

        // Health: +20 por nível de Defesa
        int oldMax = maxHealth;
        float currentPct = oldMax > 0 ? (float)currentHealth / oldMax : 1f;
        maxHealth = baseMaxHealth + defesaLevel * 20;
        currentHealth = Mathf.RoundToInt(currentPct * maxHealth);

        // Atualiza HUD se existir
        if (healthBar != null)
        {
            healthBar.SetMaxHealth(maxHealth);
            healthBar.SetHealth(currentHealth);
        }

        // Ataque: +4 por nível de Força
        if (playerAttack != null)
        {
            playerAttack.ApplyAttackBonus(forcaLevel);
        }

        // Stamina: +15 por nível de Estamina
        if (stamina != null)
        {
            stamina.ApplyMaxStaminaBonus(estaminaLevel);
        }

        // Velocidade: moveSpeed +1.0f por nível, SprintSpeed +2.0f por nível
        // Agora o upgrade de Velocidade só aumenta a velocidade de SPRINT, não a velocidade de caminhada.
        moveSpeed = baseMoveSpeed;
        SprintSpeed = baseSprintSpeed + velocidadeLevel * 2.0f;

        Debug.Log($"Upgrades aplicados ? Força:{forcaLevel} Defesa:{defesaLevel} Estamina:{estaminaLevel} Velocidade:{velocidadeLevel}");
    }

    // Retorna se o personagem está correndo.
    public bool IsSprinting()
    {
        return isSprinting;
    }
    // Retorna se o personagem está andando.
    public bool IsWalking()
    {
        return isWalking;
    }
    public bool IsDamaged()
    {
        return isDamaged;
    }
    public void SetStunned(bool value)
    {
        isStunned = value;
    }

    // Atualização a cada frame para processar entrada e movimentação.
    private void FixedUpdate()
    {
        Movement();
    }

    public void Movement()
    {
        Vector2 inputVector = new Vector2(0, 0);

        // Captura das teclas de movimento (WASD)
        if (Input.GetKey(KeyCode.W)) inputVector.y = +1;
        if (Input.GetKey(KeyCode.S)) inputVector.y = -1;
        if (Input.GetKey(KeyCode.A)) inputVector.x = -1;
        if (Input.GetKey(KeyCode.D)) inputVector.x = +1;

        inputVector = inputVector.normalized;
        Vector3 moveDir = new Vector3(inputVector.x, 0f, inputVector.y);

        float currentSpeed = moveSpeed;
        bool wantsToSprint = Input.GetKey(KeyCode.LeftShift);

        // Use a lógica centralizada de sprint
        if (stamina != null)
        {
            stamina.HandleSprint(wantsToSprint, SprintSpeed, ref currentSpeed, ref isSprinting, ref rotateSpeed);
        }

        rb.MovePosition(rb.position + moveDir * currentSpeed * Time.deltaTime);

        if (moveDir != Vector3.zero)
        {
            Quaternion toRotation = Quaternion.LookRotation(moveDir, Vector3.up);
            Quaternion smoothedRotation = Quaternion.Slerp(rb.rotation, toRotation, Time.deltaTime * rotateSpeed);
            rb.MoveRotation(smoothedRotation);
        }

        isWalking = moveDir != Vector3.zero;
    }

    public void TakeDamage(int damage)
    {
        StartCoroutine(isDamagedCorroutine());
        currentHealth -= damage;
        if (healthBar != null)
            healthBar.SetHealth(currentHealth);
        PlayBloodEffect();
        Debug.Log("tomei dano:" + damage + "pontos de vida.");
        stun.ApplyStun();
        if (currentHealth < 0)
        {
            Die();
        }
    }
    private IEnumerator isDamagedCorroutine()
    {
        isDamaged = true;
        StartCoroutine(playerAnimator.SmoothLayerTransition(1.0f, 0.1f));
        yield return new WaitForSeconds(.24f);
        isDamaged = false;
        yield return null;
        StartCoroutine(playerAnimator.SmoothLayerTransition(0f, 0.1f));
    }

    // Update is called once per frame
    void Die()
    {
        Debug.Log("morte morrida");

        // Show Game Over UI if a manager exists in the scene
        var gom = GameObject.FindObjectOfType<GameOverManager>();
        if (gom != null)
        {
            gom.ShowGameOver("Game Over", "Você morreu");
        }
        else
        {
            // fallback: pause the game and show cursor so player can inspect
            Time.timeScale = 0f;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    public void AddXP(int amount)
    {
        PlayerXPManager.Instance.AddXP(amount);
    }

    public bool TrySpendXP(int amount)
    {
        return PlayerXPManager.Instance.TrySpendXP(amount);
    }

    public int GetXPAmount()
    {
        return PlayerXPManager.Instance.XP;
    }

    public void PlayBloodEffect()
    {
        if(bloodPrefab != null)
        {
            GameObject bloodInstance = Instantiate(bloodPrefab, bloodSpawnPoint.position, Quaternion.LookRotation(bloodPrefab.transform.forward));
            visualEffect.Play();
      
            Destroy(bloodInstance, 3.0f);
        }
    }

    // Implementação do IShopCustomer - chamado pela UI da loja quando o jogador compra um item
    public void BoughtItem(Upgrades.UpgradeType upgradeType)
    {
        int cost = Upgrades.GetCost(upgradeType);
        if (TrySpendXP(cost))
        {
            bool increased = Upgrades.IncreaseLevel(upgradeType);
            if (!increased)
            {
                Debug.LogWarning($"Upgrade {upgradeType} já estava no nível máximo.");
            }
            else
            {
                Debug.Log($"PlayerThird comprou upgrade: {upgradeType}. Agora nível: {Upgrades.GetLevel(upgradeType)}");
                // Aplica imediatamente os efeitos dos upgrades
                ApplyUpgrades();
            }
        }
        else
        {
            Debug.Log("XP insuficiente para comprar o upgrade!");
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        //if (collision.gameObject.name == "Floor")
        //{
            //cubeIsGrounded = true;
            //animator.SetBool("IsGrounded", true);
            //isGrounded = true;
            //animator.SetBool("IsJumping", false);
            //isJumping = false;
        //}
    }
}
