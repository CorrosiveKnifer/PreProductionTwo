using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class Boss_Camera : MonoBehaviour
{
    private CinemachineVirtualCamera vCamera;
    private Coroutine m_shaker = null;
    // Start is called before the first frame update
    void Awake()
    {
        vCamera = GetComponent<CinemachineVirtualCamera>();
    }

    public void Shake(float _intensity, float _time)
    {
        if(m_shaker != null)
        {
            StopCoroutine(m_shaker);
        }

        m_shaker = StartCoroutine(Shaker(_intensity, _time));
    }

    private IEnumerator Shaker(float _intensity, float _time)
    {
        float time = _time;

        CinemachineBasicMultiChannelPerlin channel = vCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
        channel.m_AmplitudeGain = _intensity;

        while (time > 0)
        {
            yield return new WaitForEndOfFrame();
            time -= Time.deltaTime;
        }
        channel.m_AmplitudeGain = 0.0f;
        m_shaker = null;
        yield return null;
    }
}
