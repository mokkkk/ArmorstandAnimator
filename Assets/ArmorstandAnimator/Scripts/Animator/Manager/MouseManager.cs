using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace ArmorstandAnimator
{
    public enum MouseTarget
    {
        UI, Node, BG, None, AxisT, AxisR
    }
    public class MouseManager : MonoBehaviour
    {
        private SceneManager sceneManager;

        private MouseTarget mouseTarget;
        [SerializeField]
        private SceneviewCamera sceneviewCamera;
        private float clickTime;
        private GameObject targetCube;

        private Axis targetAxis;
        private Vector2 posClick, rotateCenter, rotateClick;
        private float rotateCurrent;
        [SerializeField]
        private float mouseTranslatePosOffset;

        private const float NodeClickTime = 0.15f;
        private const float PosOffsetNone = 1.0f;
        private const float PosOffsetShift = 0.5f;
        private const float PosOffsetCtrl = 0.1f;
        private const float PosOffsetCtrlShift = 0.05f;
        private const float DegOffsetNone = 10.0f;
        private const float DegOffsetShift = 5.0f;
        private const float DegOffsetCtrl = 1.0f;
        private const float DegOffsetCtrlShift = 0.1f;

        public void Initialize()
        {
            this.sceneManager = this.gameObject.GetComponent<SceneManager>();
            mouseTarget = MouseTarget.None;
            clickTime = 0.0f;
        }

        // Update is called once per frame
        public void Main()
        {
            if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1))
                OnClick();
            if (Input.GetMouseButtonUp(0) || Input.GetMouseButtonUp(1))
                OnRelease();

            // 移動軸クリック時
            if (mouseTarget == MouseTarget.AxisT)
                CalcPos();
            // 回転軸クリック時
            if (mouseTarget == MouseTarget.AxisR)
                CalcRotate();

            if (mouseTarget != MouseTarget.None)
            {
                clickTime += Time.deltaTime;
                // ノードクリック時，一定時間以上
                if (mouseTarget == MouseTarget.Node && clickTime > NodeClickTime)
                    this.mouseTarget = MouseTarget.BG;
            }

            if (mouseTarget == MouseTarget.BG)
                sceneviewCamera.Main();

            sceneviewCamera.CheckMouseWheel();
        }

        private void OnClick()
        {
            if (EventSystem.current.IsPointerOverGameObject())
            {
                this.mouseTarget = MouseTarget.UI;
                return;
            }

            Ray mouseRay = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit axisHit, mouseHit;

            sceneviewCamera.GetMousePos();

            // 先に回転用UIとの衝突を計算
            int axisLayerMask = LayerMask.GetMask(new string[] { LayerMask.LayerToName(6) }); ;
            if (Input.GetMouseButtonDown(0) && Physics.Raycast(mouseRay, out axisHit, Mathf.Infinity, axisLayerMask))
            {
                // 移動用ハンドルクリック時
                if (axisHit.collider.gameObject.tag == "PositionAxis")
                {
                    this.mouseTarget = MouseTarget.AxisT;
                    this.targetAxis = axisHit.collider.gameObject.GetComponent<PositionLine>().axis;
                    Debug.Log(axisHit.collider.gameObject.GetComponent<PositionLine>().axis);
                    // クリック位置保存
                    posClick = Input.mousePosition;

                    // 移動用ハンドル表示切替
                    sceneManager.MouseNodePositionColor(targetAxis, false);
                }

                // 回転用ハンドルクリック時
                if (axisHit.collider.gameObject.tag == "RotateAxis")
                {
                    this.mouseTarget = MouseTarget.AxisR;
                    this.targetAxis = axisHit.collider.gameObject.GetComponent<RotateCircle>().axis;
                    Debug.Log(axisHit.collider.gameObject.GetComponent<RotateCircle>().axis);

                    // 中心位置保存
                    rotateCenter = RectTransformUtility.WorldToScreenPoint(Camera.main, axisHit.collider.transform.position);
                    // クリック位置保存
                    rotateClick = Input.mousePosition;
                    // 初期角度保存
                    if (targetAxis == Axis.X)
                        rotateCurrent = sceneManager.currentNode.rotate.x;
                    else if (targetAxis == Axis.Y)
                        rotateCurrent = sceneManager.currentNode.rotate.y;
                    else if (targetAxis == Axis.Z)
                        rotateCurrent = sceneManager.currentNode.rotate.z;

                    // 回転用ハンドル表示切替
                    sceneManager.MouseNodeRotationColor(targetAxis, false);
                }
            }

            if (mouseTarget == MouseTarget.None)
            {
                if (Physics.Raycast(mouseRay, out mouseHit))
                {
                    if (mouseHit.collider.gameObject.tag == "PivotCube")
                    {
                        this.mouseTarget = MouseTarget.Node;
                        this.targetCube = mouseHit.collider.gameObject;
                    }
                }
                else
                {
                    this.mouseTarget = MouseTarget.BG;
                    this.targetCube = null;
                }
            }
        }

        private void OnRelease()
        {
            // UIドラッグ：UI表示リセット
            if (this.mouseTarget == MouseTarget.AxisT)
                sceneManager.MouseNodePositionColor(targetAxis, true);
            if (this.mouseTarget == MouseTarget.AxisR)
                sceneManager.MouseNodeRotationColor(targetAxis, true);

            // クリック：ターゲットノード更新
            if (this.mouseTarget == MouseTarget.Node && this.clickTime <= NodeClickTime)
            {
                sceneManager.currentNode = targetCube.transform.parent.parent.parent.parent.parent.GetComponent<Node>();
            }
            else if (this.mouseTarget != MouseTarget.UI && this.clickTime <= NodeClickTime)
            {
                sceneManager.currentNode = null;
            }

            this.mouseTarget = MouseTarget.None;
            this.clickTime = 0.0f;
        }

        // マウス座標から位置を計算
        private void CalcPos()
        {
            Vector2 mousePos = Input.mousePosition;
            var currentPos = Vector3.zero;
            if (sceneManager.appMode == AppMode.Model)
                currentPos = sceneManager.currentNode.pos;
            else
                currentPos = sceneManager.GetRootPos();

            // 差計算
            var mag = 0.0f;
            float offset = 0.0f;
            if (targetAxis == Axis.X)
            {
                mag = posClick.x - mousePos.x;
                if (mag > 0)
                    offset = 1.0f;
                else
                    offset = -1.0f;
                // カメラ角度に応じたオフセット
                if (Camera.main.transform.eulerAngles.y < 90.0f || Camera.main.transform.eulerAngles.y > 270.0f)
                    offset *= -1;
            }
            if (targetAxis == Axis.Y)
            {
                mag = posClick.y - mousePos.y;
                if (mag < 0)
                    offset = 1.0f;
                else
                    offset = -1.0f;
            }
            if (targetAxis == Axis.Z)
            {
                mag = posClick.x - mousePos.x;
                if (mag > 0)
                    offset = 1.0f;
                else
                    offset = -1.0f;
                // カメラ角度に応じたオフセット
                if (Camera.main.transform.eulerAngles.y > 180.0f)
                    offset *= -1;
            }

            float posOffset = 0.0f;
            if (Mathf.Abs(mag) > mouseTranslatePosOffset)
            {
                if (Input.GetKey(KeyCode.LeftShift) && Input.GetKey(KeyCode.LeftControl))
                {
                    posOffset = PosOffsetCtrlShift * offset;
                }
                else if (Input.GetKey(KeyCode.LeftShift))
                {
                    posOffset = PosOffsetShift * offset;
                }
                else if (Input.GetKey(KeyCode.LeftControl))
                {
                    posOffset = PosOffsetCtrl * offset;
                }
                else
                {
                    posOffset = PosOffsetNone * offset;
                }

                if (targetAxis == Axis.X)
                    currentPos += Vector3.right * posOffset;
                if (targetAxis == Axis.Y)
                    currentPos += Vector3.up * posOffset;
                if (targetAxis == Axis.Z)
                    currentPos += Vector3.forward * posOffset;

                sceneManager.MouseNodePosition(currentPos);
                posClick = mousePos;
            }
        }

        // マウス座標から回転角度計算
        private void CalcRotate()
        {
            Vector2 start = (rotateClick - rotateCenter).normalized;
            Vector2 mousePos = Input.mousePosition;
            Vector2 current = (mousePos - rotateCenter).normalized;

            // 角度計算
            float deg = Vector2.Angle(start, current);
            var cross = current.x * start.y - start.x * current.y;
            if (cross < 0)
                deg *= -1;

            // カメラ角度に応じたオフセット
            if (targetAxis == Axis.X)
                if (Camera.main.transform.eulerAngles.y < 180.0f)
                    deg *= -1;
            if (targetAxis == Axis.Y)
                if (Camera.main.transform.eulerAngles.x > 270.0f)
                    deg *= -1;
            if (targetAxis == Axis.Z)
                if (Camera.main.transform.eulerAngles.y < 90.0f || Camera.main.transform.eulerAngles.y > 270.0f)
                    deg *= -1;
            Debug.Log(Camera.main.transform.eulerAngles.x);

            if (Input.GetKey(KeyCode.LeftShift) && Input.GetKey(KeyCode.LeftControl))
            {
                var p = deg % DegOffsetCtrlShift;
                deg -= p;
            }
            else if (Input.GetKey(KeyCode.LeftShift))
            {
                var p = deg % DegOffsetShift;
                deg -= p;
            }
            else if (Input.GetKey(KeyCode.LeftControl))
            {
                var p = deg % DegOffsetCtrl;
                deg -= p;
            }
            else
            {
                var p = deg % DegOffsetNone;
                deg -= p;
            }

            sceneManager.MouseNodeRotation(targetAxis, deg + rotateCurrent);
        }
    }
}