using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.ScriptableObject;

public class PriestController : MovingObject
{
    #pragma warning disable CS0649 // #warning directive

    public float maxSpeed;
    public float walkSpeed;
    [SerializeField] float acceleration;
    [SerializeField] float visionArc = 90;
    [SerializeField] float visionRange = 15;
    [SerializeField] TalkingSpotNavigator talkingNavigator;
    [SerializeField] Transform head;
    [SerializeField] Transform ghostChildPrefab;
    [SerializeField] FollowerLightController followingLight;

    static readonly int MAX_CHECK_FOR_MOVEMENT = 20;
    static readonly int START_MEMORY_DURATION = 400;
    static readonly float MIN_DISTANCE_STUCK = 0.1f;

    static int framesToWaitAfterLosingChasers = 150;

    int distanceBetweenPathingPoints = 2;


    public bool CheckingBehind { get; set; } = false;
    public bool FinishedSitting { private get; set; } = false;
    public bool GoingToStoredDestination { get; set; } = false;
    Transform storedDestination;

    bool running = false;
    bool active = true;
    bool isCaught = false;
    bool areInFrontOfMe = false;
    bool areBehindMe = false;

    public List<GameObject> potentialStuckObjects = new List<GameObject>();
    public int stuckCounter = 15;

    StateMachine<PriestController> state;
    PathingObject pathing;
    Animator animator;

    Vector3 nextPosition;
    Vector3 rawDirection = Vector3.zero;
    Vector3 previousPosition;

    MyCountingSet<Transform> runAwayFrom = new MyCountingSet<Transform>(START_MEMORY_DURATION);
    HashSet<Transform> inRange = new HashSet<Transform>();


    void Start()
    {
        state = CreateInstance<IdleState>();
        ((IdleState)state).slowDownCounter = 0;
        state.OnStateEnter(this);

        GetComponentInChildren<SphereCollider>().radius = visionRange;

        pathing = GetComponent<PathingObject>();
        nextPosition = transform.position;

        if (framesToWaitAfterLosingChasers == 0)
            framesToWaitAfterLosingChasers = Mathf.RoundToInt(1 / Time.fixedDeltaTime) * 3;

        Init(walkSpeed, maxSpeed, acceleration);
        animator = GetComponentsInChildren<Animator>()[0];
    }
    
    void Update()
    {
        state ? .UpdateState();

        if (runAwayFrom.Count > 0)
            runAwayFrom.CountDown();

        if (active) {
            CalculateSight();
            InternalUpdate();

            if (GoingToStoredDestination && storedDestination != null)
            {
                Move(storedDestination.position - transform.position, running);
                
                if (Vector3.Distance(storedDestination.position, transform.position) < 1.5f)
                {
                    GoingToStoredDestination = false;
                    storedDestination = null;
                    nextPosition = transform.position;
                    rawDirection = transform.forward * 6;
                }
            }
        }
    }

    void CalculateSight()
    {
        Vector3 headDirection = head.up;
        Vector3 childDirection;
        headDirection.y = 0;

        runAwayFrom.CullNulls();
        foreach (Transform child in inRange)
        {
            if (child == null) continue;

            childDirection = child.position - transform.position;

            if (Vector3.Angle(childDirection, headDirection) <= visionArc / 2)
            {
                if (!Physics.Linecast(transform.position, child.position, LayerMask.GetMask("BuildingsBlocking")))
                    runAwayFrom.Add(child);
            }
        }

        areBehindMe = false;
        areInFrontOfMe = false;

        if (runAwayFrom.Count != 0)
        {
            CalculateRunningDirection();

            foreach (Transform child in runAwayFrom)
            {
                if (child == null) continue;

                if (Vector3.Angle(child.position - transform.position, transform.forward) >= 90)
                    areBehindMe = true;
                else areInFrontOfMe = true;
            }
        }
        
        followingLight.ActiveLight(runAwayFrom.Count > 0);

        if (!animator.GetBool("LookingBack") && !CheckingBehind)
        {
            if (areBehindMe && !areInFrontOfMe) Invoke("CheckBehind", 40);
            else if (areBehindMe && areInFrontOfMe) Invoke("CheckBehind", 16);
            else if (!areBehindMe && !areInFrontOfMe) Invoke("CheckBehind", 1.5f);

            CheckingBehind = true;
        }
        else if (animator.GetBool("LookingBack")) CheckingBehind = false;
    }

