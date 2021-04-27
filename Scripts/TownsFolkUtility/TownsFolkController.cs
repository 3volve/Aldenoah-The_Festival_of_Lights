using System;
using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

public class TownsFolkController : MonoBehaviour
{
    public bool isTalkable = false;
    public bool isAngry = false;
    public bool isTalking = false;
    public bool isWalking = false;
    public bool isSitting = false;

    Animator animator;

    void Start()
    {
        animator = GetComponent<Animator>();

        if (isAngry) GetComponent<ArguingTownsFolk>().StartArguing();

        animator.SetBool("Sit", isSitting);

        if (isSitting && isTalking) Invoke("StartTalking", Random.Range(0.5f, 15f));
        else animator.SetBool("Talking", isTalking);
    }

    void StartTalking() => animator.SetBool("Talking", isTalking);
}
