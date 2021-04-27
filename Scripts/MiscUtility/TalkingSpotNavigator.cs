using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TalkingSpotNavigator : MonoBehaviour
{
    static readonly int TOTAL_SPOTS = 5;
    static readonly int TOTAL_SITTING_SPOTS = 6;
    public Transform[] talkingSpots = new Transform[TOTAL_SPOTS];

    public Transform[,] sittingSpots = new Transform[TOTAL_SPOTS, TOTAL_SITTING_SPOTS + 1];
    readonly string[] spotTaken = new string[TOTAL_SPOTS];
    readonly bool[,] spotsTaken = new bool[TOTAL_SPOTS, TOTAL_SITTING_SPOTS + 1];
    
    void Start()
    {
        if (string.IsNullOrEmpty(spotTaken[0]))
        {
            for (int i = 0; i < TOTAL_SPOTS; i++)
                spotTaken[i] = "Available";
        }

        for (int i = 0; i < TOTAL_SPOTS; i++)
        {
            if (talkingSpots[i] != null && talkingSpots[i].childCount >= TOTAL_SITTING_SPOTS + 2)
            {
                sittingSpots[i, 0] = talkingSpots[i];

                for (int j = 0; j < TOTAL_SITTING_SPOTS; j++)
                    sittingSpots[i, j + 1] = talkingSpots[i].GetChild(j + 2);
            }
        }
    }


    /************************************ RecieveMessageToSit Methods ****************************/
    public Transform RecieveFollowerMessage(Vector3 followerPosition, string followerName)
    {
        Transform destination = null;
        float minDistance = float.MaxValue;
        float curDistance = 0;
        int finalSpotSelected = -1;

        for (int i = 0; i < talkingSpots.Length; i++)
        {
            if (spotTaken[i].Equals("Available") && talkingSpots[i] != null) {
                curDistance = Vector3.Distance(followerPosition, talkingSpots[i].position);

                if (curDistance < minDistance)
                {
                    minDistance = curDistance;
                    destination = talkingSpots[i];
                    finalSpotSelected = i;
                }
            }
        }

        if (finalSpotSelected == -1) return null;

        spotTaken[finalSpotSelected] = followerName;
        return destination.GetChild(0);
    }

    public Transform RecieveMaskedChildMessage(string targetPriest)
    {
        int priestIndex, seatIndex = 1;

        for (priestIndex = 0; priestIndex < TOTAL_SPOTS; priestIndex++)
            if (spotTaken[priestIndex].Equals(targetPriest)) break;

        if (priestIndex < TOTAL_SPOTS)
            for (seatIndex = 1; seatIndex < TOTAL_SITTING_SPOTS; seatIndex++)
                if (!spotsTaken[priestIndex, seatIndex]) break;

        if (priestIndex == TOTAL_SPOTS || seatIndex == TOTAL_SITTING_SPOTS)
            return null;

        spotsTaken[priestIndex, seatIndex] = true;
        return sittingSpots[priestIndex, seatIndex];
    }
}
