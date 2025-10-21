using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class effectsTester : MonoBehaviour
{
    public VisualEffect trail;
    public KeyCode testKey = KeyCode.Space;

    void Update()
    {
        if (Input.GetKeyDown(testKey))
        {
            trail.Play();
            Debug.Log("VFX ativado pelo teclado!");
        }
    }
}