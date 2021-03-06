using UnityEngine;
using UnityEngine.EventSystems;

namespace ArmorstandAnimator
{
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

        private float yAngleSum = 0.0f;

        [SerializeField]
        private SceneManager sceneManager;

        public void Main()
        {
            this.transform.position += target.transform.position - preTargetPos;
            preTargetPos = target.transform.position;

            MouseUpdate();
            return;
        }

        public void GetMousePos()
        {
            preMousePos = Input.mousePosition;
        }

        private void MouseUpdate()
        {
            MouseDrag(Input.mousePosition);
        }

        public void CheckMouseWheel()
        {
            float scrollWheel = Input.GetAxis("Mouse ScrollWheel");
            if (scrollWheel != 0.0f)
                MouseWheel(scrollWheel);
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

            yAngleSum += newAngle.y;
            var clampAngleSum = Mathf.Clamp(yAngleSum, -115.0f, 65.0f);
            var angleOffset = yAngleSum - clampAngleSum;
            yAngleSum = clampAngleSum;
            newAngle.y -= angleOffset;

            if (angleOffset > 0)
                Debug.Log("stop");

            this.transform.RotateAround(target.position, Vector3.up, newAngle.x);
            this.transform.RotateAround(target.position, transform.right, newAngle.y);
            preMousePos = Input.mousePosition;
        }
    }
}