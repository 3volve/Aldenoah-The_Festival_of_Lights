using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationBasedSpeedThrottle : StateMachineBehaviour
{
    Rigidbody rigidbody;
    float maxMS;
    float minMS;

    float timeCounter; //however many updates the animation takes to finish.
    float halfwayPoint;
    float slowProportion = 1;
    float currentSpeed;

    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        PriestController priest = animator.GetComponent<PriestController>();
        rigidbody = animator.GetComponent<Rigidbody>();
        maxMS = priest.maxSpeed;
        minMS = priest.walkSpeed;
        timeCounter = (int)(67 / stateInfo.speed);
        //Debug.Log("counter length: " + timeCounter);
        halfwayPoint = timeCounter / 2;
        slowProportion = 1;
    }

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        timeCounter -= Time.deltaTime;

        if (timeCounter <= 0)
        {
            //Debug.Log("Counter Exit to LookingBehind");
            animator.SetBool("LookingBack", false);
        }

        slowProportion = Mathf.Clamp01(
            Mathf.Abs(timeCounter - halfwayPoint) / halfwayPoint + 
            (1 - Mathf.Abs(timeCounter - halfwayPoint) / halfwayPoint) * 0.5f
            );

        currentSpeed = Mathf.Lerp(minMS, maxMS, slowProportion);

        rigidbody.velocity = Vector3.ClampMagnitude(rigidbody.velocity, currentSpeed);
    }

    //OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        animator.SetBool("LookingBack", false);
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
