using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LockOnUI : MonoBehaviour
{
    // Start is called before the first frame update
    
    public Image lockOnIconUI;
    Transform target;
    public Camera mainCamera;

    private Canvas canvas;
    private RectTransform rectTransformCanvas;
    private RectTransform rectTransformIcon;



    void Start()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
            lockOnIconUI.gameObject.SetActive(false);
        }
        canvas = lockOnIconUI.GetComponentInParent<Canvas>();
        rectTransformCanvas = lockOnIconUI.GetComponentInParent<RectTransform>();
        rectTransformIcon = lockOnIconUI.GetComponent<RectTransform>();
        lockOnIconUI.gameObject.SetActive(false);

    }

    // Update is called once per frame
    void Update()
    {
        if (target == null)
        {
            if (lockOnIconUI.gameObject.activeSelf)
                lockOnIconUI.gameObject.SetActive(false);
            return;
        }

        // calcular posição do ícone
        Vector3 worldPos = target.position + Vector3.up;
        Vector3 screenPos = mainCamera.WorldToScreenPoint(worldPos);

        Camera camForConversion = (canvas.renderMode == RenderMode.ScreenSpaceOverlay) ? null : mainCamera;

        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransformCanvas, screenPos, camForConversion, out localPoint);

        rectTransformIcon.anchoredPosition = localPoint;

        if (!lockOnIconUI.gameObject.activeSelf)
            lockOnIconUI.gameObject.SetActive(true);
    }

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
        if (target != null)
            lockOnIconUI.gameObject.SetActive(true);
    }

    public void DisableTarget()
    {
        target = null;
        
    }
}