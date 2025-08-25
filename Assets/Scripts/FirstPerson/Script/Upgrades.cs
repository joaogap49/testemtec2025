using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

public class Upgrades
{
    public enum UpgradeType
    {
        Martelo_1,
        Martelo_2
    }
    public static int GetCost(UpgradeType itemType)
    {
        switch (itemType)
        {
            default:
            case UpgradeType.Martelo_1: return 100;
            case UpgradeType.Martelo_2: return 200;
        }
    }
    public static Sprite GetSprite(UpgradeType itemType)
    {
        switch (itemType)
        {
            default:
            case UpgradeType.Martelo_1: return GameAssets.i.s_Martelo_1;
            case UpgradeType.Martelo_2: return GameAssets.i.s_Martelo_2;
        }
    }
}
