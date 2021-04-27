using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArguingTownsFolk : MonoBehaviour
{
    public static readonly float minTimeForAction = 1f;
    public static readonly float maxTimeSpentAngry = 4;
    public static readonly float maxTimeShakingHead = 1.5f;
    public static readonly float maxTimeArguing = 10;

    bool initialized = false;

    Animator animator;
    Coroutine currentRoutine;

    void Awake()
    {
        animator = GetComponent<Animator>();

        initialized = true;
    }

    public void StartArguing() => currentRoutine = currentRoutine ?? StartCoroutine(ActingAngry());

    public IEnumerator ActingAngry()
    {
        yield return new WaitUntil(() => initialized);

        while (true)
        {
            animator.SetBool("Angry", true);
            yield return new WaitForSeconds(Random.Range(minTimeForAction, maxTimeSpentAngry));
            animator.SetBool("Angry", false);

            if (Random.Range(0, 1) < 0.3f)
            {
                animator.SetBool("Shake Head", true);
                yield return new WaitForSeconds(Random.Range(minTimeForAction, maxTimeShakingHead));
                animator.SetBool("Shake Head", false);
            }

            animator.SetBool("Talking", true);
            yield return new WaitForSeconds(Random.Range(minTimeForAction, maxTimeArguing));
            animator.SetBool("Talking", false);

            if (Random.Range(0, 1) < 0.3f)
            {
                animator.SetBool("Shake Head", true);
                yield return new WaitForSeconds(Random.Range(minTimeForAction, maxTimeShakingHead));
                animator.SetBool("Shake Head", false);
            }
        }
    }
}
