using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DadController : MovingObject
{
    [SerializeField] float maxSpeed;
    [SerializeField] float walkSpeed;
    [SerializeField] float acceleration;

    [SerializeField] Transform insidePosition;
    [SerializeField] Transform endGamePosition;
    
    [SerializeField] SceneManager sceneManager;
    [SerializeField] PlayerController1 player;
    [SerializeField] BoxCollider autoTalkCollider;
    [SerializeField] [TextArea(3, 10)] string[] endingSpeakingText = new string[5];

    Animator animator;
    TalkingObject talkingObj;
    
    public int segment = 0;

    Vector3 startingPosition;
    float startMovingTime;
    

    void Start()
    {
        Init(walkSpeed, maxSpeed, acceleration);
        talkingObj = GetComponent<TalkingObject>();
        animator = GetComponent<Animator>();
        animator.SetBool("Angry", true);
    }
    
    void FixedUpdate()
    {
        //starts standing there not talking waiting for the game to start. doing angry dance maybe?
        if (segment == 0)
        {
            talkingObj.SetTalkToKeyActive(false);

            if (talkingObj.isTalking)
            {
                animator.SetBool("Angry", false);
                segment++;
            }
        }

        //then once the camera pans over to him, the scenemanager activates the auto talk, 
        //and TalkingObject makes him start talking and animating his talking.
        else if (segment == 1)
        {
            if (!talkingObj.isTalking)
            {
                StartWalkingInside();
                segment++;
            }
        }

        //then once he finishes talking, he is disabled as a talking object and starts walking inside the building.
        else if (segment == 2)
        {
            if (Vector3.Distance(transform.position, insidePosition.position) <= 0.5f) segment++;
            else Move(insidePosition.position - transform.position, false);
        }

        //he stays inside building entire game until all adepts are caught
        else if (segment == 3)
        {
            if (sceneManager.AdeptsCount <= 0)
            {
                StartWalkingOutside();
                segment++;
            }
        }

        //once all adepts are caught he walks back outside again, and after reaching his spot, he turns his talking object back on
        //and replaces the words in it with the ending words.
        else if (segment == 4)
        {
            if (Vector3.Distance(transform.position, endGamePosition.position) <= 0.5f)
            {
                tag = "TalkingTownsFolk";
                GetComponent<CapsuleCollider>().enabled = false;
                GetComponent<CapsuleCollider>().enabled = true;
                animator.SetBool("Walk", false);
                segment++;
            }
            else Move(endGamePosition.position - transform.position, false);
        }

        //the player talks to him, and after you finish talking to him...
        else if (segment == 5)
        {
            if (talkingObj.isTalking) segment++;
        }

        //the game ends
        else if (segment == 6)
        {
            if (!talkingObj.isTalking)
            {
                sceneManager.OnEnd();
                segment++;

            }
        }

        //This is a case where I really should have just used my state machine, but was making it during my last week on this...
        //and this is just one of the things that had to end up below my preferred level or tidy code =(
    }

    public void StartWalkingInside()
    {
        tag = "Untagged";
        animator.SetBool("Walk", true);
        autoTalkCollider.enabled = false;
        player.CheckFocusedTalkingObject();
        startMovingTime = Time.time;
    }

    public void StartWalkingOutside()
    {
        startMovingTime = Time.time;
        GetComponent<TalkingObject>().speakingText = endingSpeakingText;
    }
}
