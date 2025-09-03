using UnityEngine;
using Runtime.Script; // Certifique-se de usar o namespace correto para PlayerCharacter

public class XPCollection : MonoBehaviour
{
    public int xpAmount = 1; // XP que esse coletável concede

    private void OnTriggerEnter(Collider other)
    {
        PlayerThird player = other.GetComponent<PlayerThird>();
        if (player != null)
        {
            player.AddXP(xpAmount);
            Destroy(gameObject); // Destroi o coletável após pegar
        }
    }
}
