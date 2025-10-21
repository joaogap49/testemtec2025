using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PointFadeVisual : MonoBehaviour
{
    [Header("Configurar Fade")]
    private float fadeInDuration = 0.2f;
    private float fadeOutDuration = 0.3f;
    private float visibleDuration = 0.8f;

    [Header("Configurar Cor")]
    public Color[] colors = new Color[]
    {
        Color.white,
        new Color(0.8f, 1.0f, 1.0f),
        new Color(0.6f, 1.0f, 1.0f),
        new Color(0.4f, 0.8f, 1f),
        new Color(0.6f, 1f, 1f),
        new Color(0.8f, 1.0f, 1.0f),
        Color.white
    };
    public float hueSpeed = 1.0f;
    public float animationTimer;


    private TextMeshProUGUI text;
    private Color color;
    // Adicione esta função utilitária

    private void Start()
    {
        text = GetComponent<TextMeshProUGUI>();
        color = text.color;
        StartCoroutine(FadeAnimation());
        StartCoroutine(AnimatePopUp());
    }// Start is called before the first frame update
   
    private IEnumerator FadeAnimation()
    {
        float elapsed = 0;
        text.color = new Color(color.r, color.g, color.b, 0f);
        while (elapsed < fadeInDuration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / fadeInDuration;
            float currentAlpha = Mathf.Lerp(0f, 1f, progress);
            text.color = new Color(color.r, color.g, color.b, progress);
            yield return null;

        }
        
        yield return new WaitForSeconds(visibleDuration);
        
        while(elapsed < fadeOutDuration)
        {
            elapsed -= Time.deltaTime;
            float progress = elapsed / fadeOutDuration;
            float currentAlpha = Mathf.Lerp(1f, 0f, progress);
            text.color = new Color(color.r, color.g, color.b, progress);
            yield return null;
        }
        
        Destroy(gameObject);
    }

    private IEnumerator AnimatePopUp()
    {
        float totalDuration = fadeInDuration + fadeOutDuration + visibleDuration;
        animationTimer = 0f;
        while (animationTimer < totalDuration)
        {
            animationTimer += Time.deltaTime;
            float fadeProgress = animationTimer / fadeInDuration;
            float hueProgress = animationTimer / totalDuration * hueSpeed;
            float currenAlpha = Mathf.Lerp(0f, 1f, fadeProgress);
            Color currentColor = GetCurrentColor(hueProgress);
            text.color = new Color(currentColor.r, currentColor.g, currentColor.b, currenAlpha);
            yield return null;
        }

        float visibleStartTime = animationTimer;
        while (animationTimer < visibleStartTime + visibleDuration)
        {
            animationTimer += Time.deltaTime;
            float hueProgress = animationTimer / totalDuration * hueSpeed;

            // Apenas shift de matiz (alpha permanece 1)
            Color currentColor = GetCurrentColor(hueProgress);
            text.color = new Color(currentColor.r, currentColor.g, currentColor.b, 1f);

            yield return null;
        }





        float fadeOutStartTime = animationTimer; 
        while(animationTimer < fadeOutDuration + fadeOutStartTime)
        {
            animationTimer += Time.deltaTime;

            // Calcula progressos separados
            float fadeProgress = (animationTimer - fadeOutStartTime) / fadeOutDuration;
            float hueProgress = animationTimer / totalDuration * hueSpeed;

            // Aplica fade out
            float currentAlpha = Mathf.Lerp(1f, 0f, fadeProgress);

            // Aplica shift de matiz
            Color currentColor = GetCurrentColor(hueProgress);
            text.color = new Color(currentColor.r, currentColor.g, currentColor.b, currentAlpha);
        }
        
        
        yield return null;
    }
    private Color GetCurrentColor(float progress)
    {
        // Garante que progress está entre 0 e 1
        progress = Mathf.Clamp01(progress);

        // Se tivermos apenas uma cor, retorna ela
        if (colors.Length == 1)
            return colors[0];

        // Calcula entre quais cores estamos interpolando
        float colorIndex = progress * (colors.Length - 1);
        int indexA = Mathf.FloorToInt(colorIndex);
        int indexB = Mathf.Min(indexA + 1, colors.Length - 1);

        // Calcula o progresso entre as duas cores
        float blend = colorIndex - indexA;

        // Interpola entre as duas cores
        return Color.Lerp(colors[indexA],colors[indexB], blend);
    }

    // Método para configurar cores customizadas (opcional)
    public void SetColorSequence(Color[] customColors)
    {
        if (customColors != null && customColors.Length > 0)
        {
            colors = customColors;
        }
    }

}
