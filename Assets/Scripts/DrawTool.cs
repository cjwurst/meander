using System;
using UnityEngine;

public class DrawTool
{
    public static Action DrawLine(Vector3 start, Vector3 end, Color color)
    {
        GameObject line = new GameObject();
        line.transform.position = start;
        line.AddComponent<LineRenderer>();
        LineRenderer lineRenderer = line.GetComponent<LineRenderer>();
        Material material = (Material)Resources.Load("Unlit_Line", typeof(Material));
        lineRenderer.material = material;
        lineRenderer.material.color = color;
        lineRenderer.startWidth = 0.2f;
        lineRenderer.endWidth = 0.01f;
        lineRenderer.positionCount = 2;
        lineRenderer.SetPosition(0, start);
        lineRenderer.SetPosition(1, end);
        return () => GameObject.Destroy(line);
    }
}