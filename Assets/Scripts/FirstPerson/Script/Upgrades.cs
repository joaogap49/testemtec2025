using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

// Classe estática responsável por gerenciar informações dos upgrades do jogo
public class Upgrades
{
    // Tipos de upgrade disponíveis
    public enum UpgradeType
    {
        Forca,
        Defesa,
        Estamina,
        Velocidade
    }

    // Nível máximo permitido por upgrade
    public const int MAX_LEVEL = 5;

    // Prefixo de chave para PlayerPrefs
    private const string PlayerPrefsKeyPrefix = "UpgradeLevel_";

    // Dicionário que armazena o nível atual de cada upgrade.
    // Os níveis começam em 0 (sem upgrades aplicados).
    private static Dictionary<UpgradeType, int> upgradeLevels = new Dictionary<UpgradeType, int>()
    {
        { UpgradeType.Forca, 0 },
        { UpgradeType.Defesa, 0 },
        { UpgradeType.Estamina, 0 },
        { UpgradeType.Velocidade, 0 }
    };

    // Construtor estático: carrega os níveis salvos ao iniciar a classe
    static Upgrades()
    {
        LoadAllLevels();
    }

    // Retorna o nível atual do upgrade (valor entre 0 e MAX_LEVEL)
    public static int GetLevel(UpgradeType itemType)
    {
        return upgradeLevels[itemType];
    }

    // Retorna true se o upgrade já estiver no nível máximo
    public static bool IsMaxLevel(UpgradeType itemType)
    {
        return upgradeLevels[itemType] >= MAX_LEVEL;
    }

    // Incrementa o nível do upgrade em 1, se não estiver no máximo.
    // Retorna true se o nível foi incrementado com sucesso.
    public static bool IncreaseLevel(UpgradeType itemType)
    {
        if (IsMaxLevel(itemType)) return false;
        // Garante que o valor fique no intervalo válido [0, MAX_LEVEL]
        upgradeLevels[itemType] = Mathf.Clamp(upgradeLevels[itemType] + 1, 0, MAX_LEVEL);
        // Salva imediatamente o novo nível
        SaveLevel(itemType);
        return true;
    }

    // Salva o nível de um upgrade no PlayerPrefs
    private static void SaveLevel(UpgradeType itemType)
    {
        string key = PlayerPrefsKeyPrefix + itemType.ToString();
        PlayerPrefs.SetInt(key, upgradeLevels[itemType]);
        PlayerPrefs.Save();
    }

    // Carrega o nível de um upgrade do PlayerPrefs
    private static int LoadLevel(UpgradeType itemType)
    {
        string key = PlayerPrefsKeyPrefix + itemType.ToString();
        return PlayerPrefs.GetInt(key, 0);
    }

    // Carrega todos os níveis salvos e atualiza o dicionário
    private static void LoadAllLevels()
    {
        foreach (UpgradeType ut in System.Enum.GetValues(typeof(UpgradeType)))
        {
            int lvl = LoadLevel(ut);
            upgradeLevels[ut] = Mathf.Clamp(lvl, 0, MAX_LEVEL);
        }
    }

    /*
     Fórmula de preço solicitada (recursiva):
     Preço(Nível) = Preço(Nível-1) + (IncrementoBase + (Aumento * (Nível - 1)))

     Implementação utilizada aqui:
     - `basePrice` é o preço do nível 1 (Preço(1)).
     - `incrementBase` é o IncrementoBase da fórmula.
     - `increase` é o Aumento da fórmula.

     Observações sobre indexação de níveis:
     - Internamente os níveis são armazenados começando em 0 (0 = sem upgrade).
     - Ao comprar o próximo nível, o `targetLevel` é `currentLevel + 1` (1-based para a fórmula).
    */
    public static int GetCost(UpgradeType itemType)
    {
        // Se já está no nível máximo, retornamos 0 (indicando sem custo / indisponível)
        if (IsMaxLevel(itemType))
        {
            return 0;
        }

        int currentLevel = GetLevel(itemType); // nível atual (0-based)
        int targetLevel = currentLevel + 1; // nível que será comprado (1-based)

        // Parâmetros configuráveis da fórmula (ajuste conforme necessário):
        const float basePrice = 100f;         // Preço do nível 1 (Preço(1))
        const float incrementBase = 50f;      // IncrementoBase
        const float increase = 25f;           // Aumento (taxa de incremento por nível)

        // Começamos com o preço do nível 1
        float price = basePrice;

        // Se targetLevel for 1, não há termos adicionais; caso contrário, aplicamos a soma recursiva
        // para chegar no preço do nível alvo.
        for (int level = 2; level <= targetLevel; level++)
        {
            // Para cada nível n (>=2) adicionamos: incrementBase + (increase * (n - 1))
            price += (incrementBase + (increase * (level - 1)));
        }

        // Arredonda para inteiro e retorna como custo
        return Mathf.RoundToInt(price);
    }

    // Retorna o sprite associado ao tipo de upgrade (usa GameAssets do projeto)
    public static Sprite GetSprite(UpgradeType itemType)
    {
        switch (itemType)
        {
            default:
            case UpgradeType.Forca: return GameAssets.i.s_Forca;
            case UpgradeType.Defesa: return GameAssets.i.s_Defesa;
            case UpgradeType.Estamina: return GameAssets.i.s_Estamina;
            case UpgradeType.Velocidade: return GameAssets.i.s_Velocidade;

        }
    }

    // Método utilitário para debug: limpa os níveis salvos (útil em desenvolvimento)
    public static void ResetAllLevels()
    {
        foreach (UpgradeType ut in System.Enum.GetValues(typeof(UpgradeType)))
        {
            string key = PlayerPrefsKeyPrefix + ut.ToString();
            PlayerPrefs.DeleteKey(key);
            upgradeLevels[ut] = 0;
        }
        PlayerPrefs.Save();
    }
}
