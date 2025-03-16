using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterCycle : MonoBehaviour
{
    [SerializeField] TerrainMessenger terrainMessenger;
    [SerializeField] RiverConfig riverConfig;

    Digraph<WaterBody> heirarchy = new Digraph<WaterBody>();
    Dictionary<WaterBody, List<Action>> unsubscribeCommands = new Dictionary<WaterBody, List<Action>>();
    bool isSubscribed = false;

    void Update()
    { 
        if (terrainMessenger.isInit && !isSubscribed)
        {
            terrainMessenger.e_click.Subscribe((v) => CreateBody(new Spring(terrainMessenger, riverConfig, v)));
            isSubscribed = true;
        }

        IEnumerable<WaterBody> bodies = heirarchy.verticesBySuccessorCount;

        foreach (WaterBody body in bodies)
        {
            body.Flow();
            body.Draw();
        }

        List<CollisionPoint> collisionPoints = new List<CollisionPoint>();
        foreach (WaterBody body in heirarchy.verticesBySuccessorCount)
            collisionPoints.AddRange(body.collisionPoints);
        InvokeCollisions(collisionPoints);
    }

    void InvokeCollisions(List<CollisionPoint> collisionPoints)
    {
        // Find the least size of *grid* so that each colliding pair of points is guaranteed to be within one cell of each other.
        float maxRadius = 0f;
        foreach (CollisionPoint point in collisionPoints)
            maxRadius = Mathf.Max(maxRadius, point.radius);

        // Round each point in *collisionPoints* to a cell in *grid*.
        VoxelGrid grid = new VoxelGrid(maxRadius);
        Dictionary<Vector3Int, List<CollisionPoint>> cellLists = new Dictionary<Vector3Int, List<CollisionPoint>>();
        foreach (CollisionPoint point in collisionPoints)
        {
            Vector3Int roundedPosition = grid.WorldToCell(point.position);
            if (cellLists.TryGetValue(roundedPosition, out List<CollisionPoint> list)) list.Add(point);
            else cellLists.Add(roundedPosition, new List<CollisionPoint>(new CollisionPoint[] { point }));
        }

        List<Action> postCollisionCommands = new List<Action>();
        foreach(KeyValuePair<Vector3Int, List<CollisionPoint>> cellList in cellLists)
        {
            // Find all the points in the 27 neighboring cell lists around *cellList*, including the points in *cellList*.
            List<CollisionPoint> neighborPoints = new List<CollisionPoint>();
            foreach (Vector3Int neighborVector in VoxelGrid.neighborVectors)
                if (cellLists.TryGetValue(cellList.Key + neighborVector, out List<CollisionPoint> neighbors))
                    neighborPoints.AddRange(neighbors);

            // Check if there is a collision between each of the points in this cell and each of the points in *neighborPoints*. 
            //If there is, invoke its command.
            foreach (CollisionPoint point in cellList.Value)
            {
                foreach (CollisionPoint neighbor in neighborPoints)
                {
                    if (neighbor == point) continue;
                    if ((neighbor.position - point.position).magnitude < point.radius)
                        point.command.Invoke(neighbor.waterPoint, neighbor.type);
                }
            }
        }
        foreach (Action command in postCollisionCommands) command.Invoke();
    }

    void OnCreateBody (WaterBody waterBody)
    {
        List<Action> commandList = new List<Action>();
        unsubscribeCommands.Add(waterBody, commandList);
        commandList.Add(waterBody.e_createBody.Subscribe((w) => OnCreateBody(w)));
        commandList.Add(waterBody.e_destroyBody.Subscribe((w) => OnDestroyBody(w)));
        commandList.Add(waterBody.e_addSink.Subscribe((s) => OnAddSink(s)));
        commandList.Add(waterBody.e_removeSink.Subscribe((s) => OnRemoveSink(s)));

        heirarchy.AddVertices(waterBody);
        waterBody.Init();
    }
    void CreateBody(WaterBody waterBody) { OnCreateBody(waterBody); }

    void OnDestroyBody (WaterBody waterBody)
    {
        if (unsubscribeCommands.TryGetValue(waterBody, out List<Action> commands))
            foreach (Action command in commands)
                command.Invoke();

        heirarchy.RemoveVertices(waterBody);
    }

    void OnAddSink (Tuple<WaterBody, WaterBody> s)
    {
        heirarchy.AddEdges(s);
    }

    void OnRemoveSink (Tuple<WaterBody, WaterBody> s)
    {
        heirarchy.RemoveEdges(s);
    } 
}
