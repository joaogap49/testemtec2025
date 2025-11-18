using UnityEngine;
using UnityEngine.SceneManagement;
using Runtime.Script; // Needed to find PlayerCharacter / Player classes
using UnityEngine.UI;
using System.Collections;

// Gerencia o menu de opções / pausa do jogo.
// Anexe este componente a um Canvas persistente e atribua o painel de opções (optionsPanel).
public class MenuOptionsManager : MonoBehaviour
{
    [Tooltip("Panel (GameObject) that contains the options/pause UI. Will be enabled/disabled on ESC.")]
    // Painel que contém a UI de opções/pausa
    public GameObject optionsPanel;

    [Tooltip("Scene name used for Phase1 (third person). Matches the scene loaded in your project.)")]
    // Nome da cena usada para o modo third-person (Phase1)
    public string phase1SceneName = "PHASE1";

    [Tooltip("Scene name used for Shop (first person).")]
    // Nome da cena usada para a loja / first-person
    public string shopSceneName = "Shop";

    [Header("Blur Settings")] 
    // Controla se o desfoque dinâmico deve ser aplicado ao abrir o menu
    public bool enableBlur = true;
    [Tooltip("Divides the screen resolution for capture. Higher value = smaller texture = faster blur")] 
    // Redução de resolução para captura (performance)
    public int blurDownsample = 4;
    [Tooltip("Radius for box blur applied on the downsampled image")] 
    // Raio do desfoque (box blur)
    public int blurRadius = 2;
    [Tooltip("How many times to iterate the blur (more = stronger blur)")]
    // Quantas iterações do blur (mais = blur mais forte)
    public int blurIterations = 2;

    // Estado interno: se o menu está aberto e nome da cena atual
    private bool isOpen = false;
    private string currentSceneName;

    // Cache dos controladores do jogador para reativar ao retomar
    private MonoBehaviour cachedThirdPersonController;
    private MonoBehaviour cachedFirstPersonController;

    // UI em tempo de execução para o desfoque (RawImage) e textura borrada
    private RawImage blurRawImage;
    private Texture2D blurredTexture;

    void Start()
    {
        // Armazena o nome da cena atual
        currentSceneName = SceneManager.GetActiveScene().name;
        if (optionsPanel != null)
        {
            // Garantir painel inicialmente escondido
            optionsPanel.SetActive(false);
        }
    }

