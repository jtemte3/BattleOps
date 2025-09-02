using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CompassPoint : MonoBehaviour
{
    public CompassVisual compassVisualPrefab;
    private CompassVisual compassVisualObj;
    public CompassManager compassManager;
    public bool showText;
    public enum PointType { Directional, Waypoint}
    public PointType pointType;
    // Start is called before the first frame update
    void Awake()
    {
        compassManager = FindAnyObjectByType<CompassManager>();
        compassVisualObj = Instantiate(compassVisualPrefab);
        if (pointType == PointType.Directional)
        {
            compassVisualObj.isDirectionPoint = true;
            compassVisualObj.pointPosition = this.transform.localPosition;
            if (showText)
            {
                DeterminCardinalDirection(compassVisualObj);
            }
        }
        if (pointType == PointType.Waypoint)
        {
            compassVisualObj.pointPosition = this.transform.position;
            compassVisualObj.isDirectionPoint = false;
        }

        compassManager.RegisterMarker(compassVisualObj);
    }

    public void SetPointActive(bool state)
    {
        compassVisualObj.gameObject.SetActive(state);
    }

    public void UpdatePointPosition(Vector3 newPosition)
    {
        compassVisualObj.pointPosition = newPosition;
    }

    public void DeterminCardinalDirection(CompassVisual visualObject)
    {
        Vector3 direction = visualObject.pointPosition;

        switch (direction)
        {
            case Vector3 dir when dir.Equals(new Vector3(0, 0, 1)):
                {
                    visualObject.GetComponentInChildren<TMP_Text>().text = "N";
                    break;
                }
            case Vector3 dir when dir.Equals(new Vector3(0, 0, -1)):
                {
                    visualObject.GetComponentInChildren<TMP_Text>().text = "S";
                    break;
                }
            case Vector3 dir when dir.Equals(new Vector3(1, 0, 0)):
                {
                    visualObject.GetComponentInChildren<TMP_Text>().text = "E";
                    break;
                }
            case Vector3 dir when dir.Equals(new Vector3(-1, 0, 0)):
                {
                    visualObject.GetComponentInChildren<TMP_Text>().text = "W";
                    break;
                }
            case Vector3 dir when dir.Equals(new Vector3(1, 0, 1)):
                {
                    visualObject.GetComponentInChildren<TMP_Text>().text = "NE";
                    break;
                }
            case Vector3 dir when dir.Equals(new Vector3(-1, 0, 1)):
                {
                    visualObject.GetComponentInChildren<TMP_Text>().text = "NW";
                    break;
                }
            case Vector3 dir when dir.Equals(new Vector3(-1, 0, -1)):
                {
                    visualObject.GetComponentInChildren<TMP_Text>().text = "SW";
                    break;
                }
            case Vector3 dir when dir.Equals(new Vector3(1, 0, -1)):
                {
                    visualObject.GetComponentInChildren<TMP_Text>().text = "SE";
                    break;
                }
        }
    }
}
