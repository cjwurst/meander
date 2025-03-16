using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MOL;

public class River : WaterBody
{
    public RiverConfig config;

    LineRenderer lineRenderer;

    static float floatTolerance = float.Epsilon * 10f;

    bool isInvalid = false;             // flag to destroy *this* in *Init*

    public River(TerrainMessenger _terrainMessenger, float _volume, float _capacity, RiverConfig _config, WaterPoint _parentPoint, Vector3 start, Vector3 end) : 
        base(_terrainMessenger, _volume, _capacity, true)
    {
        WaterPoint startPoint = new WaterPoint(start, 0.5f, _parentPoint);
        WaterPoint endPoint = new WaterPoint(end, 0.5f, startPoint);
        Init(_config, new WaterPoint[] { startPoint, endPoint });
    }
    public River(TerrainMessenger _terrainMessenger, float _volume, float _capacity, RiverConfig _config, WaterPoint _parentPoint,  WaterPoint[] _waterPoints) :
        base(_terrainMessenger, _volume, _capacity, true)
    {
        if (_waterPoints.Length > 0)
            _waterPoints[0].AddParents(_parentPoint);
    
        WaterPoint[] points = new WaterPoint[_waterPoints.Length];
        Array.Copy(_waterPoints, points, points.Length);
        if (points.Length == 1)
        {
            points = new WaterPoint[] 
            {
                _waterPoints[0],
                new WaterPoint(_waterPoints[0].position + _waterPoints[0].position - _waterPoints[0].meanParentPosition, _waterPoints[0].radius, _waterPoints[0])
            };
        }
        Init(_config, points);

        if (points.Length == 0)
            isInvalid = true;
    }

    void Init(RiverConfig _config, WaterPoint[] _waterPoints)
    {
        config = _config;
        waterPoints = _waterPoints;
    }

    public override void Init() { if (isInvalid) Destroy(); }

    protected override void CreateGameObject(GameObject gameObject)
    {
        base.CreateGameObject(gameObject);

        lineRenderer = gameObject.AddComponent<LineRenderer>();
        lineRenderer.material = (Material)Resources.Load("Unlit_Line", typeof(Material));
        lineRenderer.material.color = new Color(56f / 255f, 116f / 255f, 176f / 255f);
    }

    protected override Action CollideWithSelf(WaterPoint point1, WaterPoint point2, bool isDominant)
    {
        List<WaterPoint> pointsToKeep = new List<WaterPoint>(waterPoints);
        int[] indices = new int[] { Array.IndexOf(waterPoints, point1), Array.IndexOf(waterPoints, point2) };
        int distance = Mathf.Abs(indices[1] - indices[0]);
        if (distance > 1)
        {
            Array.Sort(indices);
            if (distance > 2)
            {
                List<WaterPoint> oxbowPoints = new List<WaterPoint>(pointsToKeep.GetRange(indices[0] + 1, indices[1] - indices[0] - 1));
                CreateBody(new Oxbow(terrainMessenger, 1f, 1f, oxbowPoints.ToArray()));
            }
            RemovePointRange(pointsToKeep, indices[0], indices[1]);
        }
        waterPoints = pointsToKeep.ToArray();
        return () => { };
    }

    protected override Action CollideWithRiver(WaterPoint externalPoint, WaterPoint internalPoint, bool isDominant)
    {
        Action postCollisionCommand = () => { };
        int internalIndex = Array.IndexOf(waterPoints, internalPoint);
        if (!isDominant)
        {
            List<WaterPoint> pointsToKeep = new List<WaterPoint>(waterPoints);
            pointsToKeep.Remove(internalPoint);
            waterPoints = pointsToKeep.ToArray();
            postCollisionCommand = () => internalPoint.Estrange();
            Split(internalIndex, externalPoint);
        }
        else
        {
            internalPoint.AddParents(externalPoint.parentArray);
            internalPoint.AddChildren(externalPoint.childArray);
            Split(internalIndex);
        }
        return postCollisionCommand;
    }

