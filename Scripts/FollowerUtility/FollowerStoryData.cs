using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowerStoryData : MonoBehaviour
{
#pragma warning disable CS0649 // #warning directive

    [SerializeField] string characterName;
    [SerializeField] int profileIndex;
    [SerializeField] [TextArea(3, 10)] string[] speakingText;
    [SerializeField] Vector3 sittingDestination;

#pragma warning restore CS0649 // #warning directive

    public bool hasToldStory = false;
    
    bool startedTalking = false;
    TalkingObject talkingObject;

    void Update()
    {
        if (talkingObject != null)
        {
            if (!startedTalking && talkingObject.isTalking) startedTalking = true;
            else if (startedTalking && !talkingObject.isTalking) hasToldStory = true;
        }
    }

    public void SetupTalkingSpot(Transform spotTransform)
    {
        TalkingObject talkingSpot = spotTransform.GetComponent<TalkingObject>();

        talkingSpot.characterName = characterName;
        talkingSpot.profileIndex = profileIndex;
        talkingSpot.speakingText = speakingText;
        talkingSpot.TalkingAnimator = GetComponent<Animator>();
        talkingSpot.tag = "TalkingSpot";
        talkingSpot.GetComponent<Collider>().enabled = false;
        talkingSpot.GetComponent<Collider>().enabled = true;
        transform.parent = spotTransform;
        talkingObject = talkingSpot.GetComponent<TalkingObject>();
    }

    public Vector3 AdjustSittingDestination(Transform destination) => destination.localPosition = sittingDestination;
}
