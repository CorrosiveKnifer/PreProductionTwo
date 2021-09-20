using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AdrenalineProvider : MonoBehaviour
{
    public float m_durationInSeconds = 5;
    public PlayerMovement m_playerRef;

    private float m_maxDuration;

    // Start is called before the first frame update
    void Start()
    {
        m_maxDuration = m_durationInSeconds;
    }

    private void Update()
    {
        m_durationInSeconds -= Time.deltaTime;

        if(m_durationInSeconds <= 0)
        {
            Destroy(gameObject);
        }
    }

    public void GiveAdrenaline()
    {
        if(m_playerRef != null)
        {
            m_playerRef.GiveAdrenaline(m_durationInSeconds / m_maxDuration);
            Destroy(gameObject);
            return;
        }
        Debug.LogError($"Provider didn't have reference to player.");
    }
}
