using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// GameビューにてSceneビューのようなカメラの動きをマウス操作によって実現する
/// </summary>
[RequireComponent(typeof(Camera))]
public class SceneviewCamera : MonoBehaviour
{
    [SerializeField, Range(0.1f, 10f)]
    private float wheelSpeed = 1f;

    [SerializeField, Range(0.1f, 10f)]
    private float moveSpeed = 0.3f;

    [SerializeField, Range(0.1f, 10f)]
    private float rotateSpeed = 0.3f;

    [SerializeField]
    private Transform target;

    private Vector3 preMousePos, preTargetPos;

    private void Update()
    {
        // UI操作中はカメラ操作しない
        if (!EventSystem.current.IsPointerOverGameObject())
        {
            this.transform.position += target.transform.position - preTargetPos;
            preTargetPos = target.transform.position;

            MouseUpdate();
            return;
        }
    }

    private void MouseUpdate()
    {
        float scrollWheel = Input.GetAxis("Mouse ScrollWheel");
        if (scrollWheel != 0.0f)
            MouseWheel(scrollWheel);

        if (Input.GetMouseButtonDown(0) ||
           Input.GetMouseButtonDown(1) ||
           Input.GetMouseButtonDown(2))
            preMousePos = Input.mousePosition;

        MouseDrag(Input.mousePosition);
    }

    private void MouseWheel(float delta)
    {
        transform.position += transform.forward * delta * wheelSpeed;
        return;
    }

    private void MouseDrag(Vector3 mousePos)
    {
        Vector3 diff = mousePos - preMousePos;

        if (diff.magnitude < Vector3.kEpsilon)
            return;

        if (Input.GetMouseButton(1))
            CameraTranslate(diff * Time.deltaTime * moveSpeed);
        else if (Input.GetMouseButton(0))
            CameraRotate(new Vector2(-diff.y, diff.x) * rotateSpeed);

        preMousePos = mousePos;
    }

    private void CameraTranslate(Vector3 diff)
    {
        Transform trans = this.transform;

        // カメラのローカル座標軸を元に注視点オブジェクトを移動する
        target.Translate((trans.right * -diff.x) + (trans.up * -diff.y));
    }

    public void CameraRotate(Vector2 angle)
    {
        var x = (preMousePos.x - Input.mousePosition.x);
        var y = (Input.mousePosition.y - preMousePos.y);

        if (Mathf.Abs(x) < Mathf.Abs(y))
            x = 0;
        else
            y = 0;

        var newAngle = Vector3.zero;
        newAngle.x = x * -rotateSpeed;
        newAngle.y = y * -rotateSpeed;

        this.transform.RotateAround(target.position, Vector3.up, newAngle.x);
        this.transform.RotateAround(target.position, transform.right, newAngle.y);
        preMousePos = Input.mousePosition;
    }
}