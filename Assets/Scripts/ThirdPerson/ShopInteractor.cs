using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement; // Adicionado para troca de cena

public class ShopInteractor : MonoBehaviour, IInteractable
{
    private Animator animator;
    private bool isOpen;
    private SpawnerLife spawnerLife;
    private Spawner spawner;
    private void Awake()
    {
        animator = GetComponent<Animator>();
        spawnerLife = FindObjectOfType<SpawnerLife>();
        spawner = FindObjectOfType<Spawner>();
    }

    public void ToggleDoor()
    {
        isOpen = !isOpen;
        animator.SetBool("isOpen", isOpen);
    }

    public void Interact()
    {
        //if(spawnerLife.hitlerIsDead && spawner.aliveEnemies == 0)
        if (Input.GetKeyDown(KeyCode.E))
        {
            ToggleDoor();
            SceneManager.LoadScene("Shop", LoadSceneMode.Single);
        }
        
    }

    public string GetInteractText()
    {
        return "Abrir/Fechar porta";
    }
    public Transform GetTransform()
    {
        return transform;
    }
    
}