    public override void Flow()
    {
        if (config.split)
        {
            Split(Mathf.RoundToInt(waterPoints.Length / 2f));
            config.split = false;
        }

        if (waterPoints.Length < 2) { Destroy(); return; }

        waterPoints = Meander(waterPoints);
        waterPoints = GetEvenSamples(waterPoints, config.minSampleInterval, config.maxSampleInterval);

        if (config.debug)
        {
            foreach (WaterPoint waterPoint in waterPoints)
            {
                int i = Array.IndexOf(waterPoints, waterPoint);
                foreach (WaterPoint parent in waterPoint.parentArray)
                {
                    int j = Array.IndexOf(waterPoints, parent);
                    if (j > -1 && i < j)
                    {
                        MonoBehaviour.print("DEBUG: " + "Failed to verify at " + j + " and " + i);
                        Debug.Break();
                    } 
                }
            }
            MonoBehaviour.print("\n");
        }

        base.Flow();
    }

    WaterPoint[] Meander(WaterPoint[] points)
    {
        for (int i = 0; i < points.Length; i++)
        {
            Vector3 rightPoint = points[i].meanChildPosition;
            Vector3 leftPoint = points[i].meanParentPosition;

            CurveSegment segment = new CurveSegment(leftPoint, points[i], rightPoint);
            points[i].movementVector = GetMovementVector(segment);
            points[i].position += points[i].movementVector + points[i].GetMomentum(config.momentumBase, config.momentumDepth);
        }
        return points;
    }

    Vector3 GetMovementVector(CurveSegment segment)
    {
        Vector3 normal = config.normalCoeff * terrainMessenger.SampleTerrainNormal(segment.center);
        return config.totalCoeff * segment.curvature * (config.binormalCoeff * segment.weightedBinormal + config.tangentCoeff * segment.tangent)
            + new Vector3(normal.x, 0f, normal.z);
    }

    // Adds and removes elements of *points* so that the distance between adjacent points is between *minInterval* and *maxInterval*
    WaterPoint[] GetEvenSamples(WaterPoint[] points, float minInterval, float maxInterval)
    {
        if (maxInterval < 2f * minInterval) Debug.LogWarning("*maxInterval* should be at least twice *minInterval* to avoid oscillation.");

        // Find indices marking where to remove or add new *WaterPoint*s.
        List<WaterPoint> updatedPoints = new List<WaterPoint>(points);
        HashSet<int> indicesToAdd = new HashSet<int>();                         // New points will be added after these indices.
        HashSet<int> indicesToRemove = new HashSet<int>();
        {
            float distanceBackward = float.PositiveInfinity;
            for (int i = 0; i < points.Length - 1; i++)
            {
                float distanceForward = (points[i + 1].position - points[i].position).magnitude;
                bool isTooClose = distanceBackward < minInterval;           // is too close to the previous point
                bool isTooFar = distanceForward > maxInterval;              // is too far from the next point

                if (isTooFar)
                {
                    distanceForward /= 2f;
                    indicesToAdd.Add(i);
                }
                if (isTooClose)
                    indicesToRemove.Add(i);

                // Set *distanceBackward* for the next loop.
                if (isTooClose && !isTooFar)
                    distanceBackward += distanceForward;
                else
                    distanceBackward = distanceForward;
            }
        }

        // Add and remove points.
        {
            int iMod = 0;
            float startDistance = (waterPoints[0].position - updatedPoints[0].position).magnitude;
            if (startDistance > maxInterval)
            {
                WaterPoint pointToAdd = new WaterPoint(Interpolate(waterPoints[0].position, updatedPoints[0], startDistance / 2f), 0.5f, waterPoints[0]);
                if (config.debug) MonoBehaviour.print("DEBUG: " + "Point added at 0");
                InsertPoint(updatedPoints, 0, pointToAdd);
                iMod++;
            }
            for (int i = 0; i < points.Length - 1; i++)
            {
                int j = i + iMod;                                       // the index in *updatedPoints* of the same point in *points*
                bool adds = indicesToAdd.Contains(i);
                bool removes = indicesToRemove.Contains(i);

                if (adds)
                {
                    float halfDistance = (points[i + 1].position - points[i].position).magnitude / 2f;
                    WaterPoint pointToAdd = new WaterPoint(Interpolate(points[i], points[i + 1], halfDistance), 0.5f, points[i]);
                    if (config.debug) MonoBehaviour.print("DEBUG: " + "Point added at " + i);
                    InsertPoint(updatedPoints, j + 1, pointToAdd);
                }
                if (removes) RemovePoint(updatedPoints, j);
                if (config.debug && removes) MonoBehaviour.print("DEBUG: " + "Point removed at " + i);

                if (adds) iMod++;
                if (removes) iMod--;
            }
        }

        return updatedPoints.ToArray();
    }

