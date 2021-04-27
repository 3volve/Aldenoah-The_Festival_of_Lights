using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IdleKeyMovement : MonoBehaviour
{
    public float movementSpeed = 0.01f;
    public float rotationSpeedCos = 0.3f;
    public float rotationSpeedSin = 0.5f;
    public Material primary;
    public Material focused;

    private int randomSeededOffset;

    private void Start()
    {
        for (int i = 0; i < 8 && transform.rotation.eulerAngles.y % 360 != 180; i++)
            transform.Rotate(0, 90, 0);

        randomSeededOffset = Random.Range(0, 360);
    }
    void Update()
    {
        float curCos = Mathf.Cos(Time.time * rotationSpeedCos + randomSeededOffset);
        float curSin = Mathf.Sin(Time.time * rotationSpeedSin + randomSeededOffset);

        transform.position += Vector3.up * curSin * movementSpeed;

        Vector3 newUpRotation = new Vector3(curCos, 2, curSin);
        Vector3 newForwardRotation = new Vector3(curSin, curCos, -2);

        transform.rotation = Quaternion.LookRotation(newForwardRotation, newUpRotation);
    }
}
