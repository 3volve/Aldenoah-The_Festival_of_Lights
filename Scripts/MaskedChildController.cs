using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.ScriptableObject;

public class MaskedChildController : MovingObject
{
    #pragma warning disable CS0649 // #warning directive
    [SerializeField] float maxSpeed;
    [SerializeField] float walkSpeed;
    [SerializeField] float acceleration;
    [SerializeField] float visionArc = 90;
    [SerializeField] float visionRange;
    [SerializeField] Transform head;

    [SerializeField] TalkingSpotNavigator talkingSpot;
    [SerializeField] GameObject emptyTransform;

    public Transform Target { private set; get; } = null;
    HashSet<Transform> priestsInRange = new HashSet<Transform>();
    HashSet<Transform> childrenInRange = new HashSet<Transform>();

    static int timeToWaitAfterLosingPriest = 200;
    static int timeToWaitAfterLosingChild = 100;

    /*  These are all used for a tripping animation that I made for the children and never ended up having time to implement
    readonly float timeToFallDown = 1f;
    readonly float timeToSwitchPosition = 0.1f;
    readonly float timeToGetUp = 5.5f;
    bool isGettingUp = false;
    */

    bool running = false;
    bool active = true;
    bool isStanding = true;
    bool returningHome = false;

    Animator animator;
    Animator spareAnimator;

    StateMachine<MaskedChildController> state;
    PathingObject pathing;
    Vector3 nextPosition;

    GameObject startPosition;

    void Start()
    {
        state = CreateInstance<IdleState>();
        state.OnStateEnter(this);
        pathing = GetComponent<PathingObject>();
        nextPosition = transform.position;

        GetComponentInChildren<SphereCollider>().radius = visionRange;

        if (timeToWaitAfterLosingPriest == 0)
            timeToWaitAfterLosingPriest = Mathf.RoundToInt(1 / Time.fixedDeltaTime) * 3;
        if (timeToWaitAfterLosingChild == 0)
            timeToWaitAfterLosingChild = Mathf.RoundToInt(1 / Time.fixedDeltaTime) * 5;

        Init(walkSpeed, maxSpeed, acceleration);
        animator = GetComponentsInChildren<Animator>()[0];
        spareAnimator = GetComponentsInChildren<Animator>()[0];

        startPosition = Instantiate(emptyTransform, transform.position, transform.rotation);
    }

    void FixedUpdate()
    {
        if (returningHome) ReturnHomeUpdate();
        else if (state != null) state.UpdateState();
        if (active) CalculateSight();

        InternalUpdate();
    }

    /************************************ Private Methods **************************************/
    void CalculateSight()
    {
        if (priestsInRange.Count == 0 && childrenInRange.Count == 0) return;

        bool iSeeATarget = false;
        Vector3 headDirection = head.forward;
        Vector3 otherObjectDirection;
        headDirection.y = 0;
        float minDistance = float.MaxValue;
        Transform nullPriest = null;

        foreach (Transform priest in priestsInRange)
        {
            if (priest.GetComponent<PriestController>() == null)
            {
                nullPriest = priest;
                continue;
            }

            otherObjectDirection = priest.position - transform.position;

            if (Vector3.Angle(otherObjectDirection, headDirection) <= visionArc / 2)
            {
                if (!Physics.Linecast(transform.position, priest.position, LayerMask.GetMask("BuildingsBlocking")))
                {
                    float curDistance = Vector3.Distance(transform.position, priest.position);
                    if (curDistance < minDistance)
                    {
                        Target = priest;
                        minDistance = curDistance;
                    }

                    iSeeATarget = true;
                }
            }
        }

        if (nullPriest != null) priestsInRange.Remove(nullPriest);

        if (!iSeeATarget)
        {
            foreach (Transform child in childrenInRange)
            {
                if (!child.GetComponent<MovingObject>().Running) continue;

                otherObjectDirection = child.position - transform.position;

                if (Vector3.Angle(otherObjectDirection, headDirection) <= visionArc / 2)
                {
                    if (!Physics.Linecast(transform.position, child.position, LayerMask.GetMask("BuildingsBlocking")))
                    {
                        float curDistance = Vector3.Distance(transform.position, child.position);
                        if (curDistance < minDistance && curDistance >= 0.25f)
                        {
                            Target = child;
                            minDistance = curDistance;
                        }

                        iSeeATarget = true;
                    }
                }
            }
        }

        if (!iSeeATarget) Target = null;
    }

