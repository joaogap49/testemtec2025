using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

public class TotalPointsEffects : MonoBehaviour
{
    public int totalXPValue;
    
    
    void Update()
    {
        PlayerXPManager player = FindObjectOfType<PlayerXPManager>();

        if (player != null)
        {
            totalXPValue = player.XP;
            //Debug.Log("XP do collection: " + totalXPValue);
        }

        // CHAMADA CORRETA DA FUNÇÃO ESTÁTICA
        string xpText = PointEffect.FormatXP(totalXPValue);

        // Remove '+' caso exista no começo (queremos mostrar o total sem '+')
        if (!string.IsNullOrEmpty(xpText) && xpText.StartsWith("+"))
        {
            xpText = xpText.Substring(1);
        }

        // Verifique se tem componente TextMeshPro
        TMPro.TextMeshProUGUI textComponent = GetComponent<TMPro.TextMeshProUGUI>();
        
        if (textComponent != null)
        {
            //Debug.Log("Texto encontrado: " + textComponent.text);
            // Use o texto formatado
            textComponent.text = xpText; // ← AQUI: use xpText sem '+'
            //Debug.Log("Texto após modificação: " + textComponent.text);
        }
        else
        {
            //Debug.LogError("NENHUM componente TextMeshProUGUI encontrado no pop-up!");

            // Liste todos os componentes para debug
            Component[] allComponents = GetComponents<Component>();
            foreach (Component comp in allComponents)
            {
                //Debug.Log("Componente: " + comp.GetType().Name);
            }
        }
    }
}
