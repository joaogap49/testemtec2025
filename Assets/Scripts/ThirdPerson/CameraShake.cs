using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class CameraShake : MonoBehaviour
{
    private CinemachineVirtualCamera cam;
    private CinemachineBasicMultiChannelPerlin noise;

    void Start()
    {
        cam = GetComponent<CinemachineVirtualCamera>();
        noise = cam.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
        
    }
    public void Shake(float amplitude, float frequency, float duration)
    {
        StartCoroutine(ShakeCoroutine(amplitude, frequency, duration)); 
    }
    private IEnumerator ShakeCoroutine(float amplitude, float frequency, float duration)
    {
        noise.m_AmplitudeGain = amplitude;
        noise.m_FrequencyGain = frequency;
        yield return new WaitForSeconds(duration);
        noise.m_FrequencyGain = 0;
        noise.m_AmplitudeGain = 0;
    }
}
