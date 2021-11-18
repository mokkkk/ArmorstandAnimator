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
        private SceneviewCamera sceneviewCamera;
        private float clickTime;
        private GameObject targetCube;

        private Axis targetAxis;
        private Vector2 rotateCenter, rotateClick;
        private float rotateCurrent;

        private const float NodeClickTime = 0.15f;
        private const float DegOffsetNone = 5.0f;
        private const float DegOffsetShift = 1.0f;
        private const float DegOffsetCtrl = 0.5f;
        private const float DegOffsetCtrlShift = 0.1f;

        public void Initialize()
        {
            this.sceneManager = this.gameObject.GetComponent<SceneManager>();
            this.sceneviewCamera = Camera.main.gameObject.GetComponent<SceneviewCamera>();
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
                if (!ReferenceEquals(sceneManager.currentNode, null))
                {
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
            if (this.mouseTarget == MouseTarget.AxisR)
                sceneManager.MouseNodeRotationColor(targetAxis, true);

            // クリック：ターゲットノード更新
            if (this.mouseTarget == MouseTarget.Node && this.clickTime <= NodeClickTime)
            {
                sceneManager.currentNode = targetCube.transform.parent.parent.parent.parent.parent.GetComponent<Node>();
            }
            else if (this.clickTime <= NodeClickTime)
            {
                sceneManager.currentNode = null;
            }

            this.mouseTarget = MouseTarget.None;
            this.clickTime = 0.0f;
        }

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

            // Debug.Log(targetAxis + ":" + deg);
            sceneManager.MouseNodeRotation(targetAxis, deg + rotateCurrent);
        }
    }
}