    void CheckBehind() => animator.SetBool("LookingBack", true);

    void CalculateRunningDirection()
    {
        Vector3 result = Vector3.zero;
        Vector3 childPos;
        float minimumInfluence = 0.25f;
        float iDM; //inverseDistanceMultiplier

        runAwayFrom.CullNulls();
        foreach (Transform child in runAwayFrom)
        {
            if (child == null) continue;

            childPos = child.position;
            iDM = Mathf.Min(-minimumInfluence, -(visionRange + 5 - Vector3.Distance(childPos, transform.position)));
            result += (childPos - transform.position) * iDM;
        }

        result.y = 0;
        result = result.normalized;


        if (rawDirection == Vector3.zero || Vector3.Angle(result, rawDirection) > 5) rawDirection = result;
    }

    void MoveToPointInDirection()
    {
        if (Vector3.Distance(transform.position, nextPosition) <= Mathf.Max(0.71f, distanceBetweenPathingPoints))
        {
            nextPosition = pathing.GetNextPathPosition(transform.position + rawDirection * 6, distanceBetweenPathingPoints);

            for (int i = 6; i < MAX_CHECK_FOR_MOVEMENT && nextPosition == null; i++)
                nextPosition = pathing.GetNextPathPosition(transform.position + rawDirection * i, distanceBetweenPathingPoints);
        }

        Vector3 newDirection = Vector3.zero;

        if (stuckCounter > 0) newDirection = nextPosition - transform.position;
        
        if (active)
        {
            if (Vector3.Distance(previousPosition, transform.position) <= MIN_DISTANCE_STUCK)
            {
                if (--stuckCounter <= 0)
                {
                    foreach (GameObject obj in potentialStuckObjects)
                        newDirection += (transform.position - obj.transform.position).normalized;

                    if (stuckCounter < -15 && transform.forward != newDirection && nextPosition != transform.position)
                        transform.rotation = Quaternion.RotateTowards(
                            transform.rotation,
                            Quaternion.LookRotation((nextPosition - transform.position).normalized),
                            30
                            );

                    distanceBetweenPathingPoints = 0;
                }
            }

            if (stuckCounter <= 0 && potentialStuckObjects.Count == 0)
            {
                distanceBetweenPathingPoints = 2;
                stuckCounter = 15;
                nextPosition = transform.position;
            }

            newDirection.y = 0;

            if (stuckCounter > -15 || Vector3.Angle(transform.forward, nextPosition - transform.position) < 5)
                Move(newDirection, running);
            previousPosition = transform.position;
        }
    }

    public void TriggerCornerEscape(Transform target, bool clearRunningList)
    {
        if ((GoingToStoredDestination && storedDestination != null) || state.ToString() == "Caught") return;

        storedDestination = target;

        if (clearRunningList)
        {
            runAwayFrom.Clear(OnClearMethod);
            
            runAwayFrom.Add(
                Instantiate(ghostChildPrefab, target.parent.position, target.parent.rotation)
                .GetComponent<GhostChild>().Init(transform)
                );

            runAwayFrom.Add(
                Instantiate(ghostChildPrefab, target.parent.position, target.parent.rotation)
                .GetComponent<GhostChild>().Init(transform)
                );

            runAwayFrom.Add(
                Instantiate(ghostChildPrefab, target.parent.position, target.parent.rotation)
                .GetComponent<GhostChild>().Init(transform)
                );
        }
    }

    internal bool OnClearMethod(List<Transform> runAwayList)
    {
        bool result = false;

        for (int i = 0; i < runAwayList.Count; i++)
        {
            Transform child = runAwayList[i];

            if (child.tag == "GhostChild")
            {
                Destroy(child.gameObject, 0.1f);
                result = true;
            }
        }
        
        return result;
    }

