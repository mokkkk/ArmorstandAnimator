using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ArmorstandAnimator
{
    public class AnimationUI : MonoBehaviour
    {
        public bool isOpen;

        // AnimationManager
        private AnimationManager animationManager;
        // KeyframeUI
        private KeyframeUI keyframeUI;

        // 操作対象ノード
        [SerializeField]
        private Node targetNode;

        // Component取得用
        private Text nodeName;
        private Toggle toggle;
        private GameObject content;
        private InputField rotationX, rotationY, rotationZ;

        // AnimationUIサイズ決定用
        private const float AnimationUISizeClose = 25.0f;
        private const float AnimationUISizeOpen = 100.0f;

        // 初期化
        public void Initialize(Node targetNode)
        {
            // 操作対象ノード設定
            this.targetNode = targetNode;

            // Component取得
            animationManager = GameObject.Find("SceneManager").GetComponent<AnimationManager>();
            keyframeUI = animationManager.keyframeUI;
            nodeName = this.transform.Find("Tab").Find("Toggle").Find("Label").GetComponent<Text>();
            toggle = this.transform.Find("Tab").Find("Toggle").GetComponent<Toggle>();
            isOpen = toggle.isOn;
            content = this.transform.Find("Content").gameObject;
            rotationX = content.transform.Find("Rotation").Find("X").GetComponent<InputField>();
            rotationY = content.transform.Find("Rotation").Find("Y").GetComponent<InputField>();
            rotationZ = content.transform.Find("Rotation").Find("Z").GetComponent<InputField>();

            // 値設定
            nodeName.text = targetNode.nodeName;
            rotationX.text = targetNode.rotate.x.ToString();
            rotationY.text = targetNode.rotate.y.ToString();
            rotationZ.text = targetNode.rotate.z.ToString();

            // サイズ調整
            this.GetComponent<RectTransform>().sizeDelta = new Vector2(this.GetComponent<RectTransform>().sizeDelta.x, AnimationUISizeOpen);
            this.GetComponent<LayoutElement>().preferredHeight = AnimationUISizeOpen;
        }

        // 表示切替時
        public void OnToggleChanged()
        {
            // パネル表示切替
            content.SetActive(toggle.isOn);
            isOpen = toggle.isOn;

            // サイズ調整
            if (toggle.isOn)
            {
                this.GetComponent<RectTransform>().sizeDelta = new Vector2(this.GetComponent<RectTransform>().sizeDelta.x, AnimationUISizeOpen);
                this.GetComponent<LayoutElement>().preferredHeight = AnimationUISizeOpen;
            }
            else
            {
                this.GetComponent<RectTransform>().sizeDelta = new Vector2(this.GetComponent<RectTransform>().sizeDelta.x, AnimationUISizeClose);
                this.GetComponent<LayoutElement>().preferredHeight = AnimationUISizeClose;
            }
        }

        // Rotate設定時
        public void OnRotateChanged()
        {
            keyframeUI.UpdateKeyframe();
        }

        // SetRotate
        public void SetRotate(Vector3 rotate)
        {
            this.rotationX.text = rotate.x.ToString();
            this.rotationY.text = rotate.y.ToString();
            this.rotationZ.text = rotate.z.ToString();
        }

        // GetRotate
        public Vector3 GetRotate()
        {
            var rotate = Vector3.zero;
            rotate.x = float.Parse(rotationX.text);
            rotate.y = float.Parse(rotationY.text);
            rotate.z = float.Parse(rotationZ.text);
            return rotate;
        }
    }
}