    void Split(int index, WaterPoint parent)
    {
        List<WaterPoint> upstreamPoints = new List<WaterPoint>();
        List<WaterPoint> downstreamPoints = new List<WaterPoint>();
        for (int i = 0; i < waterPoints.Length; i++)
            if (i < index)
                upstreamPoints.Add(waterPoints[i]);
            else
                downstreamPoints.Add(waterPoints[i]);
        if (upstreamPoints.Count > 0)
            CreateSink(new River(terrainMessenger, 1f, 5f, config, parent, downstreamPoints.ToArray()), 1f);
        waterPoints = upstreamPoints.ToArray();
    }
    void Split(int index)
    {
        if (waterPoints.Length < 1 || index == 0) return;
        Split(index, waterPoints[index - 1]);
    }

    static void InsertPoint(List<WaterPoint> list, int i, WaterPoint point)
    {
        point.AddParents(list[i].parentArray);
        list[i].RemoveAllParents();
        list[i].AddParents(point);
        list.Insert(i, point);
    }

    static void RemovePoint(List<WaterPoint> list, int i)
    {
        foreach (WaterPoint parent in list[i].parentArray)
            parent.AddChildren(list[i].childArray);
        list[i].Estrange();
        list.RemoveAt(i);
    }
    
    // Remove the points in *list* between *i* and *j*.
    static void RemovePointRange(List<WaterPoint> list, int i, int j, bool isInclusive = false)
    {
        if (!isInclusive) i++;
        else j++;
        for (; i < j; j--) RemovePoint(list, i);
    }

    // returns the point at *x* distance along the line between *a* and *c*
    static Vector3 Interpolate(Vector3 a, Vector3 c, float x)
    {
        Matrix changeOfBasis = MatrixUtility.GetChangeOfBasis(a, c);
        Vector3 b = new Vector3(x, 0f, 0f);
        b = MatrixUtility.ToStandardBasis(changeOfBasis, a, b);
        return b;
    }

    List<Action> eraseCommands = new List<Action>();
    public override void Draw()
    {
        Vector3[] vectors = new Vector3[waterPoints.Length + 1];
        vectors[0] = terrainMessenger.SampleTerrainHeight(waterPoints[0]) + 0.1f * Vector3.up;
        for (int i = 1; i < vectors.Length; i++)
            vectors[i] = terrainMessenger.SampleTerrainHeight(waterPoints[i - 1]) + 0.1f * Vector3.up;

        foreach (Action command in eraseCommands) command.Invoke();
        if (config.debugDraw)
            foreach (WaterPoint riverPoint in waterPoints)
                foreach (WaterPoint parent in riverPoint.parentArray)
                    eraseCommands.Add(DrawTool.DrawLine(parent + 0.5f * Vector3.up, riverPoint + 0.5f * Vector3.up, Color.white));

        lineRenderer.enabled = true;
        lineRenderer.startWidth = 0.5f;
        lineRenderer.endWidth = 0.5f;
        lineRenderer.positionCount = vectors.Length;
        lineRenderer.SetPositions(vectors);
    }
    public override void Erase() { lineRenderer.enabled = false; }

    protected override void Destroy()
    {
        base.Destroy();
        List<WaterPoint> pointsToKeep = new List<WaterPoint>(waterPoints);
        RemovePointRange(pointsToKeep, 0, pointsToKeep.Count - 1, true);
        waterPoints = pointsToKeep.ToArray();
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
