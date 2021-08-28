using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boss_Cinematic : MonoBehaviour
{
    public Boss_AI m_boss;
    public SoloAudioAgent m_mainGameMusic;
    public SoloAudioAgent m_bossMusic;
    public Animator m_door;

    private bool m_isShowTime = false;
    private PlayerController player;
    private void Update()
    {
        if(m_isShowTime && !CameraManager.instance.IsDirectorPlaying("BossRoar"))
        {
            m_boss.WakeUp();
            player.m_functionalityEnabled = true;
            m_door.SetBool("IsOpen", false);
            Destroy(gameObject);
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Player")
        {
            CameraManager.instance.PlayDirector("BossRoar");
            player = other.GetComponent<PlayerController>();
            player.m_functionalityEnabled = false;
            player.m_cameraController.m_camera.m_XAxis.Value = 270f;
            player.m_cameraController.m_camera.m_YAxis.Value = 0.5f;
            m_mainGameMusic.PauseWithFadeOut(0.5F);
            m_bossMusic.PlayWithFadeIn(1.5f);
            m_isShowTime = true;
        }
    }
}
