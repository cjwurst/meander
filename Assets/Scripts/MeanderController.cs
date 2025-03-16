using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MOL;

public class MeanderController : MonoBehaviour
{
    [SerializeField] TerrainMessenger terrainMessenger;

    [SerializeField, Range(0f, 5f)] float minSampleInterval;
    [SerializeField, Range(0.1f, 10f)] float maxSampleInterval;

    [Space()]
    [SerializeField, Range(0f, 5f)] float cutThreshold;

    [Space()]
    [SerializeField] bool meander = false;
    [SerializeField] bool recalculate = false;
    [SerializeField] bool reset = false;

    [Space()]
    [SerializeField] bool showTangent = false;
    [SerializeField] bool showBinormal = false;
    [SerializeField] bool showMovementVector = false;
    [SerializeField, Range(0f, 15f)] float vectorGraphicScale = 1f;

    [Space()]
    [SerializeField, Range(0f, 5f)] float tangentCoeff = 1f;
    [SerializeField, Range(-5f, 5f)] float binormalCoeff = 1f;
    [SerializeField, Range(0f, 5f)] float normalCoeff = 1f;
    [SerializeField, Range(0f, 5f)] float totalCoeff = 1f;
    [SerializeField] float bufferBase;

    LineRenderer lineRenderer;
    List<Action> destroyLineCommands = new List<Action>();

    static float floatTolerance = float.Epsilon * 10f;

    Vector3 entryMod;

    Vector3[] linePoints
    {
        get
        {
            Vector3[] points = new Vector3[lineRenderer.positionCount];
            lineRenderer.GetPositions(points);
            return points;
        }
        set
        {
            Vector3[] vs = new Vector3[value.Length];
            for (int i = 0; i < vs.Length; i++)
                vs[i] = terrainMessenger.SampleTerrainHeight(value[i]) + 0.1f * Vector3.up;

            lineRenderer.positionCount = vs.Length;
            lineRenderer.SetPositions(vs);
        }
    }

    void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();

