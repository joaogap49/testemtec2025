using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IShopCustomer
{
    void BoughtItem(Upgrades.UpgradeType upgradeType);
    //bool TrySpendXPAmout(int amount);
}
