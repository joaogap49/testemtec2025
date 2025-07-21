using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackReceiver : MonoBehaviour
{
    public EnemyAttack attack;

    public void DealDamage()
    {
        if (attack != null)
        {
            //attack.DealDamage();
        }
    }
}