    void Update()
    {
        // Ao pressionar ESC, tenta abrir/fechar o menu se aplicável na cena atual
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (ShouldRespondInCurrentScene())
            {
                ToggleOptions();
            }
        }
    }

    // Verifica se o manager deve responder ao ESC na cena atual
    private bool ShouldRespondInCurrentScene()
    {
        // Phase1: responde somente se existir PlayerThird na cena
        if (currentSceneName == phase1SceneName)
        {
            return FindObjectOfType<PlayerThird>() != null;
        }

        // Shop: responde somente se existir um controlador first-person conhecido
        if (currentSceneName == shopSceneName)
        {
            if (FindObjectOfType<PlayerCharacter>() != null) return true;
            if (FindObjectOfType<Player>() != null) return true;
            return false;
        }

        // Em outras cenas, por padrão não responde
        return false;
    }

    // Alterna o estado do menu de opções (abre/fecha)
    private void ToggleOptions()
    {
        isOpen = !isOpen;

        if (isOpen)
        {
            if (enableBlur)
            {
                // Captura o quadro atual e aplica desfoque antes de pausar
                StartCoroutine(OpenWithBlur());
            }
            else
            {
                if (optionsPanel != null) optionsPanel.SetActive(true);
                PauseGame();
            }
        }
        else
        {
            if (optionsPanel != null) optionsPanel.SetActive(false);
            ResumeGame();
            RemoveBlur();
        }
    }

    // Coroutine que captura a tela, aplica blur e mostra o painel acima
    private IEnumerator OpenWithBlur()
    {
        // Assegura que o painel esteja inativo durante a captura
        if (optionsPanel != null) optionsPanel.SetActive(false);

        // Espera o fim do frame para garantir que a câmera terminou de renderizar
        yield return new WaitForEndOfFrame();

        Texture2D snap = CaptureScreenDownsampled();
        if (snap != null)
        {
            Texture2D blurred = BlurTexture(snap);
            CreateBlurRawImageIfNeeded();
            if (blurRawImage != null)
            {
                blurRawImage.texture = blurred;
                blurRawImage.color = Color.white;
                blurRawImage.raycastTarget = false;
                blurredTexture = blurred;
            }
            else
            {
                Destroy(blurred);
            }
        }

        // Exibe o painel acima do desfoque
        if (optionsPanel != null)
        {
            optionsPanel.SetActive(isOpen);
            Canvas canvas = optionsPanel.GetComponentInParent<Canvas>();
            if (canvas != null && blurRawImage != null)
            {
                blurRawImage.transform.SetParent(canvas.transform, false);
                int panelIndex = optionsPanel.transform.GetSiblingIndex();
                blurRawImage.transform.SetSiblingIndex(panelIndex);
                optionsPanel.transform.SetAsLastSibling();
            }

            optionsPanel.SetActive(true);
        }

        PauseGame();
    }

    // Captura a tela em uma textura reduzida conforme blurDownsample
    private Texture2D CaptureScreenDownsampled()
    {
        Camera cam = Camera.main;
        if (cam == null)
        {
            Debug.LogWarning("MenuOptionsManager: Camera.main not found, cannot capture blur.");
            return null;
        }

        int w = Mathf.Max(1, Screen.width / Mathf.Max(1, blurDownsample));
        int h = Mathf.Max(1, Screen.height / Mathf.Max(1, blurDownsample));

        RenderTexture rt = RenderTexture.GetTemporary(w, h, 0, RenderTextureFormat.Default);
        RenderTexture prev = cam.targetTexture;
        cam.targetTexture = rt;
        cam.Render();
        RenderTexture.active = rt;

        Texture2D tex = new Texture2D(w, h, TextureFormat.RGB24, false);
        tex.ReadPixels(new Rect(0, 0, w, h), 0, 0);
        tex.Apply();

        cam.targetTexture = prev;
        RenderTexture.active = null;
        RenderTexture.ReleaseTemporary(rt);

        return tex;
    }

    // Aplica um box blur simples na textura (horizontal + vertical passes)
    private Texture2D BlurTexture(Texture2D src)
    {
        if (src == null) return null;

        int w = src.width;
        int h = src.height;

        Color32[] srcColors = src.GetPixels32();
        Color32[] tempColors = new Color32[srcColors.Length];
        Color32[] dstColors = new Color32[srcColors.Length];

        int radius = Mathf.Max(0, blurRadius);
        int kernel = radius * 2 + 1;

        if (radius == 0 || blurIterations <= 0)
        {
            // Sem blur solicitado
            return src;
        }

        for (int iter = 0; iter < blurIterations; iter++)
        {
            // passagem horizontal: srcColors -> tempColors
            for (int y = 0; y < h; y++)
            {
                int baseIndex = y * w;
                int rSum = 0, gSum = 0, bSum = 0, aSum = 0;

                // janela inicial
                for (int i = -radius; i <= radius; i++)
                {
                    int xi = Mathf.Clamp(i, 0, w - 1);
                    Color32 c = srcColors[baseIndex + xi];
                    rSum += c.r; gSum += c.g; bSum += c.b; aSum += c.a;
                }

                for (int x = 0; x < w; x++)
                {
                    tempColors[baseIndex + x] = new Color32((byte)(rSum / kernel), (byte)(gSum / kernel), (byte)(bSum / kernel), (byte)(aSum / kernel));

                    int removeIndex = Mathf.Clamp(x - radius, 0, w - 1);
                    int addIndex = Mathf.Clamp(x + radius + 1, 0, w - 1);
                    Color32 rem = srcColors[baseIndex + removeIndex];
                    Color32 add = srcColors[baseIndex + addIndex];
                    rSum = rSum - rem.r + add.r;
                    gSum = gSum - rem.g + add.g;
                    bSum = bSum - rem.b + add.b;
                    aSum = aSum - rem.a + add.a;
                }
            }

            // passagem vertical: tempColors -> dstColors
            for (int x = 0; x < w; x++)
            {
                int rSum = 0, gSum = 0, bSum = 0, aSum = 0;

                for (int i = -radius; i <= radius; i++)
                {
                    int yi = Mathf.Clamp(i, 0, h - 1);
                    Color32 c = tempColors[yi * w + x];
                    rSum += c.r; gSum += c.g; bSum += c.b; aSum += c.a;
                }

                for (int y = 0; y < h; y++)
                {
                    dstColors[y * w + x] = new Color32((byte)(rSum / kernel), (byte)(gSum / kernel), (byte)(bSum / kernel), (byte)(aSum / kernel));

                    int removeIndex = Mathf.Clamp(y - radius, 0, h - 1);
                    int addIndex = Mathf.Clamp(y + radius + 1, 0, h - 1);
                    Color32 rem = tempColors[removeIndex * w + x];
                    Color32 add = tempColors[addIndex * w + x];
                    rSum = rSum - rem.r + add.r;
                    gSum = gSum - rem.g + add.g;
                    bSum = bSum - rem.b + add.b;
                    aSum = aSum - rem.a + add.a;
                }
            }

            // troca buffers para a próxima iteração
            var swap = srcColors;
            srcColors = dstColors;
            dstColors = swap;
        }

        // constrói a textura resultado (srcColors contém os dados finais)
        Texture2D result = new Texture2D(w, h, TextureFormat.RGBA32, false);
        result.SetPixels32(srcColors);
        result.Apply();

        // libera o snapshot original reduzido
        Destroy(src);

        return result;
    }

    // Cria um RawImage em tempo de execução para exibir o desfoque, se ainda não existir
    private void CreateBlurRawImageIfNeeded()
    {
        if (blurRawImage != null) return;
        if (optionsPanel == null) return;

        Canvas canvas = optionsPanel.GetComponentInParent<Canvas>();
        if (canvas == null)
        {
            Debug.LogWarning("MenuOptionsManager: Canvas not found for optionsPanel");
            return;
        }

        GameObject go = new GameObject("BlurBackground");
        go.transform.SetParent(canvas.transform, false);
        RectTransform rt = go.AddComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.anchoredPosition = Vector2.zero;
        rt.sizeDelta = Vector2.zero;

        blurRawImage = go.AddComponent<RawImage>();
        blurRawImage.raycastTarget = false;

        // coloca sob o painel de opções na hierarquia para que o painel fique acima do blur
        int panelIndex = optionsPanel.transform.GetSiblingIndex();
        go.transform.SetSiblingIndex(panelIndex);
        optionsPanel.transform.SetAsLastSibling();
    }

    // Remove/destrói a imagem e textura de desfoque criadas em tempo de execução
    private void RemoveBlur()
    {
        if (blurRawImage != null)
        {
            Destroy(blurRawImage.gameObject);
            blurRawImage = null;
        }

        if (blurredTexture != null)
        {
            Destroy(blurredTexture);
            blurredTexture = null;
        }
    }

    // Pausa o jogo: Time.timeScale = 0 e desabilita controladores apropriados
    private void PauseGame()
    {
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Desabilita controladores dependendo da cena atual
        if (currentSceneName == phase1SceneName)
        {
            cachedThirdPersonController = FindObjectOfType<PlayerThird>();
            if (cachedThirdPersonController != null)
            {
                cachedThirdPersonController.enabled = false;
            }
        }

        if (currentSceneName == shopSceneName)
        {
            MonoBehaviour fp = FindObjectOfType<PlayerCharacter>() as MonoBehaviour;
            if (fp == null) fp = FindObjectOfType<Player>() as MonoBehaviour;
            if (fp != null)
            {
                cachedFirstPersonController = fp;
                cachedFirstPersonController.enabled = false;
            }
        }
    }

    // Retoma o jogo: restaura Time.timeScale e reativa controladores previamente desabilitados
    private void ResumeGame()
    {
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (currentSceneName == phase1SceneName)
        {
            if (cachedThirdPersonController != null)
            {
                cachedThirdPersonController.enabled = true;
                cachedThirdPersonController = null;
            }
        }

        if (currentSceneName == shopSceneName)
        {
            if (cachedFirstPersonController != null)
            {
                cachedFirstPersonController.enabled = true;
                cachedFirstPersonController = null;
            }
        }
    }

    // Callbacks públicos para botões da UI
    public void Resume()
    {
        if (isOpen)
        {
            ToggleOptions();
        }
    }

    public void GoToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(0);
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private void OnDestroy()
    {
        // Limpa quaisquer texturas criadas em runtime e garante que o tempo não fique pausado
        RemoveBlur();
        Time.timeScale = 1f;
    }
}
