 using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Script respons�vel pelo controle do personagem em terceira pessoa, incluindo movimenta��o, rota��o e integra��o com o sistema de stamina.
public class PlayerThird : MonoBehaviour
{
    [SerializeField] private Animator animator; // Refer�ncia ao Animator para controlar anima��es.
    private bool isWalking; // Indica se o personagem est� andando.
    private bool isJumping; // Indica se o personagem est� pulando.
    private bool isGrounded; // Indica se o personagem est� no ch�o.
    private bool isSprinting; // Indica se o personagem est� correndo.
    private Rigidbody rb; // Refer�ncia ao Rigidbody para movimenta��o f�sica.

    public float moveSpeed = 6f; // Velocidade normal de movimento.
    public float SprintSpeed = 12f; // Velocidade ao correr.
    float rotateSpeed; // Velocidade de rota��o.

    public bool cubeIsGrounded = true; // Indica se o cubo (personagem) est� no ch�o.

    private Stamina stamina; // Refer�ncia ao script de stamina.

    // Inicializa��o das refer�ncias.
    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        stamina = FindObjectOfType<Stamina>(); // Busca o componente Stamina na cena.
    }

    // Atualiza��o a cada frame para processar entrada e movimenta��o.
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

        // Use a l�gica centralizada de sprint
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

    // Detecta colis�o com o ch�o para atualizar estados de pulo e aterrissagem.
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

    // Retorna se o personagem est� correndo.
    public bool IsSprinting()
    {
        return isSprinting;
    }
    // Retorna se o personagem est� andando.
    public bool IsWalking()
    {
        return isWalking;
    }

    // Observa��o: A l�gica de movimenta��o foi adaptada de Transform para Rigidbody para melhor integra��o com a f�sica do Unity.
}
