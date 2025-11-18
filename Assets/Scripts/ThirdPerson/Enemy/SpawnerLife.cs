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
        // Busca o MeshRenderer no próprio objeto ou nos filhos
        meshSpawner = GetComponentInChildren<MeshRenderer>();
        sphereCollider = GetComponent<SphereCollider>();
        hitlerIsDead = false;

        Debug.Log("SpawnerLife: MeshRenderer encontrado? " + (meshSpawner != null));
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
        if (hitlerIsDead) return; // Evita execução múltipla

        hitlerIsDead = true;
        Debug.Log("SpawnerLife: Spawner morreu!");

        if (meshSpawner != null)
        {
            meshSpawner.enabled = false;
        }
        if (sphereCollider != null)
        {
            sphereCollider.enabled = false; // Desativa completamente
        }

        // Para o spawn de inimigos
        Spawner spawnerComponent = GetComponentInChildren<Spawner>();
        if (spawnerComponent != null)
        {
            spawnerComponent.StopAllCoroutines();
            spawnerComponent.enabled = false;
        }

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
