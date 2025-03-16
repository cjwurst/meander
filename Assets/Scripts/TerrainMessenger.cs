using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Custom/Terrain Messenger")]
public class TerrainMessenger : ScriptableObject
{
    Terrain terrain;
    TerrainData terrainData;

    List<Action> destroyLineCommands = new List<Action>();

    public EventDispatcher<Vector3> e_click;
    Action<Vector3> ec_click;

    public bool isInit { get; private set; }
    public void Init (Terrain _terrain, TerrainData _terrainData)
    {
        terrain = _terrain;
        terrainData = _terrainData;

        e_click = new EventDispatcher<Vector3>(out ec_click);

        isInit = true;
    }

    public void OnClick(Vector3 point) { ec_click.Invoke(point); }

    public Vector3 SampleTerrainHeight(Vector3 v) { return new Vector3(v.x, terrain.SampleHeight(v), v.z); }

    public Vector3 SampleTerrainNormal(Vector3 v)
    {
        Vector3 bounds = 2f * terrainData.bounds.extents;
        Vector3 localSpace = terrain.transform.InverseTransformPoint(v);
        localSpace.x /= bounds.x;
        localSpace.z /= bounds.z;

        Vector3 normal = terrainData.GetInterpolatedNormal(localSpace.x, localSpace.z);
        //DrawTool.DrawLine(v, v + 10f * normal, Color.black);
        return normal;
    }

    public void DeleteLines()
    {
        foreach (Action command in destroyLineCommands) command.Invoke();
        destroyLineCommands = new List<Action>();
    }
}
