using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AvoidCornerTrigger : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Follower")
        {
            PriestController priest = other.GetComponent<PriestController>();

            Transform target = transform.GetChild(0);
            Transform otherTarget = transform.GetChild(1);
            float angleToTarget1 = Vector3.Angle(priest.transform.forward, target.position - priest.transform.position);
            float angleToTarget2 = Vector3.Angle(priest.transform.forward, otherTarget.position - priest.transform.position);

            priest.TriggerCornerEscape(angleToTarget1 < angleToTarget2 ? target : otherTarget, false);
        }
    }
}
