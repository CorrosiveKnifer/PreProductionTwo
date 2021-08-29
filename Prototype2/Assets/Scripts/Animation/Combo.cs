using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Combo : StateMachineBehaviour
{
    public bool m_comboStarter = true;

    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (m_comboStarter)
        {
            animator.SetBool("TrailActive", true);
        }
        else
        {
            animator.SetBool("TrailActive", false);
            animator.SetInteger("NextSwing", 0);
        }
    }

}