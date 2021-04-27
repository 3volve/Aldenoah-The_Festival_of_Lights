using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShowPathFromEditor : MonoBehaviour
{
    public bool oneWay = false;
    public bool visible = true;

    public int[] visibleNodes = new int[2];

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (visible)
        {
            if (visibleNodes[1] <= visibleNodes[0]) visibleNodes[1] = transform.childCount;

            for (int i = visibleNodes[0]; i < visibleNodes[1]; i++)
            {
                Transform currentChild = transform.GetChild(i);

                Gizmos.color = Color.cyan;
                if (i < transform.childCount - 1)
                    Gizmos.DrawLine(currentChild.position, transform.GetChild(i + 1).position);
                else if (!oneWay)
                    Gizmos.DrawLine(currentChild.position, transform.GetChild(0).position);

                Gizmos.color = Color.blue;
                Gizmos.DrawSphere(currentChild.position + currentChild.forward * 0.5f, 0.25f);// new Vector3(0.5f, 0.1f, 0.5f));
                Gizmos.DrawSphere(currentChild.position, 0.5f);
            }
        }
    }
#endif
}
