using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WalkingTownsFolk : MovingObject
{
    static readonly int STUCK_THRESHOLD = 15;

    public Transform walkingPath;
    public bool oneWay = true;
    public int startingStop = 0;
    public float movementSpeed;
    public float acceleration;
    public float turnSpeed;

    bool initialized = false;
    bool overrideWaitUntil = false;

    PathingObject pathing;
    Animator animator;
    FootTrafficManager trafficManager;

    Vector3 nextPosition;
    float offSet = 0;
    Vector3 adjustedOffset = Vector3.zero;

    Vector3 amIStuckPosition;
    int amIStuckCounter = 0;

    Transform currentDestination;
    StopData currentStopData;
    int currentStop = 0;
    bool finishedLoop = true;
    bool walking = true;
    bool turning = true;
    float waitStartTime;

    void Awake()
    {
        animator = GetComponent<Animator>();
        pathing = GetComponent<PathingObject>();
        nextPosition = transform.position;
        Init(movementSpeed, 0, acceleration);
        amIStuckPosition = transform.position;
        initialized = true && !oneWay;
    }

    void Update()
    {
        if (!initialized) return;

        if (finishedLoop)
        {
            currentDestination = walkingPath.GetChild(currentStop);
            currentStopData = currentDestination.GetComponent<StopData>();
            animator.SetBool("Walk", true);

            overrideWaitUntil = false;
            walking = true;
            turning = true;

            finishedLoop = false;
        }

        if (walking)
        {
            MoveTowardsTranform(currentDestination);

            if (Vector3.Distance(transform.position, currentDestination.position + adjustedOffset) <= 0.75f || overrideWaitUntil)
                walking = false;
        }
        else if (turning) {
            PivotToMatch(currentDestination);

            if (Quaternion.Angle(transform.rotation, currentDestination.rotation) <= 5 || overrideWaitUntil)
            {
                turning = false;
                animator.SetBool("Walk", false);
                if (currentStopData.timeToWait != 0)
                {
                    waitStartTime = Time.time;
                    animator.SetBool(currentStopData.actionAtThisStop, true);
                }
            }
        }
        else
        {
            if (currentStopData.timeToWait == 0 || (Time.time - waitStartTime) >= currentStopData.timeToWait)
            {
                if (currentStopData.timeToWait != 0)
                    animator.SetBool(currentStopData.actionAtThisStop, false);

                if (++currentStop >= walkingPath.childCount)
                {
                    if (!oneWay) currentStop = 0;
                    else Destroy(gameObject);
                }

                finishedLoop = true;
            }
        }
    }

    public void OneWayInit(Transform wp, int ss, float os, FootTrafficManager tm)
    {
        walkingPath = wp;
        startingStop = ss;
        offSet = os;
        trafficManager = tm;
        initialized = true;
    }

    void MoveTowardsTranform(Transform target)
    {
        if (target == null) { Debug.Log("No Target Found..."); return; }

        float signedAngle = Vector3.SignedAngle(Vector3.forward, transform.forward, Vector3.up) % 90;

        if (Mathf.Abs(signedAngle) < 10) adjustedOffset = transform.right * offSet;

        Vector3 adjustedDirection = (target.position + adjustedOffset - transform.position).normalized;

        //This section is just to make sure the walking townsfolk don't just sit there indefinitely stuck trying to walk somewhere.
        if (Vector3.Distance(transform.position, amIStuckPosition) <= 0.05f) amIStuckCounter++;
        else amIStuckCounter = 0;

        if (amIStuckCounter > STUCK_THRESHOLD)
        {
            if (Vector3.Distance(transform.position, target.position) < 5 && target != walkingPath.GetChild(walkingPath.childCount - 1))
                overrideWaitUntil = true;
            else if (pathing.IsPositionWalkable(transform.position + adjustedDirection * 2))
            {
                transform.position = transform.position + adjustedDirection * 2;
                nextPosition = transform.position;
            }

            amIStuckCounter = 0;
        }
        amIStuckPosition = transform.position;

        if (Vector3.Distance(transform.position, nextPosition) <= 0.5f)
        {
            int tryDistance = 0;

            if (Vector3.Distance(target.position, transform.position) < 5)
                nextPosition = pathing.GetNextPathPosition(target.position + adjustedOffset, 1);
            else nextPosition = pathing.GetNextPathPosition(transform.position + adjustedDirection * 5, 1);

            while (nextPosition == transform.position && ++tryDistance < 10)
                nextPosition = pathing.GetNextPathPosition(transform.position + adjustedDirection * tryDistance, 1);

            //if the next position is the end point for this leg of the walk,
            //and they are within 10 of it, but can't reach it or anywhere in between, then just move on.
            if (nextPosition == transform.position && Vector3.Distance(target.position, transform.position) < 10)
                overrideWaitUntil = true;
        }

        Vector3 direction = nextPosition - transform.position;
        direction.y = 0;

        Move(direction, false);
    }

    void PivotToMatch(Transform target) => transform.rotation = Quaternion.RotateTowards(transform.rotation, target.rotation, turnSpeed);
    
    void OnDestroy() => trafficManager?.ReduceCounter();
}
