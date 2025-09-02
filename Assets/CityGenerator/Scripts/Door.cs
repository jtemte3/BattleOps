using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class Door : MonoBehaviour
{
    public CityBlock block;
    public Vector2Int pathingVector;
    public bool hasPath = false;
    public List<Vector2Int> roadPath;

    public void Awake()
    {
        SetPathingState(false);
    }

    public Vector2Int GetPathingVector()
    {
        return pathingVector;
    }  

    public void SetPathingState(bool state)
    {
        hasPath = state;

        if (hasPath)
        {
            GetComponent<MeshRenderer>().enabled = false;
            GetComponent<Collider>().enabled = false;
        }
        else
        {
            GetComponent<MeshRenderer>().enabled = true;
            GetComponent<Collider>().enabled = true;
        }
    }

    public void SetHallwayPath(List<Vector2Int> path)
    {
        roadPath = path;
    }

#if (UNITY_EDITOR)
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;

        Vector3 gridPosition = new Vector3();
        gridPosition.x = (block.transform.position.x * block.GetGridScale()) + block.GetPosition().x;
        gridPosition.y = 0;
        gridPosition.z = (block.transform.position.z * block.GetGridScale()) + block.GetPosition().y;

        Vector3 vector = new Vector3(transform.position.x + (GetPathingVector().x * block.GetGridScale()), 1.5f, transform.position.z + (GetPathingVector().y * block.GetGridScale()));
        //Vector3 vector = new Vector3((Position.x * 5) + (door.GetDoorVector().x * 5), 1.5f, (Position.y * 5) + (door.GetDoorVector().y * 5));
        Gizmos.DrawWireCube(vector, new Vector3(block.GetGridScale(), 3, block.GetGridScale()));

        if (hasPath)
        {
            if(roadPath != null || roadPath.Count == 0)
            {
                for(int i = 0; i < roadPath.Count; i++)
                {
                    if (i != roadPath.Count - 1)
                    {
                        Vector3 from = new Vector3((roadPath[i].x * block.GetGridScale()) + gridPosition.x, 1.5f, (roadPath[i].y * block.GetGridScale()) + gridPosition.z);
                        Vector3 to = new Vector3((roadPath[i + 1].x * block.GetGridScale()) + gridPosition.x, 1.5f, (roadPath[i + 1].y * block.GetGridScale()) + gridPosition.z);
                        
                        Gizmos.color = Color.yellow;
                        Gizmos.DrawLine(from, to);

                        Gizmos.color = Color.gray;
                        Gizmos.DrawSphere(from, 0.25f);
                    }
                }
            }
            else
            {
                Gizmos.color = Color.red;
                Gizmos.DrawSphere(transform.position, .5f);
            }
        }
    }
#endif
}
