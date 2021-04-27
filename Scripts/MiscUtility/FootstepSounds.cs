using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FootstepSounds : MonoBehaviour
{
    public AudioClip[] footsteps = new AudioClip[10];
    public Transform[] feet = new Transform[2];

    public bool fullySetUp = false;
    public float reqDeltaZForStep;

    FootData[] feetData = new FootData[2];
    AudioSource audioSource;
    int previousSound = -1;

    struct FootData
    {
        internal Transform foot;
        internal float basePosition;
        internal float totalDeltaZ;
        internal bool isPrimed;
    }

    void Start()
    {
        for (int i = 0; i < feet.Length; i++)
        {
            feetData[i] = new FootData
            {
                foot = feet[i],
                basePosition = transform.InverseTransformPoint(feet[i].position).z,
                totalDeltaZ = 0,
                isPrimed = false
            };
        }

        audioSource = GetComponent<AudioSource>();
    }

    void Update()
    {
        if (fullySetUp) //TODO: Make sure you don't leave this in, this is why footsteps aren't happening...
        {
            bool makeSound = false;

            for (int i = 0; i < feetData.Length; i++)
            {
                float newDeltaZ = transform.InverseTransformPoint(feetData[i].foot.position).z - feetData[i].basePosition;
                float deltaZ = newDeltaZ - feetData[i].totalDeltaZ;
                feetData[i].totalDeltaZ = newDeltaZ;

                if (Mathf.Abs(deltaZ) < 0.0005f) deltaZ = 0;

                if (!feetData[i].isPrimed) feetData[i].isPrimed = feetData[i].totalDeltaZ >= reqDeltaZForStep;
                else if (deltaZ < 0)
                {
                    feetData[i].isPrimed = false;
                    makeSound = true;
                }
            }

            if (makeSound)
            {
                int newSound = Random.Range(0, footsteps.Length);
                while (newSound == previousSound) newSound = Random.Range(0, footsteps.Length);
                previousSound = newSound;

                audioSource.PlayOneShot(footsteps[newSound]);
            }
        }
    }
}
