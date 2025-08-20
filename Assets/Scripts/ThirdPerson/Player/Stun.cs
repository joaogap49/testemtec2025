using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stun : MonoBehaviour
{
    float stunDuration = 0.2f;
    public bool isStunned = false;
    PlayerThird playerThird;

    private void Start()
    {
        playerThird = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerThird>();
    }

    public void ApplyStun()
    {
        if(!isStunned)
        {
            StartCoroutine(StunCoroutine());
        }
    }
    IEnumerator StunCoroutine()
    {
        isStunned= true;

        if (playerThird != null)
        {
            playerThird.SetStunned(true);

        }
        yield return new WaitForSeconds(stunDuration);

        isStunned = false;
        if (playerThird != null)
        {

            playerThird.SetStunned(false);

        }
    }
}
