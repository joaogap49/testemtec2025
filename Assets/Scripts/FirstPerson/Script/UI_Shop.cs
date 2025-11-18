using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using CodeMonkey.Utils;
using System.Runtime.CompilerServices;
using Runtime.Script;

// Script responsável por gerenciar a interface da loja (UI)
public class UI_Shop : MonoBehaviour
{
    // Referências internas para o container e o template dos itens da loja
    private Transform Container;
    private Transform ShopItemTemplate;
    private IShopCustomer shopCustomer;

    [Header("Layout")]
    // Posição inicial e espaçamento entre itens na UI (ajustável pelo inspector)
    [SerializeField] private Vector2 shopItemStartPosition = new Vector2(308f, 0f);
    [SerializeField] private Vector2 shopItemSpacing = new Vector2(0f, -120f);

    // Armazena as transforms criadas para cada tipo de upgrade para facilitar atualizações da UI
    private Dictionary<Upgrades.UpgradeType, Transform> shopItemTransforms = new Dictionary<Upgrades.UpgradeType, Transform>();

    private void Awake()
    {
        // Busca referências dentro da hierarquia do GameObject
        Container = transform.Find("Container");
        ShopItemTemplate = Container.Find("ShopItemTemplate");
        // Mantemos o template INATIVO para evitar que ele fique sobrepondo os itens criados
        // e usaremos clones do template ativados individualmente.
        ShopItemTemplate.gameObject.SetActive(false);
    }

    private void Start()
    {
        // Cria botões/linhas para cada upgrade disponível
        CreateItemButton(Upgrades.UpgradeType.Forca, Upgrades.GetSprite(Upgrades.UpgradeType.Forca), "Força", 0);
        CreateItemButton(Upgrades.UpgradeType.Defesa, Upgrades.GetSprite(Upgrades.UpgradeType.Defesa), "Defesa", 1);
        CreateItemButton(Upgrades.UpgradeType.Velocidade, Upgrades.GetSprite(Upgrades.UpgradeType.Velocidade), "Velocidade", 2);
        CreateItemButton(Upgrades.UpgradeType.Estamina, Upgrades.GetSprite(Upgrades.UpgradeType.Estamina), "Estamina", 3);

        // Atualiza os valores iniciais (preço/nível) na UI
        foreach (var kv in shopItemTransforms)
        {
            RefreshShopItem(kv.Key);
        }
    }

    // Cria uma entrada da loja para o tipo de upgrade informado
    private void CreateItemButton(Upgrades.UpgradeType upgradeType, Sprite upgradeSprite, string upgradeName, int positionIndex)
    {
        // Instancia um clone do template dentro do Container
        Transform shopItemTransform = Instantiate(ShopItemTemplate, Container);

        // ATENÇÃO: quando o template estiver inativo, o clone também nasce inativo.
        // Por isso precisamos ativar explicitamente o clone para que ele apareça na UI,
        // mantendo o template original inativo (para não sobrepor os itens criados).
        shopItemTransform.gameObject.SetActive(true);

        RectTransform shopItemRectTransform = shopItemTransform.GetComponent<RectTransform>();

        // Posiciona o item usando posição inicial e espaçamento configuráveis
        shopItemRectTransform.anchoredPosition = shopItemStartPosition + shopItemSpacing * positionIndex;

        // Define nome e imagem do item
        shopItemTransform.Find("NameText").GetComponent<TextMeshProUGUI>().SetText(upgradeName);
        shopItemTransform.Find("ItemImage").GetComponent<Image>().sprite = upgradeSprite;

        // Ao clicar, tenta comprar o upgrade correspondente
        shopItemTransform.GetComponent<Button_UI>().ClickFunc = () =>
        {
            TryBuyUpgrade(upgradeType);
        };

        // Armazena a transform criada para atualizações posteriores
        shopItemTransforms[upgradeType] = shopItemTransform;
    }

