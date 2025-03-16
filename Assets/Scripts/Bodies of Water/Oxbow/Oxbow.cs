using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Oxbow : WaterBody
{
    LineRenderer lineRenderer;

    public Oxbow(TerrainMessenger _terrainMessenger, float _volume, float _capacity, WaterPoint[] _waterPoints) : 
        base(_terrainMessenger, _volume, _capacity, true)
    {
        waterPoints = _waterPoints;
    }

    protected override void CreateGameObject(GameObject gameObject)
    {
        base.CreateGameObject(gameObject);

        lineRenderer = gameObject.AddComponent<LineRenderer>();
        lineRenderer.positionCount = 0;
        lineRenderer.material = (Material)Resources.Load("Unlit_Line", typeof(Material));
        lineRenderer.material.color = new Color(56f / 255f, 116f / 255f, 176f / 255f);
    }

    List<Action> eraseCommands = new List<Action>();
    public override void Draw()
    {
        Vector3[] vectors = new Vector3[waterPoints.Length];
        for (int i = 0; i < vectors.Length; i++)
            vectors[i] = terrainMessenger.SampleTerrainHeight(waterPoints[i]) + 0.1f * Vector3.up;

        foreach (Action command in eraseCommands) command.Invoke();

        lineRenderer.enabled = true;
        lineRenderer.startWidth = 0.5f;
        lineRenderer.endWidth = 0.5f;
        lineRenderer.positionCount = vectors.Length;
        lineRenderer.SetPositions(vectors);
    }
    public override void Erase() { lineRenderer.enabled = false; }
}
