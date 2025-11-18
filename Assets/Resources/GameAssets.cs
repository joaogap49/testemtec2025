using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameAssets : MonoBehaviour
{
    private static GameAssets _i;

    public static GameAssets i
    {
        get
        {
            if (_i == null) _i = Instantiate(Resources.Load<GameAssets>("GameAssets"));
            return _i;
        }
    }
    public Sprite s_Forca;
    public Sprite s_Defesa;
    public Sprite s_Velocidade;
    public Sprite s_Estamina;
}
