using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParentAnimationSync : StateMachineBehaviour
{
    public int heirarchyDepth = 0;

    private Transform updatingParent;
    private Transform connectionParent;
    private GameObject player;

    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        animator.SetBool("CanRotate", false);
        updatingParent = animator.transform;
        for (int i = 0; i < heirarchyDepth; i++)
        {
            updatingParent = updatingParent.transform?.parent;
        }
        player = GameObject.FindGameObjectWithTag("Player");
        connectionParent = animator.transform.parent;
        animator.transform.parent = null;
        animator.transform.rotation = updatingParent.transform.rotation;
    }

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        updatingParent.position = animator.transform.position;
    }

    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
   override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
   {
        animator.transform.parent = connectionParent;
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
