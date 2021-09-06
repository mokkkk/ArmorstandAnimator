using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MatrixRotation
{
    public static Vector3 RotationLocal(Vector3 pos, Vector3 rotate)
    {
        var phi = Mathf.Deg2Rad * rotate.x;
        var theta = Mathf.Deg2Rad * rotate.y;

        float[,] matrix = new float[3, 3];

        matrix[0, 0] = Mathf.Cos(theta);
        matrix[0, 1] = Mathf.Sin(phi) * Mathf.Sin(theta);
        matrix[0, 2] = Mathf.Cos(phi) * Mathf.Sin(theta);

        matrix[1, 0] = 0.0f;
        matrix[1, 1] = Mathf.Cos(phi);
        matrix[1, 2] = -Mathf.Sin(phi);

        matrix[2, 0] = -Mathf.Sin(theta);
        matrix[2, 1] = Mathf.Sin(phi) * Mathf.Cos(theta);
        matrix[2, 2] = Mathf.Cos(phi) * Mathf.Cos(theta);

        var rotatedPos = new Vector3();
        rotatedPos.x = pos.x * matrix[0, 0] + pos.y * matrix[0, 1] + pos.z * matrix[0, 2];
        rotatedPos.y = pos.x * matrix[1, 0] + pos.y * matrix[1, 1] + pos.z * matrix[1, 2];
        rotatedPos.z = pos.x * matrix[2, 0] + pos.y * matrix[2, 1] + pos.z * matrix[2, 2];

        return rotatedPos;
    }

    public static Vector3 RotationWorld(Vector3 pos, Vector3 rotate)
    {
        var psi = Mathf.Deg2Rad * rotate.z;

        float[,] matrix = new float[3, 3];

        matrix[0, 0] = Mathf.Cos(psi);
        matrix[0, 1] = -Mathf.Sin(psi);
        matrix[0, 2] = 0.0f;

        matrix[1, 0] = Mathf.Sin(psi);
        matrix[1, 1] = Mathf.Cos(psi);
        matrix[1, 2] = 0.0f;

        matrix[2, 0] = 0.0f;
        matrix[2, 1] = 0.0f;
        matrix[2, 2] = 1.0f;

        var rotatedPos = new Vector3();
        rotatedPos.x = pos.x * matrix[0, 0] + pos.y * matrix[0, 1] + pos.z * matrix[0, 2];
        rotatedPos.y = pos.x * matrix[1, 0] + pos.y * matrix[1, 1] + pos.z * matrix[1, 2];
        rotatedPos.z = pos.x * matrix[2, 0] + pos.y * matrix[2, 1] + pos.z * matrix[2, 2];

        return rotatedPos;
    }
}
