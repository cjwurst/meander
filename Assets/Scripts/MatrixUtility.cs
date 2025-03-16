using UnityEngine;
using MOL;

public class MatrixUtility : MonoBehaviour
{
    // returns the change of basis matrix from E to a basis oriented along this along the x-axis
    public static Matrix GetChangeOfBasis(Vector3 start, Vector3 end)
    {
        //float tolerance = 0.001f;

        Vector3[] basis = new Vector3[3];

        basis[0] = end - start;
        basis[0].Normalize();

        if (basis[0].x != 0)
            basis[1] = new Vector3((-basis[0].y - basis[0].z) / basis[0].x, 1, 1);
        else
            basis[1] = new Vector3(1, 0, 0);
        basis[1].Normalize();

        basis[2] = Vector3.Cross(basis[0], basis[1]);

        Matrix changeOfBasis = new Matrix(3, 3);
        changeOfBasis.SetColumn(0, basis[0]);
        changeOfBasis.SetColumn(1, basis[1]);
        changeOfBasis.SetColumn(2, basis[2]);
        changeOfBasis = changeOfBasis.Transposition();          // This is a more performant way of getting the inverse than *Inverse()* since the 
                                                                // inverse of an orthonormal matrix is its transpose.
        return changeOfBasis;
    }

    public static Vector3 ToStandardBasis(Matrix changeOfBasis, Vector3 origin, Vector3 vector)
    {
        vector = changeOfBasis.Transposition().VectorMultiply(vector);
        vector += origin;
        return vector;
    }
    public static Vector3 ToCustomBasis(Matrix changeOfBasis, Vector3 origin, Vector3 vector)
    {
        vector -= origin;
        vector = changeOfBasis.VectorMultiply(vector);
        return vector;
    }
}
