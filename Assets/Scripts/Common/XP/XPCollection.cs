using UnityEngine;

public class XPCollection : MonoBehaviour
{
    public int xpAmount; // XP que esse coletável concede

    private void OnTriggerEnter(Collider other)
    {
        PlayerThird player = other.GetComponent<PlayerThird>();
        if (player != null)
        {
            Debug.Log("XP coletado: " + xpAmount);
            player.AddXP(xpAmount);

            Destroy(gameObject); // Destroi o coletável após pegar
        }
    }
}