    void ReturnHomeUpdate()
    {
        if (Vector3.Distance(transform.position, startPosition.transform.position) > 0.5)
            Move(startPosition.transform);
        else if (Vector3.Angle(transform.forward, startPosition.transform.forward) > 10)
            transform.rotation = Quaternion.RotateTowards(
                        transform.rotation,
                        startPosition.transform.rotation,
                        5
                    );
        else returningHome = false;

        if (Target?.tag == "Follower") returningHome = false;
    }


    /************************************ Public Methods ***************************************/
    public void Move(Transform target)
    {
        if (target == null) return;

        if (Vector3.Distance(transform.position, nextPosition) <= 0.2f)
            nextPosition = pathing.GetNextPathPosition(target);

        Vector3 direction = nextPosition - transform.position;
        
        direction.y = 0;

        //if (direction.magnitude > 0 && !isStanding && !isGettingUp) StartCoroutine(GettingUp());

        if (Vector3.Angle(target.position - transform.position, direction) > 90)
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation,
                Quaternion.LookRotation(target.position - transform.position),
                15
                );

        if (isStanding) Move(direction, running);
    }

    public void TriggerReturningHome()
    {
        Target = null;
        returningHome = true;
        nextPosition = transform.position;
    }


    /************************************ OnContact Unity Methods ****************************/
    void OnTriggerEnter(Collider other)
    {
        if (name.StartsWith("Small"))
        {
            /*
            if (other.tag == "TrippingObject")
            {
                if (Random.Range(0, 100) < 75)
                    FallOver();
            }
            else if (other.tag == "MapBoundary")
            {
                //something to stop in their tracks and turn around?
            }
            */
        }

        if (other.tag.EndsWith("Follower"))
            priestsInRange.Add(other.transform);
        else if (other.tag == "MaskedChildren" || other.tag == "Player")
            childrenInRange.Add(other.transform);
    }

    void OnTriggerExit(Collider other)
    {
        if (other.tag.EndsWith("Follower"))
            priestsInRange.Remove(other.transform);
        else if (other.tag == "MaskedChildren" || other.tag == "Player")
            childrenInRange.Remove(other.transform);
    }


    /************************************ Getting Up Methods **************************** This was a feature that I didn't end up having the time to implement..
    IEnumerator GettingUp()
    {
        isGettingUp = true;
        //Debug.Log("before " + Time.time);

        yield return new WaitForSeconds(timeToFallDown); //wait until models are in correct position before resetting position.

        animator.SetBool("FallOver", false);
        animator.transform.position = spareAnimator.transform.GetChild(3).position;
        //Debug.Log("in between " + Time.time);
        yield return new WaitForSeconds(timeToSwitchPosition); //wait until models are in correct position to begin getting up.

        //Debug.Log("after " + Time.time);
        //limbs are slightly off from laying down position, and there is probably a way to fix that if I use IK if I feel like it.
        SwitchModels();
        spareAnimator.SetBool("FallOver", false);

        yield return new WaitForSeconds(timeToGetUp);

        GotUp();

        //Debug.Log("isStanding = " + isStanding + ", isGettingUp = " + isGettingUp);
        transform.DetachChildren();
        transform.position = spareAnimator.transform.position = animator.transform.position;
        animator.transform.parent = transform;
        spareAnimator.transform.parent = transform;
    }

    public void GotUp() => isStanding = !(isGettingUp = false);

    public void SwitchModels()
    {
        animator.transform.localScale *= animator.transform.localScale.y == 1 ? 0.001f : 1000;
        spareAnimator.transform.localScale *= spareAnimator.transform.localScale.y == 1 ? 0.001f : 1000;
    }

    protected void FallOver()
    {
        //Debug.Log("blah");
        SwitchModels();
        animator.SetBool("FallOver", true);
        spareAnimator.SetBool("FallOver", true);
        isStanding = false;
    }


    /************************************ State Machine ****************************************/
    internal class IdleState : StateMachine<MaskedChildController>
    {
        MaskedChildController child;

        internal override void OnStateEnter(MaskedChildController controller)
        {
            child = controller;
            child.state = this;
            child.running = false;
            child.active = true;
            child.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
        }

        internal override void UpdateState()
        {
            Transform currentTarget = child.Target;

            if (currentTarget != null)
            {
                if (currentTarget.tag.EndsWith("Follower")) OnStateExit(CreateInstance<ChasingPriestState>());
                else if (currentTarget.GetComponent<MovingObject>().Running)
                    OnStateExit(CreateInstance<ChasingChildState>());
            }
            
            //I need to add some idle looking around them animation, maybe walking around aimlessly in some way?
        }

        internal override void OnStateExit(StateMachine<MaskedChildController> nextState)
        {
            //Debug.Log("Exiting Idle...");
            nextState.OnStateEnter(child);
            Destroy(this);
        }
    }

    internal class ChasingPriestState : StateMachine<MaskedChildController>
    {
        MaskedChildController child;

        int lostPriestCounter = timeToWaitAfterLosingPriest;

        Transform targetPriest;
        Transform SetTargetPriest {
            set {
                lostPriestCounter = timeToWaitAfterLosingPriest;
                targetPriest = value;
            }
        }

        internal override void OnStateEnter(MaskedChildController controller)
        {
            child = controller;
            child.state = this;
            child.running = true;
        }

        internal override void UpdateState()
        {
            Transform currentTarget = child.Target;

            if (currentTarget == null || !currentTarget.tag.EndsWith("Follower"))
                lostPriestCounter--;
            else SetTargetPriest = currentTarget;

            if (lostPriestCounter > 0 && targetPriest != null)
            {
                float distanceFromPriest = Vector3.Distance(child.transform.position, targetPriest.position);

                if (targetPriest.tag == "Follower" || distanceFromPriest > 2)
                    child.Move(targetPriest);
                else if (targetPriest.tag != "Follower")
                    child.Move(targetPriest.position - child.transform.position, false);
                else if (distanceFromPriest < 1f)
                    child.Move(child.transform.position - targetPriest.position, false);
                else
                {
                    child.transform.rotation = Quaternion.RotateTowards(
                        child.transform.rotation,
                        Quaternion.LookRotation(targetPriest.position - child.transform.position, Vector3.up),
                        10
                    );
                }

                if (targetPriest.tag.StartsWith("Walking") || targetPriest.tag.StartsWith("Story"))
                {
                    child.Target = targetPriest;
                    OnStateExit(CreateInstance<CaughtPriestState>());
                }
            }
            else if (currentTarget == null) OnStateExit(CreateInstance<IdleState>());
            else OnStateExit(CreateInstance<ChasingChildState>());
        }

        internal override void OnStateExit(StateMachine<MaskedChildController> nextState)
        {
            //Debug.Log("Exiting Priest Chasing...");
            nextState.OnStateEnter(child);
            Destroy(this);
        }
    }

    internal class ChasingChildState : StateMachine<MaskedChildController>
    {
        MaskedChildController child;

        int lostTargetCounter = timeToWaitAfterLosingChild;

        Transform validTarget;
        Transform SetValidTarget {
            set {
                lostTargetCounter = timeToWaitAfterLosingChild;
                validTarget = value;
            }
        }

        internal override void OnStateEnter(MaskedChildController controller)
        {
            child = controller;
            child.state = this;
            child.running = true;
        }

        internal override void UpdateState()
        {
            Transform currentTarget = child.Target;

            if (currentTarget == null)
                lostTargetCounter--;
            else if (currentTarget.tag.EndsWith("Follower"))
            {
                OnStateExit(CreateInstance<ChasingPriestState>());
                return;
            }
            else
            {
                bool isRunning = currentTarget.GetComponent<MovingObject>().Running;

                if (currentTarget.tag == "MaskedChild" && currentTarget.name != name && isRunning)
                    SetValidTarget = currentTarget;//.GetComponent<MaskedChildController>().Target;
                else if (isRunning)
                    SetValidTarget = currentTarget;
            }

            if (lostTargetCounter > 0 && validTarget != null)
            {
                float distanceFromTarget = Vector3.Distance(child.transform.position, validTarget.position);

                if (distanceFromTarget > 1.5f)
                    child.Move(validTarget);
                else if (distanceFromTarget < 0.5f)
                    child.Move(child.transform.position - validTarget.position, false);
                else
                {
                    child.transform.rotation = Quaternion.RotateTowards(
                        child.transform.rotation,
                        Quaternion.LookRotation(validTarget.position - child.transform.position, Vector3.up),
                        10
                    );
                }
            }
            else OnStateExit(CreateInstance<IdleState>());
        }

        internal override void OnStateExit(StateMachine<MaskedChildController> nextState)
        {
            //Debug.Log("Exiting Child Chasing...");
            nextState.OnStateEnter(child);
            Destroy(this);
        }
    }

    internal class CaughtPriestState : StateMachine<MaskedChildController>
    {
        MaskedChildController child;

        float clappingTime = 1;
        bool arrivedAtStory = false;
        bool arrivedAtSeatingSpot = false;
        bool isClapping = false;

        Transform seatingSpot;
        Transform target;
        FollowerStoryData followerData;

        internal override void OnStateEnter(MaskedChildController controller)
        {
            child = controller;
            child.state = this;
            child.running = false;
            child.active = false;
            child.CalculateSight();
            target = child.Target;

            followerData = child.Target?.GetComponent<FollowerStoryData>();
        }

        internal override void UpdateState()
        {
            if (!arrivedAtStory)
            {
                if (target == null)
                {
                    if (child.Target == null)
                    {
                        child.TriggerReturningHome();
                        OnStateExit(CreateInstance<IdleState>());
                    }
                    else target = child.Target;
                }

                if (Vector3.Distance(child.transform.position, target.position) > 1.5f)
                    child.Move(target);
                //else if (Vector3.Angle(target.forward, child.transform.forward) > 135)
                    //child.Move(target.forward, child.running);
                    
                if (target.tag == "StoryFollower")
                {
                    seatingSpot = child.talkingSpot.RecieveMaskedChildMessage(child.Target.name);

                    if (seatingSpot == null)
                    {
                        child.TriggerReturningHome();
                        OnStateExit(CreateInstance<IdleState>());
                    }

                    arrivedAtStory = true;
                }
            }
            else if (!arrivedAtSeatingSpot)
            {
                child.Move(seatingSpot);

                if (Vector3.Distance(child.transform.position, seatingSpot.position) < 0.5f)
                    arrivedAtSeatingSpot = true;
                //Debug.Log("Child is walking towards target name of: " + child.Target.name);
            }
            else if (Vector3.Angle(child.transform.forward, seatingSpot.forward) > 10)
            {
                child.transform.rotation = Quaternion.RotateTowards(
                        child.transform.rotation,
                        seatingSpot.rotation,
                        10
                    );

                if (Vector3.Angle(child.transform.forward, seatingSpot.forward) <= 10)
                {
                    child.animator.SetFloat("StartOffset", Random.Range(0.0f, 1.0f));
                    child.animator.SetInteger("WaitingForStory", Random.Range(1, 3));
                }
            }
            else if (followerData.hasToldStory && !isClapping)
            {
                child.animator.SetInteger("WaitingForStory", 0);

                clappingTime = Random.Range(1, 3);

                isClapping = true;

                child.animator.SetBool("Clapping", true);
            }
            else if (isClapping)
            {
                clappingTime -= Time.deltaTime;

                if (clappingTime <= 0)
                {
                    //might want to do something to make them wander away or something???
                    child.animator.SetBool("Clapping", false);
                    OnStateExit(CreateInstance<IdleState>());
                }
            }
        }

        internal override void OnStateExit(StateMachine<MaskedChildController> nextState)
        {
            nextState.OnStateEnter(child);
            Destroy(this);
        }
    }
}
