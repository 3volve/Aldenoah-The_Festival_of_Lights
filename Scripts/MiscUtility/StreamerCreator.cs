using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StreamerCreator : MonoBehaviour
{
    [SerializeField] int numOfLights = 8;
    [SerializeField] float streamerDip = 1;

    [SerializeField] Transform ropePart;
    [SerializeField] GameObject lightPrefab;
    [SerializeField] Material[] materials;
    [SerializeField] Transform[] endPoints = new Transform[2];
    [SerializeField] bool createStreamer = false;

#pragma warning disable
#if UNITY_EDITOR
    void OnValidate()
    {
        if (createStreamer)
        {
            float totalDistance = Vector3.Distance(endPoints[0].position, endPoints[1].position);
            float totalPartitions = Mathf.Floor(totalDistance / ropePart.transform.localScale.y) / 1.5f;
            Vector3 position;
            Quaternion rotation;
            endPoints[0].LookAt((endPoints[1].position - endPoints[0].position) / 2 - Vector3.up);
            endPoints[0].rotation = Quaternion.LookRotation(endPoints[0].up, endPoints[0].forward);
            Transform curObj = endPoints[0];

            Instantiate(transform.GetChild(0), transform).name = "Rope";
            int materialCounter = 1;

            for (int i = 0; i < totalPartitions; i++)
            {
                position = Vector3.Lerp(endPoints[0].position, endPoints[1].position, i / totalPartitions);
                position.y -= streamerDip * Mathf.Sin((Mathf.PI * Vector3.Distance(endPoints[0].position, position))/totalDistance);
                
                Vector3 directionOfPrev = curObj.position - position;
                rotation = Quaternion.LookRotation(Vector3.Project(Vector3.up, directionOfPrev) - Vector3.up, directionOfPrev);

                curObj = Instantiate(ropePart, position, rotation, transform.GetChild(2));
                
                if (materialCounter <= numOfLights && i == (int)((totalPartitions / (numOfLights + 1)) * materialCounter))
                {
                    int index = Random.Range(0, 4);
                    position.y -= 0.1f;

                    Instantiate(lightPrefab, position, rotation, transform.GetChild(2))
                        .GetComponent<MeshRenderer>().material = materials[index];

                    materialCounter++;
                }
            }

            createStreamer = false;
        }
    }

#endif
}
