using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Script responsável por gerenciar a stamina do jogador, incluindo drenagem, regeneração e atualização da UI.
public class Stamina : MonoBehaviour
{
    [Header("Stamina Main Parameters")]
    public float playerStamina = 100f; // Valor atual da stamina do jogador.
    [SerializeField] private float maxStamina = 100f; // Valor máximo de stamina.
    [HideInInspector] public bool hasRegenerated = true; // Indica se a stamina já regenerou totalmente.
    [HideInInspector] public bool weAreSprinting = false; // Indica se o jogador está tentando correr.
    [HideInInspector] public bool sprintingActive = false; // Indica se o sprint está ativo.

    [Header("Stamina Regen Parameter")]
    [Range(0, 50)][SerializeField] private float staminDrain = 10f; // Taxa de drenagem da stamina por segundo ao correr.
    [Range(0, 50)][SerializeField] private float staminRegen = 5f; // Taxa de regeneração da stamina por segundo ao não correr.

    [Header("Stamina Speed Parameters")]
    [SerializeField] private int slowedRunSpeed = 4; // Velocidade reduzida ao ficar sem stamina (não utilizado diretamente aqui).
    [SerializeField] private int normalRunSpeed = 8; // Velocidade normal de corrida (não utilizado diretamente aqui).

    [Header("Stamina UI Elements")]
    [SerializeField] private Image staminaProgressUI; // Referência ao componente de UI que mostra a barra de stamina.
    [SerializeField] private CanvasGroup sliderCanvasGroup; // Grupo de canvas para manipular a visibilidade da barra (opcional).

    // Só pode correr se a stamina for maior que 1/3 do máximo
    public bool CanSprint => playerStamina > (maxStamina / 3f);

    // Método para consumir stamina ao correr.
    public void UseStamina(float deltaTime)
    {
        if (playerStamina > 0)
        {
            playerStamina -= staminDrain * deltaTime;
            if (playerStamina < 0) playerStamina = 0;
        }
    }

    // Método para regenerar stamina ao não correr.
    public void RegenerateStamina(float deltaTime)
    {
        if (playerStamina < maxStamina)
        {
            playerStamina += staminRegen * deltaTime;
            if (playerStamina > maxStamina) playerStamina = maxStamina;
        }
    }

    // Atualiza a stamina a cada frame, drenando ou regenerando conforme o estado.
    void Update()
    {
        // Permite drenagem até stamina zerar
        if (weAreSprinting && playerStamina > 0)
        {
            UseStamina(Time.deltaTime);
        }
        else
        {
            RegenerateStamina(Time.deltaTime);
        }

        UpdateStamina();
    }

    // Atualiza a UI da stamina e controla a visibilidade da barra
    private void UpdateStamina()
    {
        if (staminaProgressUI != null)
            staminaProgressUI.fillAmount = playerStamina / maxStamina;

        // A barra só some quando a stamina está cheia
        if (sliderCanvasGroup != null)
        {
            if (playerStamina >= maxStamina)
                sliderCanvasGroup.alpha = 0;
            else
                sliderCanvasGroup.alpha = 1;
        }
    }

    // Gerencia o estado de sprint do jogador
    // Parâmetros:
    // - wantsToSprint: indica se o jogador está pressionando a tecla de correr (ex: Shift).
    // - SprintSpeed: velocidade máxima de corrida.
    // - currentSpeed: referência para a velocidade atual do jogador (será alterada aqui).
    // - isSprinting: referência para o estado de corrida do jogador (será alterado aqui).
    // - rotateSpeed: referência para a velocidade de rotação do jogador (será alterada aqui).
    public void HandleSprint(
        bool wantsToSprint,
        float SprintSpeed,
        ref float currentSpeed,
        ref bool isSprinting,
        ref float rotateSpeed)
    {
        // Se o jogador quer correr e tem stamina suficiente para iniciar o sprint
        if (wantsToSprint && playerStamina > (maxStamina / 3f) && !sprintingActive)
        {
            sprintingActive = true;
        }
        // Se o jogador soltar o sprint ou stamina zerar, desativa o sprint
        if (!wantsToSprint || playerStamina <= 0)
        {
            sprintingActive = false;
        }

        if (sprintingActive && playerStamina > 0)
        {
            currentSpeed = SprintSpeed;
            isSprinting = true;
            rotateSpeed = 5f;
            weAreSprinting = true;
        }
        else
        {
            isSprinting = false;
            rotateSpeed = 10f;
            weAreSprinting = false;
        }
    }
}