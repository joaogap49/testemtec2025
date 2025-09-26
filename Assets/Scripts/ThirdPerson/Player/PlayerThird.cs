using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

// Script responsável pelo controle do personagem em terceira pessoa, incluindo movimentação, rotação e integração com o sistema de stamina.
public class PlayerThird : MonoBehaviour
{
    [SerializeField] private Animator animator; // Referência ao Animator para controlar animações.
    private bool isWalking; // Indica se o personagem está andando.
    private bool isJumping; // Indica se o personagem está pulando.
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



    public bool cubeIsGrounded = true; // Indica se o cubo (personagem) está no chão.

    private Stamina stamina; // Referência ao script de stamina.

    // Inicialização das referências.
    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        visualEffect = bloodPrefab.GetComponent<VisualEffect>();
       
    }

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        stun = GetComponent<Stun>();
        stamina = FindObjectOfType<Stamina>();
        if(bloodSpawnPoint == null)
        {
            bloodSpawnPoint = transform;
        }
        currentHealth = maxHealth;
        if (healthBar != null)
            healthBar.SetMaxHealth(maxHealth);

        Debug.Log("Rigidbody inicializado no Awake: " + rb);


    }

    // Atualização a cada frame para processar entrada e movimentação.
    private void FixedUpdate()
    {
        Movement();
       
    }

    // Detecta colisão com o chão para atualizar estados de pulo e aterrissagem.
    

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
    public void SetStunned(bool value)
    {
        isStunned = value;
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
    // Observação: A lógica de movimentação foi adaptada de Transform para Rigidbody para melhor integração com a física do Unity.
   
    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        healthBar.SetHealth(currentHealth);
        PlayBloodEffect();
        Debug.Log("tomei dano:" + damage + "pontos de vida.");
        //stun.ApplyStun();
        if (currentHealth < 0)
        {
            Die();
             
        }

    }

    // Update is called once per frame
    void Die()
    {
        Debug.Log("morte morrida");
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
