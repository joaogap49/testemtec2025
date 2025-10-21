using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class attackVfx : MonoBehaviour
{
    public VisualEffect effect;
    void Start()
    {
        //effect = GetComponentInChildren<VisualEffect>();
    }
    public void StartEffect()
    {
        Debug.Log("Evento de animação chamado!"); // Para testar

        if (effect != null)
        {
            effect.Play();
            Debug.Log("VFX iniciado!");
        }
        else
        {
            Debug.LogError("SwordTrail não atribuído! Arraste o VFX para o slot no Inspector.");
        }
    }
   
}
