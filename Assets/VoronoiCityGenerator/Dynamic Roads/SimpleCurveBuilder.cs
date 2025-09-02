using System.Collections;
using System.Collections.Generic;
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
    public static List<Vector3> SampleCircularArc(Vector3 start, Vector3 mid, Vector3 end, int samples)
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
}
