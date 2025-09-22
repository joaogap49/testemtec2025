using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShopTriggerCollider : MonoBehaviour
{
    [SerializeField] private AudioSource openingAudio;
    [SerializeField] private AudioSource closingAudio;
    [SerializeField] private UI_Shop uiShop;
    private void OnTriggerEnter(Collider collider)
    {
        openingAudio.Play();
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
        closingAudio.Play();
        IShopCustomer shopCustomer = collider.GetComponent<IShopCustomer>();

        if (shopCustomer != null)
        {
            uiShop.Hide();
        }
    }
}
