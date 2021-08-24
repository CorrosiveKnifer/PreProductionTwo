using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class MainMenu : MonoBehaviour
{
    public string m_sceneName = "MainGameScene";

    [Header("Settings")]
    public GameObject m_settings;
    public GameObject m_settingsButton;
    public Slider m_masterVolume;
    public Slider m_soundEffectVolume;
    public Slider m_musicVolume;
    private GameObject m_lastSelected;

    Dictionary<Slider,AudioManager.VolumeChannel> m_sliders = new Dictionary<Slider, AudioManager.VolumeChannel>();

    // Start is called before the first frame update
    void Start()
    {
        m_settings.SetActive(false);
        m_sliders.Add(m_masterVolume, AudioManager.VolumeChannel.MASTER);
        m_sliders.Add(m_soundEffectVolume, AudioManager.VolumeChannel.SOUND_EFFECT);
        m_sliders.Add(m_musicVolume, AudioManager.VolumeChannel.MUSIC);

        foreach (var slider in m_sliders)
        {
            slider.Key.value = AudioManager.instance.volumes[(int)slider.Value];
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ChangeSliderNumber(Slider _slider)
    {
        float newValue = _slider.value;
        _slider.GetComponentInChildren<Text>().text = ((int)(newValue * 100.0f)).ToString();
        AudioManager.instance.volumes[(int)m_sliders[_slider]] = newValue;

        Debug.Log(m_sliders[_slider].ToString() + ": " + AudioManager.instance.volumes[(int)m_sliders[_slider]]);
    }

    public void StartGame()
    {
        LevelLoader.instance.LoadNewLevel(m_sceneName);
    }
    public void Settings()
    {
        m_settings.SetActive(!m_settings.activeInHierarchy);
        if (m_settings.activeInHierarchy)
        {
            EventSystem.current.SetSelectedGameObject(m_masterVolume.gameObject);
        }
        else
        {
            EventSystem.current.SetSelectedGameObject(m_settingsButton);
        }
    }
    public void QuitGame()
    {
        LevelLoader.instance.QuitGame();
    }
}
