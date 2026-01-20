using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class SimpleCurveBuilder
{
    // Wrapper that picks first, middle, last nodes of a segment and samples the arc
    public static List<Vector3> SampleThreePointArc(List<Vector2Int> segment, float gridScale, int samples)
    {
        if (segment == null || segment.Count == 0)
            return new List<Vector3>();

        int i0 = 0;
        int i1 = segment.Count / 2;                 // middle node index
        int i2 = segment.Count - 1;

        Vector3 start = new Vector3(segment[i0].x * gridScale, 0f, segment[i0].y * gridScale);
        Vector3 mid = new Vector3(segment[i1].x * gridScale, 0f, segment[i1].y * gridScale);
        Vector3 end = new Vector3(segment[i2].x * gridScale, 0f, segment[i2].y * gridScale);

        /*List<Vector3> pts = new()
        {
            start, mid, end
        };*/

        return SampleCircularArc(start, mid, end, samples);
        //return pts;
    }

    // Sample an arc that passes exactly through start, mid, end (XZ plane). 
    // Falls back to a straight line if points are nearly collinear.
    private static List<Vector3> SampleCircularArc(Vector3 start, Vector3 mid, Vector3 end, int samples)
    {
        Vector2 A = new Vector2(start.x, start.z);
        Vector2 B = new Vector2(mid.x, mid.z);
        Vector2 C = new Vector2(end.x, end.z);

        // Circumcenter of triangle ABC
        float a = A.x - B.x, b = A.y - B.y;
        float c = A.x - C.x, d = A.y - C.y;
        float e = ((A.x * A.x - B.x * B.x) + (A.y * A.y - B.y * B.y)) * 0.5f;
        float f = ((A.x * A.x - C.x * C.x) + (A.y * A.y - C.y * C.y)) * 0.5f;
        float det = a * d - b * c;

        var pts = new List<Vector3>();

        // Nearly collinear → linear sample from start to end
        if (Mathf.Abs(det) < 1e-5f)
        {
            for (int i = 0; i <= samples; i++)
            {
                float t = i / (float)samples;
                Vector2 p2 = Vector2.Lerp(A, C, t);
                pts.Add(new Vector3(p2.x, start.y, p2.y));
            }
            return pts;
        }

        float cx = (d * e - b * f) / det;
        float cy = (-c * e + a * f) / det;
        Vector2 O = new Vector2(cx, cy);
        float r = Vector2.Distance(O, A);

        float angA = Mathf.Atan2(A.y - O.y, A.x - O.x);
        float angB = Mathf.Atan2(B.y - O.y, B.x - O.x);
        float angC = Mathf.Atan2(C.y - O.y, C.x - O.x);

        // CCW sweep from A to C
        float twoPI = 2f * Mathf.PI;
        float ccw_AC = Mathf.Repeat(angC - angA, twoPI); // in [0, 2π)
        float ccw_AM = Mathf.Repeat(angB - angA, twoPI);

        bool useCCW = ccw_AM > 0f && ccw_AM < ccw_AC;    // does CCW A→C pass through mid?
        float sweep = useCCW ? ccw_AC : (ccw_AC - twoPI); // CW if not CCW

        for (int i = 0; i <= samples; i++)
        {
            float t = i / (float)samples;
            float ang = angA + sweep * t;
            Vector2 p = O + new Vector2(Mathf.Cos(ang), Mathf.Sin(ang)) * r;
            pts.Add(new Vector3(p.x, start.y, p.y));
        }

        return pts;
    }

    // --------------------------
    // Sample spline points along a segment
    // --------------------------
    public static List<Vector3> SimpleSpline(List<Vector2Int> segment, int samples)
    {
        List<Vector3> points = segment.Select(p => new Vector3(p.x, 0, p.y)).ToList();
        List<Vector3> splinePoints = new List<Vector3>();

        for (int i = 0; i < points.Count - 1; i++)
        {
            Vector3 p0 = i == 0 ? points[i] : points[i - 1];
            Vector3 p1 = points[i];
            Vector3 p2 = points[i + 1];
            Vector3 p3 = (i + 2 < points.Count) ? points[i + 2] : points[i + 1];

            for (int s = 0; s < samples; s++)
            {
                float t = s / (float)samples;
                splinePoints.Add(CatmullRom(p0, p1, p2, p3, t));
            }
        }

        splinePoints.Add(points.Last());
        return splinePoints;
    }

    // --------------------------
    // Sample spline points along a segment
    // --------------------------
    public static List<Vector3> SimpleSplineFromTrasforms(List<Vector3> path, int samples)
    {
        List<Vector3> splinePoints = new List<Vector3>();

        for (int i = 0; i < path.Count - 1; i++)
        {
            Vector3 p0 = i == 0 ? path[i] : path[i - 1];
            Vector3 p1 = path[i];
            Vector3 p2 = path[i + 1];
            Vector3 p3 = (i + 2 < path.Count) ? path[i + 2] : path[i + 1];

            for (int s = 0; s < samples; s++)
            {
                float t = s / (float)samples;
                splinePoints.Add(CatmullRom(p0, p1, p2, p3, t));
            }
        }

        splinePoints.Add(path.Last());
        return splinePoints;
    }

    // --------------------------
    // Catmull-Rom spline
    // --------------------------
    private static Vector3 CatmullRom(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
    {
        float t2 = t * t;
        float t3 = t2 * t;
        return 0.5f * ((2f * p1) +
                       (-p0 + p2) * t +
                       (2f * p0 - 5f * p1 + 4f * p2 - p3) * t2 +
                       (-p0 + 3f * p1 - 3f * p2 + p3) * t3);
    }
}