    // Atualiza a exibição de um item da loja (preço, estado MAX, interação)
    private void RefreshShopItem(Upgrades.UpgradeType upgradeType)
    {
        if (!shopItemTransforms.ContainsKey(upgradeType)) return;
        Transform shopItemTransform = shopItemTransforms[upgradeType];
        var priceText = shopItemTransform.Find("PriceText")?.GetComponent<TextMeshProUGUI>();
        var levelText = shopItemTransform.Find("LevelText")?.GetComponent<TextMeshProUGUI>();
        var button = shopItemTransform.GetComponent<Button_UI>();

        int level = Upgrades.GetLevel(upgradeType);

        // Define cores:
        // Nível 0 -> neutro (cinza-claro/branco)
        // Níveis 1-4 -> verde
        // Nível máximo -> dourado/amarelo
        Color neutralColor = new Color(0.9f, 0.9f, 0.9f);
        Color greenColor = new Color(0.18f, 0.8f, 0.25f);
        Color goldColor = new Color(1f, 0.84f, 0f);

        Color chosenColor = neutralColor;
        if (level == 0)
        {
            chosenColor = neutralColor;
        }
        else if (level >= 1 && level < Upgrades.MAX_LEVEL)
        {
            chosenColor = greenColor;
        }
        else if (level >= Upgrades.MAX_LEVEL)
        {
            chosenColor = goldColor;
        }

        if (Upgrades.IsMaxLevel(upgradeType))
        {
            // Se já atingiu o nível máximo, mostra "MAX" e remove ação de clique
            priceText?.SetText("MAX");
            // Mostra nível final também
            levelText?.SetText($"Nível {level}");
            // Aplica cor dourada no texto de nível
            if (levelText != null) levelText.color = chosenColor;
            button.ClickFunc = null;
        }
        else
        {
            // Caso contrário, mostra o preço do próximo nível
            int cost = Upgrades.GetCost(upgradeType);
            priceText?.SetText(cost.ToString());
            // Garante que o clique esteja configurado para tentar comprar
            button.ClickFunc = () => TryBuyUpgrade(upgradeType);

            // Atualiza o texto de nível atual e aplica cor conforme nível
            if (levelText != null)
            {
                levelText.SetText($"Nível {level}");
                levelText.color = chosenColor;
            }

            // Observação: aqui poderíamos desabilitar visualmente o botão se o jogador não tiver XP suficiente
            if (shopCustomer is PlayerCharacter pc)
            {
                // Exemplo: aplicar cor cinza se pc.GetXPAmount() < cost
            }
        }
    }

    // Lógica para tentar comprar um upgrade quando o jogador clica no botão
    private void TryBuyUpgrade(Upgrades.UpgradeType upgradeType)
    {
        if (shopCustomer != null)
        {
            int cost = Upgrades.GetCost(upgradeType);
            if (shopCustomer is PlayerThird playerThir)
            {
                // Verifica se o jogador tem XP suficiente antes de autorizar a compra
                if (playerThir.GetXPAmount() >= cost)
                {
                    shopCustomer.BoughtItem(upgradeType);
                    // Atualiza a UI após a compra
                    RefreshShopItem(upgradeType);
                }
                else
                {
                    Debug.Log("XP insuficiente para comprar o upgrade!");
                }
            }
            else
            {
                // Caso o cliente da loja não seja um PlayerThird, realiza a ação sem checar XP aqui
                shopCustomer.BoughtItem(upgradeType);
                // Atualiza a UI após a ação
                RefreshShopItem(upgradeType);
            }
        }
    }

    // Exibe a UI da loja e atualiza todos os itens
    public void Show(IShopCustomer shopCustomer)
    {
        this.shopCustomer = shopCustomer;
        gameObject.SetActive(true);

        // Torna o cursor visível e desbloqueado para permitir interação com a UI
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        // Refresca todos os itens ao abrir a loja
        foreach (var kv in shopItemTransforms)
        {
            RefreshShopItem(kv.Key);
        }
    }

    // Esconde a UI da loja
    public void Hide()
    {
        gameObject.SetActive(false);

        // Ao fechar, esconde e trava o cursor novamente (modo primeira pessoa)
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }
}
