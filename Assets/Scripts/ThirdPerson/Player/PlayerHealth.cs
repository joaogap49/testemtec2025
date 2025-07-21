using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    int maxHealth = 100;
    private int currentHealth;
    // Start is called before the first frame update
    void Awake()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        Debug.Log("tomei dano:" + damage + "pontos de vida.");
        if(currentHealth < 0)
        {
            Die();
        }

    }

    // Update is called once per frame
    void Die()
    {
        Debug.Log("morte morrida");
    }
}
