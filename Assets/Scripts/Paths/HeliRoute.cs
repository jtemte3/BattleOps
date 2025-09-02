using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class HeliRoute : MonoBehaviour
{
    public List<GameObject> infilApproachRoute = new List<GameObject>();
    public List<GameObject> infilExitRoute = new List<GameObject>();
    public List<GameObject> extractApproachRoute = new List<GameObject>();
    public List<GameObject> extractExitRoute = new List<GameObject>();
    public GameObject beginDecentNode;
    public GameObject exitNode;

#if (UNITY_EDITOR)
    void OnDrawGizmos()
    {
        if (infilApproachRoute.Count > 1)
        {
            Gizmos.color = Color.cyan;
            for (int i = 0; i < infilApproachRoute.Count; i++)
            {
                if (i != infilApproachRoute.Count - 1)
                {
                    int nextIndex = i + 1;
                    Gizmos.DrawLine(infilApproachRoute[i].transform.position, infilApproachRoute[nextIndex].transform.position);
                    Gizmos.DrawSphere(infilApproachRoute[i].transform.position, 0.1f);

                }
                Gizmos.DrawLine(infilApproachRoute[i].transform.position, infilApproachRoute[i].transform.position + (infilApproachRoute[i].transform.forward * 2));
            }
        }

        if (infilExitRoute.Count > 1)
        {
            Gizmos.color = Color.red;
            for (int i = 0; i < infilExitRoute.Count; i++)
            {
                if (i != infilExitRoute.Count - 1)
                {
                    int nextIndex = i + 1;
                    Gizmos.DrawLine(infilExitRoute[i].transform.position, infilExitRoute[nextIndex].transform.position);
                    Gizmos.DrawSphere(infilExitRoute[i].transform.position, 0.1f);

                }
                Gizmos.color = Color.cyan;
                Gizmos.DrawLine(infilExitRoute[i].transform.position, infilExitRoute[i].transform.position + (infilExitRoute[i].transform.forward * 2));
            }
        }
    }
#endif
}


