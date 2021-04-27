using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class RelocatePriest : MonoBehaviour
{  //This should be named something else now that It's also handling sending kids back, but I don't have the time to fix right now.
    public Transform[] boundaryDirections = new Transform[4];
    
    private void OnTriggerEnter(Collider other)
    {
        PriestController priest = other.GetComponent<PriestController>();

        if (other.tag == "Follower" && !priest.GoingToStoredDestination)
        {
            priest.GoingToStoredDestination = true;

            float curDirection = Mathf.Abs(other.transform.position.x) - Mathf.Abs(other.transform.position.z);
            int[] possibleDirections = { };

            if (curDirection > 0 && other.transform.position.x < 0)
            {
                curDirection = 0; //currently west
                possibleDirections = new int[] { 1, 2, 3 };
            }
            else if (curDirection > 0 && other.transform.position.x > 0)
            {
                curDirection = 2; //currently east
                possibleDirections = new int[] { 0, 1, 3 };
            }
            else if (curDirection < 0 && other.transform.position.z < 0)
            {
                curDirection = 3; //currently south
                possibleDirections = new int[] { 0, 1, 2 };
            }
            else if (curDirection < 0 && other.transform.position.z > 0)
            {
                curDirection = 1; //currently north
                possibleDirections = new int[] { 0, 2, 3 };
            }

            int newDirection = Random.Range(0, 3);

            Transform newPosition = boundaryDirections[possibleDirections[newDirection]];

            newPosition = newPosition.GetChild(Random.Range(0, newPosition.childCount));

            StartCoroutine(WaitUntilReady(other.transform, newPosition, Vector3.Distance(newPosition.position, other.transform.position) / 8));
        }
        else if (other.tag == "MaskedChild")
        {
            other.transform.rotation.SetFromToRotation(other.transform.forward, transform.GetChild(0).position - other.transform.position);
            other.GetComponent<MaskedChildController>().TriggerReturningHome();
        }
    }

    IEnumerator WaitUntilReady(Transform other, Transform newPosition, float timeToWait)
    {
        yield return new WaitForSeconds(timeToWait);

        PriestController priest = other.GetComponent<PriestController>();

        priest.TriggerCornerEscape(newPosition.GetChild(0), true);
        other.position = newPosition.position;
        other.rotation *= Quaternion.LookRotation(newPosition.GetChild(0).position - newPosition.position, Vector3.up);

        yield break;
    }
}
