using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AOEAdrenalineProvider : PlayerAdrenalineProvider
{
    public GameObject m_player;

    public float m_currentWindow = 0.0f;
    public float m_maxWindow = 0.0f;
    public float m_modifier = 1.0f;
    
    public void StartAdrenalineWindow(float window)
    {
        m_maxWindow = window;
        m_currentWindow = window;
        m_value = 1.0f * m_modifier;
        m_player.GetComponent<PlayerMovement>().SetPotentialAdrenaline(this);
    }

    // Update is called once per frame
    void Update()
    {
        if (m_currentWindow >= 0.0f)
        {
            m_currentWindow -= Time.deltaTime;
            m_value = (m_currentWindow / m_maxWindow) * m_modifier;
        }
        else
        {
            m_value = 0.0f;
        }
    }
}
