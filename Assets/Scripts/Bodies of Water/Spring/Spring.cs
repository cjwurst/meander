using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spring : WaterBody
{
    RiverConfig riverConfig;

    WaterPoint point;

    public Spring (TerrainMessenger _terrainMessenger, RiverConfig _riverConfig, Vector3 position) : base(_terrainMessenger, float.PositiveInfinity, float.PositiveInfinity)
    {
        riverConfig = _riverConfig;
        point = new WaterPoint(position, 0f);
    }

    public override void Init()
    {
        CreateSink(new River(terrainMessenger, 1f, 5f, riverConfig, point, point.position + Vector3.right, point.position + new Vector3(0, 0, 1f)), 1f);
    }
}