    static readonly float DISTANCE_TO_ARRIVE = 0.15f;

    internal void OnCaughtTravelToTalkingSpot(Transform talkingSpot)
    {
        if (talkingSpot == null) return;
        if (Vector3.Distance(transform.position, talkingSpot.position) < 0.71f) // 0.71 is the longest distance from a node point possible
            nextPosition = talkingSpot.position;
        else if (Vector3.Distance(transform.position, nextPosition) < DISTANCE_TO_ARRIVE)
            nextPosition = pathing.GetNextPathPosition(talkingSpot);

        Vector3 direction = nextPosition - transform.position;

        if (Vector3.Distance(previousPosition, transform.position) <= MIN_DISTANCE_STUCK)
        {
            if (--stuckCounter <= 0)
            {
                foreach (GameObject obj in potentialStuckObjects)
                    direction += (transform.position - obj.transform.position).normalized;

                if (stuckCounter < -15 && transform.forward != direction && nextPosition != transform.position)
                    transform.rotation = Quaternion.RotateTowards(
                        transform.rotation,
                        Quaternion.LookRotation((nextPosition - transform.position).normalized),
                        30
                        );
            }
        }

        if (stuckCounter <= 0 && potentialStuckObjects.Count == 0)
        {
            stuckCounter = 15;
            nextPosition = transform.position;
        }

        direction.y = 0;
        if (stuckCounter > -15 || Vector3.Angle(transform.forward, nextPosition - transform.position) < 5)
            Move(direction, running);

        previousPosition = transform.position;
    }

    internal bool CheckIfArrived(Transform talkingSpot)
    {
        if (talkingSpot == null) return true;

        float distanceFromDestination = Vector3.Distance(transform.position, talkingSpot.position);
        if (distanceFromDestination < 1f) talkingSpot.parent.GetChild(1).GetComponent<Collider>().enabled = false;
        if (distanceFromDestination < DISTANCE_TO_ARRIVE) return true;

        return false;
    }

    internal void DisableAbilityToTellStory()
    {
        tag = "Building";
        Destroy(this);
        Destroy(transform.parent.GetComponent<TalkingObject>());
        transform.parent.tag = "Building";
        transform.parent.GetComponent<Collider>().enabled = false;
        transform.parent.GetComponent<Collider>().enabled = true;
        followingLight.ActiveLight(false);
        //I can destroy the priest and disable talkable on everything involved.
    }


    /************************************ OnContact Unity Methods ****************************/
    void OnCollisionEnter(Collision collision)
    {
        string tag = collision.collider.tag;

        if (tag == "MaskedChild" || tag == "Player") isCaught = true;
        else if (tag != "Ground") potentialStuckObjects.Add(collision.collider.gameObject);
    }

    void OnCollisionExit(Collision collision)
    {
        if (tag != "MaskedChild" && tag != "Player")
        {
            GameObject other = collision.collider.gameObject;

            if (potentialStuckObjects.Contains(other))
                potentialStuckObjects.Remove(other);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.tag == "MaskedChild" || other.tag == "Player")
            inRange.Add(other.transform);
    }

    void OnTriggerExit(Collider other)
    {
        if (other.tag == "MaskedChild" || other.tag == "Player")
            inRange.Remove(other.transform);
    }


    /************************************ State Machine ****************************************/
    internal class IdleState : StateMachine<PriestController>
    {
        PriestController priest;
        internal int slowDownCounter = 60;

        internal override void OnStateEnter(PriestController controller)
        {
            priest = controller;
            priest.state = this;
            priest.running = false;
            priest.followingLight.ActiveLight(false);
        }

        internal override void UpdateState()
        {
            if (priest.isCaught) OnStateExit(CreateInstance<CaughtState>());
            else if (priest.runAwayFrom.Count > 0) OnStateExit(CreateInstance<RunningState>());
            else if (slowDownCounter-- > 0) priest.MoveToPointInDirection();
            else priest.runAwayFrom.Clear(priest.OnClearMethod);

            //I need to add some idle looking around them animation, maybe walking around aimlessly in some way?
            // Not enough time, maybe I'll look into adding this later on.
        }

