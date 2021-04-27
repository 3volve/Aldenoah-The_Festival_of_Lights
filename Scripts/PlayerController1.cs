using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController1 : MovingObject
{
    static readonly int maxControlsDistance = 10;
    static readonly int minControlsDistance = 5;

    #pragma warning disable CS0649 // #warning directive
    [SerializeField] float maxSpeed;
    [SerializeField] float walkSpeed;
    [SerializeField] float acceleration;
    [SerializeField] DialogueBoxController dBox;
    [SerializeField] SpriteRenderer wasdControls;
    [SerializeField] SpriteRenderer shiftControls;

    float wasdOpacity = 225f / 255f;
    float shiftOpacity = 225f / 255f;
    Color controlsColor;
    bool controlsGone = false;

    Vector3 direction = Vector3.zero;
    Dictionary<string, TalkingObject> inRangeTalkingObjects = new Dictionary<string, TalkingObject>();
    TalkingObject focusedTalkingObject = null;

    Transform turnAroundTarget = null;
    bool returning = false;
    bool running = false;
    bool isBusy = true;
    bool gameStarted = false;

    void Start()
    {
        Init(walkSpeed, maxSpeed, acceleration);
        controlsColor = wasdControls.color;
    }

    void FixedUpdate()
    {
        if (!gameStarted) return;

        if (returning && turnAroundTarget != null)
        {
            direction = turnAroundTarget.position - transform.position;
            if (Vector3.Distance(turnAroundTarget.position, transform.position) <= 0.3f)
            {
                returning = false;
                direction = Vector3.zero;
            }
        }
        else turnAroundTarget = null;

        if (!isBusy) Move(direction, running);
        InternalUpdate();

        bool existsNullTalkingObject = false;

        if (inRangeTalkingObjects.Count > 0)
        {
            foreach (TalkingObject talker in inRangeTalkingObjects.Values)
            {
                if (talker == null)
                {
                    existsNullTalkingObject = true;
                    continue;
                }

                Vector3 talkerDirection = talker.transform.position - transform.position;

                if (Vector3.Angle(talkerDirection, transform.forward) <= 10)
                {
                    if (focusedTalkingObject != talker)
                    {
                        focusedTalkingObject?.SetFocused(false);
                        talker.SetFocused(true);
                        focusedTalkingObject = talker;
                    }
                }
            }

            if (existsNullTalkingObject)
            {
                List<string> nullObjects = new List<string>();

                foreach (string talker in inRangeTalkingObjects.Keys)
                    if (inRangeTalkingObjects.TryGetValue(talker, out TalkingObject obj))
                        if (obj == null)
                            nullObjects.Add(talker);

                foreach (string nullObj in nullObjects)
                    inRangeTalkingObjects.Remove(nullObj);
            }
        }
        else if (focusedTalkingObject != null) focusedTalkingObject = null;


        if (!controlsGone)
        {
            int maxDeltaDistance = maxControlsDistance - minControlsDistance;
            float wasdDistance = Vector3.Distance(transform.position, wasdControls.transform.position);
            float shiftDistance = Vector3.Distance(transform.position, shiftControls.transform.position);

            if (wasdDistance < maxControlsDistance || shiftDistance < maxControlsDistance)
            {
                float curWASDOpacity = Mathf.Clamp01(1 - ((wasdDistance - minControlsDistance) / maxDeltaDistance));
                float curShiftOpacity = Mathf.Clamp01(1 - ((shiftDistance - minControlsDistance) / maxDeltaDistance));

                if (curWASDOpacity < wasdOpacity)
                {
                    controlsColor.a = wasdOpacity = curWASDOpacity;
                    wasdControls.color = controlsColor;
                }

                if (curShiftOpacity < shiftOpacity)
                {
                    controlsColor.a = shiftOpacity = curShiftOpacity;
                    shiftControls.color = controlsColor;
                }
            }
            else
            {
                controlsGone = true;
                Destroy(wasdControls.gameObject);
                Destroy(shiftControls.gameObject);
            }
        }
    }


    /************************************ Public Methods ************************************/
    public void GameStart() => gameStarted = true;


    public void CheckFocusedTalkingObject()
    {
        if (!focusedTalkingObject.tag.StartsWith("Talking"))
        {
            inRangeTalkingObjects.Remove(focusedTalkingObject.name);
            focusedTalkingObject.GetComponent<TalkingObject>().SetFocused(false);
            focusedTalkingObject = null;
        }
    }


    /************************************ Private Helper Methods ****************************/
    void TriggerReturn(Transform target)
    {
        returning = true;
        direction = target.position - transform.position;
        turnAroundTarget = target;
    }

    /************************************ OnContact Unity Methods ****************************/
    void OnTriggerEnter(Collider other)
    {
        if (other.tag.StartsWith("Talking"))
        {
            TalkingObject newTO = other.GetComponent<TalkingObject>();

            inRangeTalkingObjects.Add(other.name, newTO);

            if (inRangeTalkingObjects.Count == 1)
            {
                newTO.SetFocused(true);
                focusedTalkingObject = newTO;
            }
        }
        else if (other.tag == "AutoTalk")
        {
            isBusy = false;
            OnTalk(null);

            if (!other.name.StartsWith("Intro")) TriggerReturn(other.transform.GetChild(0));
        }
        
        if (other.tag == "MapBoundary") TriggerReturn(other.transform.GetChild(0));
    }

    void OnTriggerExit(Collider other)
    {
        if (other.tag.StartsWith("Talking"))
        {
            inRangeTalkingObjects.Remove(other.name);
            other.GetComponent<TalkingObject>().SetFocused(false);

            if (focusedTalkingObject.name == other.name)
                focusedTalkingObject = null;
        }
    }


    /******************************* Player Input Messaging Methods ********************/
    public void OnMove(InputValue value)
    {
        if (!returning)
        {
            Vector2 input = value.Get<Vector2>();
            direction.x = input.x;
            direction.z = input.y;
        }
    }

    public void OnTalk(InputValue value)
    {
        if (!gameStarted) return;

        if (focusedTalkingObject != null && !focusedTalkingObject.isTalking && !dBox.IsTalking && !isBusy)
        {
            isBusy = true;
            focusedTalkingObject.OpenDialogueBox();
        }
        else if (!dBox.WaitingToContinue && isBusy) //might want to transition the bool interaction to 
        {                                           // have the currentTalkingObject in between.
            dBox.WaitingToContinue = true;
        }
        else if (dBox.IsTalking && isBusy)
        {
            dBox.Autocomplete = true;
        }
        else if (!dBox.IsTalking && isBusy)
        {
            isBusy = false;
            focusedTalkingObject.CloseDialogueBox();
        }
    }

    public void OnRun(InputValue value) => running = !returning && value.Get<float>() != 0;
}
