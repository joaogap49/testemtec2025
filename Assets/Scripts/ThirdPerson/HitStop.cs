using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitStop : MonoBehaviour
{
    bool isFrozen = false;

    public void DoHitStop(float duration, float timeScale)
    {
        if(!isFrozen)
        {
            StartCoroutine(HitStopCoroutine(duration, timeScale));

        }
    }
    private IEnumerator HitStopCoroutine(float duration, float timeScale)
    {
        isFrozen = true;
        Time.timeScale = timeScale;
        yield return new WaitForSeconds(duration);
        Time.timeScale = 1.0f;
        isFrozen = false;
    }

}
