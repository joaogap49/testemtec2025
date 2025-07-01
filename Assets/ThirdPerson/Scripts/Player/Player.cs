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
        rb.MovePosition(rb.position + moveDir * moveSpeed * Time.deltaTime); //rb.position � quase o mesmo que transform.position. Ap�s, o +, tem as mesams variaveis anteriores � tradu��o de transform para rigibody.
       
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
            Quaternion toRotation = Quaternion.LookRotation(moveDir, Vector3.up); //Aqui tinha um problema. Transform.forward pode dar problemas, ainda mais com slerp. � melhor: Usar quaternion.LookRotation, que tem o que a gente precisa.
            Quaternion smoothedRotation = Quaternion.Slerp(rb.rotation, toRotation, Time.deltaTime * rotateSpeed); // ai fazemos um calculo simples entre a rota��o atual, e a rota��o do nosso input.
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

    // A l�gica deste aqui � a mesma. A unica coisa que fiz foi traduzir a l�gica de movimenta��o de Transform para RIGIDBODY.


}