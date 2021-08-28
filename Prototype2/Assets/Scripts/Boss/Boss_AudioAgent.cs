using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boss_AudioAgent : MultiAudioAgent
{
    public void PlayStep()
    {
        base.PlayOnce("BossFootStep", false, Random.Range(0.85f, 1.25f));
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
