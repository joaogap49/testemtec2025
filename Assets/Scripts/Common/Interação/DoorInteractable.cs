using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement; // Adicionado para troca de cena

public class DoorInteractable : MonoBehaviour, IInteractable
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
        SceneManager.LoadScene("PHASE1"); // Troca para a cena PHASE 1
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
