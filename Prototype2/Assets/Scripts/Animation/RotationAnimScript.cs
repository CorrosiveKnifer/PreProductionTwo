using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotationAnimScript : StateMachineBehaviour
{
    [Range(0, 360, order = 0)]
    public float rotationAmount;

    public int heirarchyDepth = 0;

    public string boolLimit;

    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        animator.SetBool("CanRotate", false);
    }

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    //override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    
    //}

    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        Transform theParent = animator.transform;
        for (int i = 0; i < heirarchyDepth; i++)
        {
            theParent = theParent.transform?.parent;
        }
        theParent.Rotate(new Vector3(0, 1, 0), rotationAmount);
        animator.SetBool(boolLimit, false);
        animator.SetBool("CanRotate", true);
    }

    // OnStateMove is called right after Animator.OnAnimatorMove()
    //override public void OnStateMove(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    // Implement code that processes and affects root motion
    //}

    // OnStateIK is called right after Animator.OnAnimatorIK()
    //override public void OnStateIK(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    // Implement code that sets up animation IK (inverse kinematics)
    //}
}
