using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class EnemyHealth : MonoBehaviour
{

    public float currentHealth = 30f;
    private Animator animator;
    public EnemyBasicMovement enemyBasicMovement;
    public EnemyAttack enemyAttack;

    public GameObject DropLootPrefab;

    GameObject _dropLootTarget;


    private void Start()
    {
        animator = GetComponentInChildren<Animator>();    
        enemyBasicMovement = GetComponentInChildren<EnemyBasicMovement>();
        _dropLootTarget = GameObject.FindGameObjectWithTag("DropLootTracker");
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

        int lootToDrop = Mathf.Max(1, Mathf.RoundToInt(30f / 10f)); // Supondo 30f é a vida máxima
        for (int i = 0; i < lootToDrop; i++)
        {
            var go = Instantiate(DropLootPrefab, transform.position + new Vector3(0, Random.Range(0, 2), 0), Quaternion.identity);
            go.GetComponent<Follow>().Target = _dropLootTarget.transform;
        }

        

    }
}
