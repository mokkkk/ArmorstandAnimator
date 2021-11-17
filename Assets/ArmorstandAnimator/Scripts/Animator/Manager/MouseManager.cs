using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace ArmorstandAnimator
{
    public enum MouseTarget
    {
        UI, Node, BG, None
    }
    public class MouseManager : MonoBehaviour
    {
        private MouseTarget mouseTarget;
        private SceneviewCamera sceneviewCamera;
        private float clickTime;
        private GameObject targetCube;

        private const float NodeClickTime = 0.2f;

        public void Initialize()
        {
            this.sceneviewCamera = Camera.main.gameObject.GetComponent<SceneviewCamera>();
            clickTime = 0.0f;
        }

        // Update is called once per frame
        public void Main()
        {
            if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1))
                OnClick();
            if (Input.GetMouseButtonUp(0) || Input.GetMouseButtonUp(1))
                OnRelease();

            if (mouseTarget == MouseTarget.Node)
            {
                clickTime += Time.deltaTime;
                if (clickTime > NodeClickTime)
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
            RaycastHit mouseHit;

            sceneviewCamera.GetMousePos();

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
            }
        }

        private void OnRelease()
        {
            if (this.mouseTarget == MouseTarget.Node && this.clickTime <= NodeClickTime)
            {
                Debug.Log("Click Node");
            }

            this.mouseTarget = MouseTarget.None;
            this.targetCube = null;
            this.clickTime = 0.0f;
        }
    }
}