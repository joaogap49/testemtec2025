using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnerLife : MonoBehaviour, IHitable
{
    [SerializeField] private int maxHealth = 100;
    private int currentHealth;
    public GameObject spawner;
    public MeshRenderer meshSpawner;
    public SphereCollider sphereCollider;
    public bool hitlerIsDead;
    private void Start()
    {
        currentHealth = maxHealth;
        spawner = GetComponentInChildren<Spawner>().gameObject;
        meshSpawner = GameObject.FindGameObjectWithTag("spawnObject").GetComponent<MeshRenderer>();
        sphereCollider = GetComponent<SphereCollider>();   
        hitlerIsDead = false;
    }
    private void Update()
    {
        if(currentHealth <= 0)
        {
            Die();
        }
    }
    public void TakeDamage(int health)
    {
        if (currentHealth <= 0) return;
        currentHealth -= health;
    }
    void Die()
    {
        
        if(meshSpawner != null)
        {
            meshSpawner.enabled = false;
        }
        if(sphereCollider != null)
        {
            sphereCollider.isTrigger = true;
        }
        hitlerIsDead = true;
        
    }
    public void Execute(Transform attackSource, bool isPlayerAttack)
    {
        Debug.Log(isPlayerAttack + "eu adoro chupar rolas gordas PRA CARALHO");
        if(isPlayerAttack)
        {
            TakeDamage(30);
        }
    }
}
