using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class VFX : MonoBehaviour
{
    public VisualEffect effect;
    public GameObject blood;
    // Start is called before the first frame update
    void Start()
    {
        effect = GetComponent<VisualEffect>();
    }
   
   
}