        entryMod = new Vector3(0f, 0f, UnityEngine.Random.Range(-1f, 1f));
    }

    void Update()
    {
        if (reset)
        {
            ResetCurve();
            linePoints = Recalculate(linePoints);
            reset = false;
        }

        if (recalculate)
        {
            linePoints = Recalculate(linePoints);
            recalculate = false;
        }

        if (meander)
        {
            Vector3[] points = Recalculate(linePoints);
            points = Meander(points);
            points = Cut(points, cutThreshold);
            linePoints = points;
        }
    }

    void ResetCurve()
    {
        Vector3[] positions = new Vector3[2];
        float interval = 100f / 49;
        for (int i = 0; i < 2; i++)
        {
            float x = i + 1;
            positions[i] = new Vector3(x * interval, 0f, 2f * Mathf.Sin(x));
        }

        linePoints = positions;
    }

    Vector3[] Recalculate(Vector3[] points)
    {
        Vector3[] samples = GetEvenSamples(points, minSampleInterval, maxSampleInterval);

        foreach (Action command in destroyLineCommands) command.Invoke();
        for (int i = 1; i < samples.Length - 1; i++)
        {
            CurveSegment segment = new CurveSegment(samples[i - 1], samples[i], samples[i + 1]);
            if (showBinormal)
                destroyLineCommands.Add(DrawTool.DrawLine(segment.center + Vector3.up,
                    segment.center + vectorGraphicScale * binormalCoeff * segment.weightedBinormal + Vector3.up, Color.blue));
            if (showTangent)
                destroyLineCommands.Add(DrawTool.DrawLine(segment.center + Vector3.up,
                    segment.center + vectorGraphicScale * tangentCoeff * segment.tangent + Vector3.up, Color.red));
            if (showMovementVector)
                destroyLineCommands.Add(DrawTool.DrawLine(segment.center + Vector3.up,
                    segment.center + vectorGraphicScale * GetMovementVector(segment) + Vector3.up, Color.yellow));
        }

        return samples;
    }

    Vector3[] Meander(Vector3[] points)
    {
        Vector3[] updatedPoints = new Vector3[points.Length];
        updatedPoints[0] = Vector3.zero;

        entryMod += totalCoeff * new Vector3(UnityEngine.Random.Range(0f, 0.5f), 0f, UnityEngine.Random.Range(-1f, 1f));
        entryMod.Normalize();
        Vector3 entryVector = (points[points.Length - 1] - points[0]).normalized / 2f;
        Vector3[] movementBuffer = new Vector3[5];
        for (int i = 0; i < movementBuffer.Length; i++)
            movementBuffer[i] = ((movementBuffer.Length - i - 1f) * entryMod.normalized + entryVector).normalized;

        for(int i = 1; i < updatedPoints.Length; i++)
        {
            Vector3 rightPoint;
            if (i == updatedPoints.Length - 1) rightPoint = points[i];
            else rightPoint = points[i + 1];
            CurveSegment segment = new CurveSegment(points[i - 1], points[i], rightPoint);
            Vector3 movementVector = GetMovementVector(segment);
            updatedPoints[i] = points[i] + movementVector;
            movementBuffer[movementBuffer.Length - 1] = movementVector;
            for (int j = 1; j < movementBuffer.Length; j++)
            {
                updatedPoints[i] += Mathf.Pow(bufferBase, j) * movementBuffer[movementBuffer.Length - 1 - j];
                movementBuffer[movementBuffer.Length - 1 - j] = movementBuffer[movementBuffer.Length - j];
            }
        }

        return updatedPoints;
    }

    Vector3[] Cut(Vector3[] points, float threshold)
    {
        List<Vector3> pointsToKeep = new List<Vector3>(points);
        for (int i = pointsToKeep.Count - 1; i > 1; i--)
        {
            for (int j = i - 2; j > 0; j--)
            {
                if ((pointsToKeep[i] - pointsToKeep[j]).magnitude < threshold)
                    for (int k = i - 1; k > j; k--)
                        { pointsToKeep.RemoveAt(k); i--; }
            }
        }     
        return pointsToKeep.ToArray();
    }

    // returns the vector to be added to the movement buffer
    Vector3 GetMovementVector(CurveSegment segment)
    {
        Vector3 normal = normalCoeff * terrainMessenger.SampleTerrainNormal(segment.center);
        return totalCoeff * segment.curvature * (binormalCoeff * segment.weightedBinormal + tangentCoeff * segment.tangent) 
            + new Vector3(normal.x, 0f, normal.z);
    }

    // Adds and removes points in *points* so that the distance between adjacent points is between *minInterval* and *maxInterval*
    static Vector3[] GetEvenSamples (Vector3[] points, float minInterval, float maxInterval)
    {
        if (maxInterval < 2f * minInterval) Debug.LogWarning("*maxInterval* should be at least twice *minInterval* to avoid oscillation.");

        List<Vector3> samples = new List<Vector3>();
        float distanceBackward = float.PositiveInfinity;
        for (int i = 0; i < points.Length - 1; i++)
        {
            float distanceForward = (points[i + 1] - points[i]).magnitude;
            bool isTooClose = distanceBackward < minInterval;           // is too close to the previous point
            bool isTooFar = distanceForward > maxInterval;              // is too far from the next point

            if (!isTooClose)
                samples.Add(points[i]);
            if (isTooFar)
                samples.Add(Interpolate(points[i], points[i + 1], distanceForward /= 2f));

            // Set *distanceBackward* for the next loop.
            if (isTooClose && !isTooFar)
                distanceBackward += distanceForward;
            else
                distanceBackward = distanceForward;
        }
        samples.Add(points[points.Length - 1]);
        return samples.ToArray();
    }

    // returns the point at *x* distance along the line between *a* and *c*
    static Vector3 Interpolate(Vector3 a, Vector3 c, float x)
    {
        Matrix changeOfBasis = MatrixUtility.GetChangeOfBasis(a, c);
        Vector3 b = new Vector3(x, 0f, 0f);
        b = MatrixUtility.ToStandardBasis(changeOfBasis, a, b);
        return b;
    }

    class CurveSegment
    {
        public Vector3 left { get; private set; }
        public Vector3 center { get; private set; }
        public Vector3 right { get; private set; }

        public Vector3 tangent { get; private set; }
        public Vector3 binormal { get; private set; }

        public float signedCurvature { get; private set; }              // the Menger curvature, signed based on handedness with the tangent and normal
        public float curvature { get; private set; }

        public Vector3 weightedBinormal { get { return binormal * signedCurvature; } }          // weighted by *signedCurvature*

        public CurveSegment(Vector3 _left, Vector3 _center, Vector3 _right)
        {
            left = _left;
            center = _center;
            right = _right;

            tangent = (right - left).normalized;
            binormal = Vector3.Cross(tangent, new Vector3(0f, 1f, 0f)).normalized;

            float a = (center - left).magnitude;
            float b = (center - right).magnitude;
            float c = (left - right).magnitude;
            float p = a * b * c;
            if (Mathf.Abs(p) < floatTolerance)
            {
                curvature = 1f;
                signedCurvature = 1f;
                return;
            }

            float s = (a + b + c) / 2f;
            float radicand = s * (s - a) * (s - b) * (s - c);
            float area = 0;
            if (radicand > floatTolerance)
                area = Mathf.Sqrt(radicand);

            curvature = 4f * area / p;
            Matrix segmentBasis = MatrixUtility.GetChangeOfBasis(left, right);
            signedCurvature = curvature * -Mathf.Sign(MatrixUtility.ToCustomBasis(segmentBasis, left, center).z);
        }
    }
}
