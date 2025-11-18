using UnityEngine;
using UnityEngine.SceneManagement;
using Runtime.Script; // Needed to find PlayerCharacter / Player classes
using UnityEngine.UI;
using System.Collections;

// Simple options/pause menu manager. Attach to a persistent Canvas GameObject in your scene
// and assign the optionsPanel (the UI Panel to show when ESC is pressed).
public class MenuOptionsManager : MonoBehaviour
{
    [Tooltip("Panel (GameObject) that contains the options/pause UI. Will be enabled/disabled on ESC.")]
    public GameObject optionsPanel;

    [Tooltip("Scene name used for Phase1 (third person). Matches the scene loaded in your project.)")]
    public string phase1SceneName = "PHASE1";

    [Tooltip("Scene name used for Shop (first person).")]
    public string shopSceneName = "Shop";

    [Header("Blur Settings")] 
    public bool enableBlur = true;
    [Tooltip("Divides the screen resolution for capture. Higher value = smaller texture = faster blur")] 
    public int blurDownsample = 4;
    [Tooltip("Radius for box blur applied on the downsampled image")] 
    public int blurRadius = 2;
    [Tooltip("How many times to iterate the blur (more = stronger blur)")]
    public int blurIterations = 2;

    private bool isOpen = false;
    private string currentSceneName;

    // cached player controllers so we can re-enable them when resuming
    private MonoBehaviour cachedThirdPersonController;
    private MonoBehaviour cachedFirstPersonController;

    // runtime blur UI
    private RawImage blurRawImage;
    private Texture2D blurredTexture;

    void Start()
    {
        currentSceneName = SceneManager.GetActiveScene().name;
        if (optionsPanel != null)
        {
            optionsPanel.SetActive(false);
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (ShouldRespondInCurrentScene())
            {
                ToggleOptions();
            }
        }
    }

    private bool ShouldRespondInCurrentScene()
    {
        // Phase1: only respond if a third person player exists
        if (currentSceneName == phase1SceneName)
        {
            return FindObjectOfType<PlayerThird>() != null;
        }

        // Shop: only respond if a first person player exists (check common first-person player classes)
        if (currentSceneName == shopSceneName)
        {
            if (FindObjectOfType<PlayerCharacter>() != null) return true;
            if (FindObjectOfType<Player>() != null) return true;
            return false;
        }

        // For other scenes, do not open by default. You can change this if wanted.
        return false;
    }

    private void ToggleOptions()
    {
        isOpen = !isOpen;

        if (isOpen)
        {
            if (enableBlur)
            {
                // capture current frame and apply blur before pausing
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

    private IEnumerator OpenWithBlur()
    {
        // ensure options panel inactive while we capture the scene behind it
        if (optionsPanel != null) optionsPanel.SetActive(false);

        // wait for end of frame so camera finished rendering
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

        // show panel above blur
        if (optionsPanel != null)
        {
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
            // no blur requested
            return src;
        }

        for (int iter = 0; iter < blurIterations; iter++)
        {
            // horizontal pass: srcColors -> tempColors
            for (int y = 0; y < h; y++)
            {
                int baseIndex = y * w;
                int rSum = 0, gSum = 0, bSum = 0, aSum = 0;

                // initial window
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

            // vertical pass: tempColors -> dstColors
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

            // swap buffers for next iteration
            var swap = srcColors;
            srcColors = dstColors;
            dstColors = swap;
        }

        // build result texture (srcColors currently contains latest)
        Texture2D result = new Texture2D(w, h, TextureFormat.RGBA32, false);
        result.SetPixels32(srcColors);
        result.Apply();

        // free the original small snapshot
        Destroy(src);

        return result;
    }

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

        // place under the options panel in hierarchy so panel is visible above blur
        int panelIndex = optionsPanel.transform.GetSiblingIndex();
        go.transform.SetSiblingIndex(panelIndex);
        optionsPanel.transform.SetAsLastSibling();
    }

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

    private void PauseGame()
    {
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Disable player controllers to prevent input while paused
        cachedThirdPersonController = FindObjectOfType<PlayerThird>();
        if (cachedThirdPersonController != null)
        {
            cachedThirdPersonController.enabled = false;
        }

        MonoBehaviour fp = FindObjectOfType<PlayerCharacter>() as MonoBehaviour;
        if (fp == null) fp = FindObjectOfType<Player>() as MonoBehaviour;
        if (fp != null)
        {
            cachedFirstPersonController = fp;
            cachedFirstPersonController.enabled = false;
        }
    }

    private void ResumeGame()
    {
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (cachedThirdPersonController != null)
        {
            cachedThirdPersonController.enabled = true;
            cachedThirdPersonController = null;
        }

        if (cachedFirstPersonController != null)
        {
            cachedFirstPersonController.enabled = true;
            cachedFirstPersonController = null;
        }
    }

    // Public UI callbacks
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
        // cleanup any runtime textures
        RemoveBlur();
        Time.timeScale = 1f;
    }
}
