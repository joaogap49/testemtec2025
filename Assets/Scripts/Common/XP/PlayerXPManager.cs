using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerXPManager : MonoBehaviour
{
    public static PlayerXPManager Instance { get; private set; }
    public int XP { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        SetXP(2500); // Valor preestabelecido pra testar a loja
    }

    public void AddXP(int amount)
    {
        XP += amount;
    }

    public bool TrySpendXP(int amount)
    {
        if (XP >= amount)
        {
            XP -= amount;
            return true;
        }
        return false;
    }

    public void SetXP(int amount)
    {
        XP = amount;
    }
}
