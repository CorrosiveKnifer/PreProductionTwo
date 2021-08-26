using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boss_Cinematic : MonoBehaviour
{
    public Boss_AI m_boss;
    
    private bool m_isShowTime = false;

    private void Update()
    {
        if(m_isShowTime && !CameraManager.instance.IsDirectorPlaying("BossRoar"))
        {
            m_boss.WakeUp();
            Destroy(gameObject);
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Player")
        {
            CameraManager.instance.PlayDirector("BossRoar");
            m_isShowTime = true;
        }
    }
}
