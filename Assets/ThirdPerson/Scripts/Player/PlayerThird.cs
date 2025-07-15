 using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Script responsável pelo controle do personagem em terceira pessoa, incluindo movimentação, rotação e integração com o sistema de stamina.
public class PlayerThird : MonoBehaviour
{
    [SerializeField] private Animator animator; // Referência ao Animator para controlar animações.
    private bool isWalking; // Indica se o personagem está andando.
    private bool isJumping; // Indica se o personagem está pulando.
    private bool isGrounded; // Indica se o personagem está no chão.
    private bool isSprinting; // Indica se o personagem está correndo.
    private Rigidbody rb; // Referência ao Rigidbody para movimentação física.

    public float moveSpeed = 6f; // Velocidade normal de movimento.
    public float SprintSpeed = 12f; // Velocidade ao correr.
    float rotateSpeed; // Velocidade de rotação.

    public bool cubeIsGrounded = true; // Indica se o cubo (personagem) está no chão.

    private Stamina stamina; // Referência ao script de stamina.

    // Inicialização das referências.
    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        stamina = FindObjectOfType<Stamina>(); // Busca o componente Stamina na cena.
    }

    // Atualização a cada frame para processar entrada e movimentação.
    private void Update()
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

    // Detecta colisão com o chão para atualizar estados de pulo e aterrissagem.
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.name == "Floor")
        {
            cubeIsGrounded = true;
            animator.SetBool("IsGrounded", true);
            isGrounded = true;
            animator.SetBool("IsJumping", false);
            isJumping = false;
        }
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

    // Observação: A lógica de movimentação foi adaptada de Transform para Rigidbody para melhor integração com a física do Unity.
}
