using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField] private Animator animator;
    private bool isWalking;
    private bool isJumping;
    private bool isGrounded;
    private bool isSprinting;
    private Rigidbody rb;

    //public Rigidbody rb;

    public float moveSpeed = 6f;
    public float SprintSpeed = 12f;
    float rotateSpeed;

    public bool cubeIsGrounded = true;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        Vector2 inputVector = new Vector2(0, 0);

        if (Input.GetKey(KeyCode.W))
        {
            inputVector.y = +1;
        }
        if (Input.GetKey(KeyCode.S))
        {
            inputVector.y = -1;
        }
        if (Input.GetKey(KeyCode.A))
        {
            inputVector.x = -1;
        }
        if (Input.GetKey(KeyCode.D))
        {
            inputVector.x = +1;
        }


        inputVector = inputVector.normalized;

        Vector3 moveDir = new Vector3(inputVector.x, 0f, inputVector.y);
        rb.MovePosition(rb.position + moveDir * moveSpeed * Time.deltaTime); //rb.position é quase o mesmo que transform.position. Após, o +, tem as mesams variaveis anteriores à tradução de transform para rigibody.
       
       if (Input.GetKey(KeyCode.LeftShift))
       {
           rb.MovePosition(rb.position + moveDir * SprintSpeed * Time.deltaTime);
           isSprinting = true;
           rotateSpeed = 5f;
       }
       else
       {
           isSprinting = false;
           rotateSpeed = 10f;
       }
        
       if(moveDir != Vector3.zero)
        {
            Quaternion toRotation = Quaternion.LookRotation(moveDir, Vector3.up); //Aqui tinha um problema. Transform.forward pode dar problemas, ainda mais com slerp. É melhor: Usar quaternion.LookRotation, que tem o que a gente precisa.
            Quaternion smoothedRotation = Quaternion.Slerp(rb.rotation, toRotation, Time.deltaTime * rotateSpeed); // ai fazemos um calculo simples entre a rotação atual, e a rotação do nosso input.
            rb.MoveRotation(smoothedRotation);
        }
       
        //transform.forward += Vector3.Slerp(transform.forward, moveDir, Time.deltaTime * rotateSpeed);

        isWalking = moveDir != Vector3.zero;

    }

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

    public bool IsSprinting()
    {
        return isSprinting;
    }
    public bool IsWalking()
    {
        return isWalking;
    }

    // A lógica deste aqui é a mesma. A unica coisa que fiz foi traduzir a lógica de movimentação de Transform para RIGIDBODY.


}