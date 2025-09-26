using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using CodeMonkey.Utils;
using System.Runtime.CompilerServices;
using Runtime.Script;

public class UI_Shop : MonoBehaviour
{
    private Transform Container;
    private Transform ShopItemTemplate;
    private IShopCustomer shopCustomer;
    

    private void Awake()
    {
        Container = transform.Find("Container");
        ShopItemTemplate = Container.Find("ShopItemTemplate");
        ShopItemTemplate.gameObject.SetActive(true);
    }   

    private void Start()
    {
        CreateItemButton(Upgrades.UpgradeType.Martelo_1, Upgrades.GetSprite(Upgrades.UpgradeType.Martelo_1), "Martelo 1", Upgrades.GetCost(Upgrades.UpgradeType.Martelo_1), 0);
        CreateItemButton(Upgrades.UpgradeType.Martelo_1, Upgrades.GetSprite(Upgrades.UpgradeType.Martelo_2), "Martelo 2", Upgrades.GetCost(Upgrades.UpgradeType.Martelo_2), 1);
    }

    private void CreateItemButton(Upgrades.UpgradeType upgradeType, Sprite upgradeSprite, string upgradeName, int upgradeCost, int positionIndex)
    {
        Transform shopItemTransform = Instantiate(ShopItemTemplate, Container);
        RectTransform shopItemRectTransform = shopItemTransform.GetComponent<RectTransform>();

        float shopItemHeight = 120f;
        shopItemRectTransform.anchoredPosition = new Vector2(0, -shopItemHeight * positionIndex);

        shopItemTransform.Find("NameText").GetComponent<TextMeshProUGUI>().SetText(upgradeName);
        shopItemTransform.Find("PriceText").GetComponent<TextMeshProUGUI>().SetText(upgradeCost.ToString());

        shopItemTransform.Find("ItemImage").GetComponent<Image>().sprite = upgradeSprite;

        shopItemTransform.GetComponent<Button_UI>().ClickFunc = () =>
        {
            TryBuyUpgrade(upgradeType);
        };
        
        
       
    }
    private void TryBuyUpgrade(Upgrades.UpgradeType upgradeType)
    {
        if (shopCustomer != null)
        {
            int cost = Upgrades.GetCost(upgradeType);
            if (shopCustomer is PlayerThird playerThir)
            {
                if (playerThir.GetXPAmount() >= cost)
                {
                    
                    shopCustomer.BoughtItem(upgradeType);
                    

                }
                else
                {
                    
                    Debug.Log("XP insuficiente para comprar o upgrade!");
                }
            }
            else
            {
                shopCustomer.BoughtItem(upgradeType);
            }
        }
    }

    public void Show(IShopCustomer shopCustomer)
    {
        this.shopCustomer = shopCustomer;
        gameObject.SetActive(true);
    }
    public void Hide()
    {
        gameObject.SetActive(false);
    }
}
