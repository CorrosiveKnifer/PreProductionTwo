using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boss_AudioAgent : MultiAudioAgent
{
    public void PlayHurt()
    {
        base.Play($"BossHurt{Random.Range(0, 3)}", false, Random.Range(0.9f, 1.1f));
    }
    public void PlayMad()
    {
        base.PlayDelayed("BossMad", 0.6f, false, Random.Range(0.9f, 1.1f));
    }
    public void PlayKick()
    {
        base.Play("KickSE", false, 1.0f);
    }
    public void PlayStep()
    {
        base.PlayOnce("BossFootStep", false, Random.Range(0.85f, 1.25f));
    }
    public void PlayCastFireBall()
    {
        base.Play("FireballCast", false, Random.Range(0.90f, 1.1f));
    }
    public void PlaySlam()
    {
        base.Play("BossSlam", false, Random.Range(0.90f, 1.1f));
    }
    public void PlaySlash()
    {
        base.Play("HeavySlash", false, Random.Range(0.90f, 1.1f));
    }
    public void PlayBossDeath(int index)
    {
        if(index == 0)
            base.Play("BossDeath", false, 1.0f);
        else
            base.Play("BossDeath2", false, Random.Range(0.90f, 1.1f));
    }
}
