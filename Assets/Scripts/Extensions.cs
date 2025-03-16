using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MOL;

public static class Vector3Extensions
{
    public static Vector3Int RoundToVector3Int(this Vector3 vector3)
    {
        int x = Mathf.RoundToInt(vector3.x);
        int y = Mathf.RoundToInt(vector3.y);
        int z = Mathf.RoundToInt(vector3.z);

        Vector3Int vector3Int = new Vector3Int(x, y, z);
        return vector3Int;
    }

    public static Matrix ToMatrix(this Vector3 vector3)
    {
        Matrix matrix = new Matrix(3, 1);
        matrix[0, 0] = vector3.x;
        matrix[1, 0] = vector3.y;
        matrix[2, 0] = vector3.z;

        return matrix;
    }

    public static Vector3 InverseScale(this Vector3 u, Vector3 v) { return Vector3.Scale(u, new Vector3(1 / v.x, 1 / v.y, 1 / v.z)); }
}

public static class MatrixExtensions
{
    public static void SetColumn(this Matrix matrix, int index, Vector3 column)
    {
        for (int i = 0; i < 3; i++)
            matrix[i, index] = column[i];
    }

    public static Vector3 ToVector3(this Matrix matrix)
    {
        return new Vector3((float)matrix[0, 0], (float)matrix[1, 0], (float)matrix[2, 0]);
    }

    public static Vector3 VectorMultiply(this Matrix matrix, Vector3 vector3)
    {
        return (matrix * vector3.ToMatrix()).ToVector3();
    }
}
