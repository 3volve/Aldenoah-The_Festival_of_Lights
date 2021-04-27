using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FootTrafficManager : MonoBehaviour
{
    public PathingGrid grid;
    public float[] spawnTownsfolkDelayRange = new float[2];
    public int spawnLimit = 40;

    public Transform[] paths = new Transform[4];
    public GameObject[] prefabs = new GameObject[10];

    int entityCounter = 0;
    float[] timeToSpawnPerPath;

    void Start()
    {
        timeToSpawnPerPath = new float[paths.Length];

        for (int i = 0; i < timeToSpawnPerPath.Length; i++)
            timeToSpawnPerPath[i] = Time.fixedTime + Random.Range(0, spawnTownsfolkDelayRange[1]);

        foreach (GameObject prefab in prefabs)
            prefab.GetComponent<PathingObject>().grid = grid;
    }
    
    void FixedUpdate()
    {
        GameObject currentSpawn;
        float offSet;

        for (int i = 0; i < paths.Length; i++)
        {
            if (timeToSpawnPerPath[i] <= Time.fixedTime && entityCounter < spawnLimit)
            {
                currentSpawn = prefabs[Random.Range(0, prefabs.Length)];
                offSet = Random.Range(-1f, 1f);

                currentSpawn = Instantiate(
                    currentSpawn,
                    paths[i].GetChild(0).position + paths[i].GetChild(0).right * offSet,
                    paths[i].GetChild(0).rotation
                );

                currentSpawn.GetComponent<WalkingTownsFolk>().OneWayInit(paths[i], 1, offSet, this);

                if ((i == 0 || i == 2) && Mathf.Abs(timeToSpawnPerPath[0] - timeToSpawnPerPath[2]) < 10)
                {
                    if (i == 0) timeToSpawnPerPath[2] += 10;
                    else timeToSpawnPerPath[0] += 10;
                }

                timeToSpawnPerPath[i] = Time.fixedTime + Random.Range(spawnTownsfolkDelayRange[0], spawnTownsfolkDelayRange[1]);
                entityCounter++;
                //if (i == 0) Debug.Log("time: " + Time.fixedTime + ", nextSpawn: " + timeToSpawnPerPath[0]);
            }
        }
    }

    public void ReduceCounter() => entityCounter--;
}
