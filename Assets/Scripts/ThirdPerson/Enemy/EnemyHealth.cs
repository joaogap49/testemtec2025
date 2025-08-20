using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    public float currentHealth = 30f;
    private Animator animator;
    public EnemyBasicMovement enemyBasicMovement;
    public EnemyAttack enemyAttack;


    private void Start()
    {
        animator = GetComponentInChildren<Animator>();    
        enemyBasicMovement = GetComponentInChildren<EnemyBasicMovement>();
    }
    // Start is called before the first frame update
    public void SetHealth(float health)
    {
        currentHealth = health;
    }
    public void TakeDamage(float damage)
    {
        if(currentHealth < 0)
        {
            return;
        }
        currentHealth -= damage;
        int randomReaction = Random.Range(5, 7);
        animator.SetInteger("state", randomReaction);
        animator.SetTrigger("attacked");
        if (currentHealth <= 0)
        {
            Die();
            animator.ResetTrigger("attacked");
        }
    }
    // Update is called once per frame
    void Die()
    {
        enemyBasicMovement.enabled = false;
        //enemyAttack.enabled = false;
        animator.SetTrigger("death");
    }
}