        internal override void OnStateExit(StateMachine<PriestController> nextState)
        {
            nextState.OnStateEnter(priest);
            Destroy(this);
        }

        public override string ToString() => "Idle State";
    }

    internal class RunningState : StateMachine<PriestController>
    {
        PriestController priest;

        int lostChasersCounter = framesToWaitAfterLosingChasers;

        internal override void OnStateEnter(PriestController controller)
        {
            priest = controller;
            priest.state = this;
            priest.running = true;
            priest.followingLight.ActiveLight(true);
        }

        internal override void UpdateState()
        {
            if (priest.isCaught) OnStateExit(CreateInstance<CaughtState>());
            else if (priest.GoingToStoredDestination && priest.storedDestination != null) return;
            else if (priest.runAwayFrom.Count == 0) lostChasersCounter--;
            else lostChasersCounter = framesToWaitAfterLosingChasers;
            
            if (lostChasersCounter > 0) priest.MoveToPointInDirection();
            else if (priest.runAwayFrom.Count == 0) OnStateExit(CreateInstance<IdleState>());
        }

        internal override void OnStateExit(StateMachine<PriestController> nextState)
        {
            nextState.OnStateEnter(priest);
            Destroy(this);
        }

        public override string ToString() => "Running State";
    }

    internal class CaughtState : StateMachine<PriestController>
    {
        PriestController priest;
        Transform talkingSpot;
        FollowerStoryData followerData;

        bool walkingToSpot = true;
        bool setupSpotToTalk = false;

        internal override void OnStateEnter(PriestController controller)
        {
            priest = controller;
            priest.state = this;
            priest.running = false;
            priest.active = false;
            talkingSpot = priest.talkingNavigator.RecieveFollowerMessage(priest.transform.position, priest.name);
            followerData = priest.GetComponent<FollowerStoryData>();
            followerData.AdjustSittingDestination(talkingSpot);
            talkingSpot.parent.gameObject.layer = LayerMask.GetMask("Ignore Raycast");
            priest.tag = "WalkingFollower";

            priest.followingLight.BurstLight();
            priest.followingLight.ActiveLight(false);
        }

        internal override void UpdateState()
        {
            if (!setupSpotToTalk)
            {
                bool isSitting = priest.animator.GetBool("Sitting");

                if (walkingToSpot)
                {
                    priest.OnCaughtTravelToTalkingSpot(talkingSpot);
                    walkingToSpot = !priest.CheckIfArrived(talkingSpot);
                    priest.InternalUpdate();

                    if (!walkingToSpot)
                    {
                        Destroy(priest.GetComponent<Rigidbody>());
                        priest.GetComponent<Collider>().isTrigger = true;
                        talkingSpot.parent.GetChild(1).GetComponent<Collider>().enabled = true;
                    }
                }
                else if (!isSitting && Vector3.Angle(priest.transform.forward, talkingSpot.forward) < 170)
                {
                    priest.transform.rotation = Quaternion.RotateTowards(
                        priest.transform.rotation,
                        Quaternion.LookRotation(-talkingSpot.forward, Vector3.up),
                        10
                    );
                }
                else if (!isSitting)
                {
                    //start animator movement to sit down.
                    priest.animator.applyRootMotion = true;
                    priest.animator.SetBool("Sitting", true);
                }
                else if (priest.FinishedSitting)
                {
                    priest.tag = "StoryFollower";
                    followerData.SetupTalkingSpot(talkingSpot.parent);
                    priest.followingLight.ReplaceTarget(talkingSpot.parent);
                    priest.followingLight.ActiveLight(true);
                    setupSpotToTalk = true;
                }
            }
            else if (followerData.hasToldStory)
            {
                priest.DisableAbilityToTellStory();
            }
        }

        internal override void OnStateExit(StateMachine<PriestController> nextState) => throw new System.NotImplementedException();

        public override string ToString() => "Caught State";
    }
}
