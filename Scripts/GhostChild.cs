using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GhostChild : MonoBehaviour
{
    [SerializeField] float distanceToStopFromTarget = 2;
    [SerializeField] float followRate = 0.5f;
    [SerializeField] float survivalTime = 10;

    Transform grudgeTarget;
    float birthTime = float.MaxValue;

    public Transform Init(Transform target)
    {
        grudgeTarget = target;
        birthTime = Time.time;

        return transform;
    }

    void FixedUpdate()
    {
        if (grudgeTarget == null) return;

        if (Vector3.Distance(grudgeTarget.position, transform.position) > distanceToStopFromTarget)
            transform.position = Vector3.MoveTowards(transform.position, grudgeTarget.position, followRate);

        if (Time.time - birthTime > survivalTime) Destroy(gameObject);
    }
}
