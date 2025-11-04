using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class hitEffectScript : MonoBehaviour
{
   
    [SerializeField] private VisualEffect effectItself;
    public GameObject effectInsume;
    private float effectTime = 99.0f;
    
    public void StartEffect(bool playEffect, Transform spawn)
    {
        if(playEffect)
        {
            StartCoroutine(PlayingCoroutine(spawn));
        }
    }
    public IEnumerator PlayingCoroutine(Transform transformSpawn)
    {
        GameObject spawnedEffect;
        spawnedEffect = Instantiate(effectInsume, transformSpawn);
        effectItself.Play();
        yield return new WaitForSeconds(effectTime);
        Destroy(spawnedEffect);
        yield return null;
    }
}
