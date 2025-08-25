using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShopTriggerCollider : MonoBehaviour
{
    [SerializeField] private UI_Shop uiShop;
    private void OnTriggerEnter(Collider collider)
    {
        Debug.Log("OnTriggerEnter: " + collider.name);
        IShopCustomer shopCustomer = collider.GetComponent<IShopCustomer>();

        if (shopCustomer != null)
        {
            Debug.Log("Player entrou no trigger e é IShopCustomer");
            uiShop.Show(shopCustomer);
        }
    }
            
    private void OnTriggerExit(Collider collider)
    {
        IShopCustomer shopCustomer = collider.GetComponent<IShopCustomer>();

        if (shopCustomer != null)
        {
            uiShop.Hide();
        }
    }
}
