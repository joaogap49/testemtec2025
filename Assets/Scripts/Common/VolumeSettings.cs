using UnityEngine.Audio;
using UnityEngine;
using UnityEngine.UI;
using KinematicCharacterController.Walkthrough._10__Multiple_movement_states_setup.Scripts;

public class VolumeSettings : MonoBehaviour
{
    [SerializeField] private AudioMixer myMixer;
    [SerializeField] private Slider musicSlider;

    private void Start()
    {
        if (PlayerPrefs.HasKey("soundVolume"))
        {
            LoadVolume();
        }
        else 
        {
            SetVolume();
        }
            
    }
    public void SetVolume()
    {
        float volume = musicSlider.value;
        myMixer.SetFloat("Sound", Mathf.Log10(volume)*20);
        PlayerPrefs.SetFloat("soundVolume", volume);
    }
    private void LoadVolume()
    {
        musicSlider.value = PlayerPrefs.GetFloat("soundVolume");
        SetVolume();
    }
}
