using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TalkingObject : MonoBehaviour
{
    #pragma warning disable CS0649 // #warning directive
    [SerializeField] GameObject talkToKey;
    [SerializeField] float waveHelloTime = 1f;
    [SerializeField] DialogueBoxController dBox;
    #pragma warning restore CS0649 // #warning directive

    public string characterName;
    public int profileIndex;
    [TextArea(1, 10)]
    public string[] speakingText;
    public bool isTalking = false;

    public Animator TalkingAnimator { private get; set; }

    ParticleSystem particles;

    void Start()
    {
        talkToKey = Instantiate(talkToKey, transform);
        talkToKey.SetActive(false);
        particles = talkToKey.GetComponent<ParticleSystem>();

        if (TalkingAnimator == null) TalkingAnimator = GetComponent<Animator>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player" && tag.StartsWith("Talking"))
            talkToKey.SetActive(true);
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "Player")
            talkToKey.SetActive(false);
    }

    public void SetTalkToKeyActive(bool active) => talkToKey.SetActive(active);

    public void SetFocused(bool isFocused)
    {
        Transform box = talkToKey.transform.GetChild(0);

        for (int i = 0; i < box.childCount; i++)
            box.GetChild(i).GetComponent<MeshRenderer>().material =
                isFocused ?
                talkToKey.GetComponent<IdleKeyMovement>().focused :
                talkToKey.GetComponent<IdleKeyMovement>().primary;

        if (isFocused && gameObject.activeInHierarchy) particles.Play();

        if (waveHelloTime > 0)
        {
            TalkingAnimator?.SetBool("Hi", isFocused);
            Invoke("StopWaving", waveHelloTime);
        }
    }

    void StopWaving() => TalkingAnimator.SetBool("Hi", false);

    public void OpenDialogueBox()
    {
        if (!isTalking && !dBox.IsTalking)
        {
            isTalking = true;
            TalkingAnimator?.SetBool("Talking", true);
            dBox.OpenDialogueBox(characterName, profileIndex, speakingText);
        }
    }

    public void CloseDialogueBox()
    {
        if (isTalking && !dBox.IsTalking)
        {
            TalkingAnimator?.SetBool("Talking", false);
            dBox.CloseDialogueBox();
            isTalking = false;
        }
    }

    private void OnDestroy()
    {
        talkToKey.SetActive(false);
    }
}
