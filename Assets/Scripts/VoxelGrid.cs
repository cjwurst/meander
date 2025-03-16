using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VoxelGrid
{
    public Vector3 cellSize = new Vector3(1, 1, 1);
    public Vector3 origin = Vector3.zero;

    public float maxCellDimension { get { return Mathf.Max(cellSize.x, cellSize.y, cellSize.z); } }

    public static Vector3Int[] neighborVectors
    {
        get
        {
            List<Vector3Int> _neighborVectors = new List<Vector3Int>();
            for (int i = -1; i <= 1; i++)
                for (int j = -1; j <= 1; j++)
                    for (int k = -1; k <= 1; k++)
                        _neighborVectors.Add(new Vector3Int(i, j, k));
            return _neighborVectors.ToArray();
        }
    }

    public VoxelGrid(Vector3 _cellSize) { cellSize = _cellSize; }
    public VoxelGrid(float _cellSize) { cellSize = new Vector3(_cellSize, _cellSize, _cellSize); }

    public Vector3Int WorldToCell(Vector3 worldPosition)
    {
        worldPosition -= origin;
        Vector3Int cellPosition = Vector3Int.RoundToInt(worldPosition.InverseScale(cellSize));
        return cellPosition;
    }

    public Vector3 CellToWorld(Vector3Int cellPosition)
    {
        Vector3 center = cellPosition + origin;
        center = Vector3.Scale(center, cellSize);
        return center;
    }
}
