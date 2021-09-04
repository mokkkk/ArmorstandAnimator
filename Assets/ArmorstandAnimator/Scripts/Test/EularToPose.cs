using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EularToPose : MonoBehaviour
{
    [SerializeField]
    private Vector3 rotation;

    private Transform child;
    private Transform childVec;

    [SerializeField]
    private Transform sub;

    private int index;

    // Start is called before the first frame update
    void Start()
    {
        child = this.transform.GetChild(0);
        childVec = child.Find("Vec");
    }

    // Update is called once per frame
    void Update()
    {
        this.transform.eulerAngles = new Vector3(0.0f, 0.0f, rotation.z);
        child.transform.localEulerAngles = new Vector3(rotation.x, rotation.y, 0.0f);

        if (Input.GetKeyDown(KeyCode.Z))
            RotationLine();
    }

    // 回転変換
    private void RotationLine()
    {
        // var phi = Mathf.Deg2Rad * rotation.x;
        // var theta = Mathf.Deg2Rad * rotation.y;
        // var psi = Mathf.Deg2Rad * rotation.z;

        // float[,] matrix = new float[3, 3];

        // matrix[0, 0] = Mathf.Cos(theta) * Mathf.Cos(psi);
        // matrix[0, 1] = Mathf.Sin(phi) * Mathf.Sin(theta) * Mathf.Cos(psi) - Mathf.Cos(phi) * Mathf.Sin(psi);
        // matrix[0, 2] = Mathf.Cos(phi) * Mathf.Sin(theta) * Mathf.Cos(psi) + Mathf.Sin(phi) * Mathf.Sin(psi);

        // matrix[1, 0] = Mathf.Cos(theta) * Mathf.Sin(psi);
        // matrix[1, 1] = Mathf.Sin(phi) * Mathf.Sin(theta) * Mathf.Sin(psi) + Mathf.Cos(phi) * Mathf.Cos(psi);
        // matrix[1, 2] = Mathf.Cos(phi) * Mathf.Sin(theta) * Mathf.Sin(psi) - Mathf.Sin(phi) * Mathf.Cos(psi);

        // matrix[2, 0] = -Mathf.Sin(theta);
        // matrix[2, 1] = Mathf.Sin(phi) * Mathf.Cos(theta);
        // matrix[2, 2] = Mathf.Cos(phi) * Mathf.Cos(theta);

        // Vector3 rotatedForward = new Vector3(1, 1, 1);

        // rotatedForward.x = Mathf.Atan2(matrix[2, 1], matrix[2, 2]) * Mathf.Rad2Deg;
        // rotatedForward.y = Mathf.Asin(-matrix[2, 0]) * Mathf.Rad2Deg;
        // rotatedForward.z = Mathf.Atan2(matrix[1, 0], matrix[0, 0]) * Mathf.Rad2Deg;

        // sub.rotation = Quaternion.Euler(rotatedForward);
        // Debug.Log(rotatedForward);

        var vec = new Vector3(1, 1, 1);
        vec = RotationLocal(vec);
        vec = RotationWorld(vec);
        Debug.Log("ChildVec : " + childVec.position);
        Debug.Log("RotatedVec : " + vec);
        sub.GetChild(0).GetComponent<LineRenderer>().SetPosition(1, vec + sub.position);
    }

    private Vector3 RotationLocal(Vector3 vec)
    {
        var phi = Mathf.Deg2Rad * rotation.x;
        var theta = Mathf.Deg2Rad * rotation.y;

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

        var rotatedVec = new Vector3();
        rotatedVec.x = vec.x * matrix[0, 0] + vec.y * matrix[0, 1] + vec.z * matrix[0, 2];
        rotatedVec.y = vec.x * matrix[1, 0] + vec.y * matrix[1, 1] + vec.z * matrix[1, 2];
        rotatedVec.z = vec.x * matrix[2, 0] + vec.y * matrix[2, 1] + vec.z * matrix[2, 2];

        return rotatedVec;
    }

    private Vector3 RotationWorld(Vector3 vec)
    {
        var psi = Mathf.Deg2Rad * rotation.z;

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

        var rotatedVec = new Vector3();
        rotatedVec.x = vec.x * matrix[0, 0] + vec.y * matrix[0, 1] + vec.z * matrix[0, 2];
        rotatedVec.y = vec.x * matrix[1, 0] + vec.y * matrix[1, 1] + vec.z * matrix[1, 2];
        rotatedVec.z = vec.x * matrix[2, 0] + vec.y * matrix[2, 1] + vec.z * matrix[2, 2];

        return rotatedVec;
    }
}