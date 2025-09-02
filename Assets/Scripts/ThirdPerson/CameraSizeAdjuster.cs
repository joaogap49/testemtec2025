using Cinemachine;
using UnityEngine;

public class CameraSizeAdjuster : MonoBehaviour
{
    public CinemachineVirtualCamera virtualCamera;
    public float sizeMultiplier = 1.2f;

    void Start()
    {
        if (virtualCamera == null)
            virtualCamera = GetComponent<CinemachineVirtualCamera>();

        AdjustCameraToFitLevel();
    }

    void AdjustCameraToFitLevel()
    {
        // Encontra todos os objetos renderizáveis para calcular os limites
        Renderer[] allRenderers = FindObjectsOfType<Renderer>();
        if (allRenderers.Length == 0) return;

        Bounds combinedBounds = allRenderers[0].bounds;
        foreach (Renderer renderer in allRenderers)
        {
            combinedBounds.Encapsulate(renderer.bounds);
        }

        // Calcula o tamanho necessário
        float maxSize = Mathf.Max(combinedBounds.extents.x, combinedBounds.extents.z);
        float requiredSize = maxSize * sizeMultiplier;

        // Ajusta a câmera
        virtualCamera.m_Lens.OrthographicSize = requiredSize;
        virtualCamera.m_Lens.FarClipPlane = requiredSize * 4f;
    }
}