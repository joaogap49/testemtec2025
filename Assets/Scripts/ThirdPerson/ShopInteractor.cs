using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement; // Adicionado para troca de cena

public class ShopInteractor : MonoBehaviour, IInteractable
{
    private Animator animator;
    private bool isOpen;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    public void ToggleDoor()
    {
        isOpen = !isOpen;
        animator.SetBool("isOpen", isOpen);
    }

    public void Interact()
    {
        ToggleDoor();
        SceneManager.LoadScene("Shop");
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
