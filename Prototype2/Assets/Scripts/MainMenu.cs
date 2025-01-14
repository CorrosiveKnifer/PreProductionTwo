﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Cinemachine;

public class MainMenu : MonoBehaviour
{
    public string m_sceneName = "MainGameScene";
    public CinemachineVirtualCamera vCamera;
    public Animator doors;
    public GameObject player;
    public GameObject m_tutorialText;
    public Button m_playButton;

    [Header("Settings")]
    public GameObject m_settings;
    public GameObject m_settingsButton;
    public Slider m_masterVolume;
    public Slider m_soundEffectVolume;
    public Slider m_musicVolume;
    private GameObject m_lastSelected;
    private bool ignore = true;
    //Dictionary<Slider,AudioManager.VolumeChannel> m_sliders = new Dictionary<Slider, AudioManager.VolumeChannel>();

    // Start is called before the first frame update
    void Start()
    {
        m_playButton.Select();
        HUDManager.instance.gameObject.SetActive(false);
        player = GameObject.FindGameObjectWithTag("Player");
        player.GetComponent<PlayerController>().m_functionalityEnabled = false;
        m_tutorialText.SetActive(false);
        m_masterVolume.value = AudioManager.instance.volumes[0];
        m_soundEffectVolume.value = AudioManager.instance.volumes[1];
        m_musicVolume.value = AudioManager.instance.volumes[2];
        ignore = false;
    }
    // Update is called once per frame
    void Update()
    {
        //if (InputManager.instance.IsGamepadButtonDown(ButtonType.SOUTH, 0) || InputManager.instance.IsKeyDown(KeyType.SPACE))
        //{
        //    StartGame();
        //}
        if(InputManager.instance.IsGamepadButtonDown(ButtonType.NORTH, 0) || InputManager.instance.IsKeyDown(KeyType.T))
        {
            GameManager.instance.enableTimer = true;
            GetComponent<SoloAudioAgent>().Play();
        }
        AudioManager.instance.volumes[0] = m_masterVolume.value;        
        AudioManager.instance.volumes[1] = m_soundEffectVolume.value;
        AudioManager.instance.volumes[2] = m_musicVolume.value;
    }

    public void ChangeSliderNumber(Slider _slider)
    {
        //float newValue = _slider.value;
        //_slider.GetComponentInChildren<Text>().text = ((int)(newValue * 100.0f)).ToString();
        //AudioManager.instance.volumes[(int)m_sliders[_slider]] = newValue;
        //
        //Debug.Log(m_sliders[_slider].ToString() + ": " + AudioManager.instance.volumes[(int)m_sliders[_slider]]);
        if(!ignore)
        {
            GetComponent<SoloAudioAgent>().Play();
        }
    }

    public void StartGame()
    {
        HUDManager.instance.gameObject.SetActive(true);
        m_tutorialText.SetActive(true);
        doors.SetBool("IsOpen", true);
        vCamera.Priority = 0;
        player.GetComponent<PlayerController>().m_functionalityEnabled = true;
        Destroy(gameObject);

        //LevelLoader.instance.LoadNewLevel(m_sceneName);
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
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #endif
        LevelLoader.instance.QuitGame();
    }
}
