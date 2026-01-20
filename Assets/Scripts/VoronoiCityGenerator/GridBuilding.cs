using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridBuilding : MonoBehaviour
{
    public string blockId;
    public Vector2Int gridPosition;
    public Vector2Int dimensions;
    public int gridScale;
    public float blockSpacingDistance = 7f; //Distance this block is from another block with the same ID

    public Vector2Int GetPosition()
    {
        return gridPosition;
    }

    public void SetPosition(Vector2Int pos)
    {
        gridPosition = pos;
    }

    public int GetGridScale()
    {
        return gridScale;
    }

    public Vector2Int GetRoomPosition()
    {
        return gridPosition;
    }

    public string GetId()
    {
        return blockId;
    }

    public float GetSpacing()
    {
        return blockSpacingDistance;
    }
#if (UNITY_EDITOR)
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.magenta;
        List<Vector2Int> gridpoints = VoronoiCityUtils.FindSurroundingPositions(new Vector2Int(0, 0), dimensions.x, dimensions.y, false);

        for (int i = 0; i < gridpoints.Count; i++)
        {
            int nextIndex = i + 1;
            if (nextIndex >= gridpoints.Count)
            {
                nextIndex -= gridpoints.Count;
            }

            Vector3 pos = new Vector3((gridpoints[i].x * gridScale) + transform.position.x, this.transform.position.y, (gridpoints[i].y * gridScale) + transform.position.z);

            Gizmos.DrawSphere(pos, 0.1f);
        }

    }
#endif
}
