using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PointEffect : MonoBehaviour
{
    
    public static string FormatXP(int xp)
    {
        if (xp >= 1000)
        {
            return "+" + (xp / 1000f).ToString("F1") + "K";
        }
        else
        {
            return "+" + xp.ToString();
        }
    }

    // No seu script de pop-up ou EnemyHealth:
    
}
