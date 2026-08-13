using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CompassManager : MonoBehaviour
{
    public float pointVisibilityAngle = 180f;
    public float pointMinScale = 0.5f;
    public float pointDistanceMinScale = 50f;

    public GameObject player;
    public Transform cameraTransform;
    public List<CompassVisual> pointList = new List<CompassVisual>();
    RectTransform rectTransform;
    float widthMultiplier;
    float heightOffset;
    // Start is called before the first frame update
    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        widthMultiplier = rectTransform.rect.width / pointVisibilityAngle;
        heightOffset = -rectTransform.rect.height / 2;
    }

    // Update is called once per frame
    void Update()
    {
        foreach (var point in pointList)
        {
            float distanceRatio = 1;
            float angle;

            Vector3 viewForward;

            if (cameraTransform != null)
            {
                viewForward = Vector3.ProjectOnPlane(cameraTransform.forward, Vector3.up);
            }
            else
            {
                viewForward = Vector3.ProjectOnPlane(player.transform.forward, Vector3.up);
            }

            if (point.isDirectionPoint)
            {
                angle = Vector3.SignedAngle(viewForward, point.pointPosition.normalized, Vector3.up);
            }
            else
            {
                Vector3 targetDir = (point.pointPosition - player.transform.position).normalized;
                targetDir = Vector3.ProjectOnPlane(targetDir, Vector3.up);

                angle = Vector3.SignedAngle(viewForward, targetDir, Vector3.up);

                Vector3 directionVector = point.pointPosition - player.transform.position;

                distanceRatio = directionVector.magnitude / pointDistanceMinScale;
                distanceRatio = Mathf.Clamp01(distanceRatio);
            }

            if (angle > -pointVisibilityAngle / 2 && angle < pointVisibilityAngle / 2)
            {
                point.canvasGroup.alpha = 1;

                point.canvasGroup.transform.localPosition = new Vector2(widthMultiplier * angle, heightOffset);
                point.canvasGroup.transform.localScale = Vector3.one * Mathf.Lerp(1, pointMinScale, distanceRatio);
            }
            else
            {
                point.canvasGroup.alpha = 0;
            }
        }
    }

    public void RegisterMarker(CompassVisual compassVisual)
    {
        compassVisual.transform.SetParent(this.transform);
        pointList.Add(compassVisual);
    }

    public void UnRegisterMarker(CompassVisual compassVisual)
    {
        if (pointList.Contains(compassVisual))
        {
            pointList.Remove(compassVisual);
            Destroy(compassVisual);
        }
    }
}
