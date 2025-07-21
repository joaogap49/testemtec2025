using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
