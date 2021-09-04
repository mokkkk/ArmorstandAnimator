using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ArmorstandAnimator
{
    public class NodeUI : MonoBehaviour
    {
        public bool isOpen;

        // 操作対象ノード
        [SerializeField]
        private Node targetNode;

        // Component取得用
        private Toggle toggle;
        private GameObject content;

        private InputField positionX, positionY, positionZ;
        private InputField rotationX, rotationY, rotationZ;

        // NodeUIサイズ決定用
        private const float NodeUISizeClose = 25.0f;
        private const float NodeUISizeOpen = 225.0f;

        // 実装後削除する
        void Start()
        {
            // Component取得
            toggle = this.transform.Find("Tab").Find("Toggle").GetComponent<Toggle>();
            isOpen = toggle.isOn;
            content = this.transform.Find("Content").gameObject;

            positionX = content.transform.Find("Position").Find("X").GetComponent<InputField>();
            positionY = content.transform.Find("Position").Find("Y").GetComponent<InputField>();
            positionZ = content.transform.Find("Position").Find("Z").GetComponent<InputField>();
            rotationX = content.transform.Find("Rotation").Find("X").GetComponent<InputField>();
            rotationY = content.transform.Find("Rotation").Find("Y").GetComponent<InputField>();
            rotationZ = content.transform.Find("Rotation").Find("Z").GetComponent<InputField>();

            // 初期値代入
            positionX.text = positionY.text = positionZ.text = "0.0";
            rotationX.text = rotationY.text = rotationZ.text = "0.0";
        }

        // 初期化
        public void Initialize(Node targetNode)
        {
            // 操作対象ノード設定
            this.targetNode = targetNode;

            // Component取得
            toggle = this.transform.Find("Tab").Find("Toggle").GetComponent<Toggle>();
            isOpen = toggle.isOn;
            content = this.transform.Find("Content").gameObject;
            positionX = content.transform.Find("Position").Find("X").GetComponent<InputField>();
            positionY = content.transform.Find("Position").Find("Y").GetComponent<InputField>();
            positionZ = content.transform.Find("Position").Find("Z").GetComponent<InputField>();
            rotationX = content.transform.Find("Rotation").Find("X").GetComponent<InputField>();
            rotationY = content.transform.Find("Rotation").Find("X").GetComponent<InputField>();
            rotationZ = content.transform.Find("Rotation").Find("X").GetComponent<InputField>();

            // 初期値代入
            positionX.text = positionY.text = positionZ.text = "0.0";
            rotationX.text = rotationY.text = rotationZ.text = "0.0";

            // 値取得

            // サイズ調整
            this.GetComponent<RectTransform>().sizeDelta = new Vector2(this.GetComponent<RectTransform>().sizeDelta.x, NodeUISizeOpen);
            this.GetComponent<LayoutElement>().preferredHeight = NodeUISizeOpen;
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
                this.GetComponent<RectTransform>().sizeDelta = new Vector2(this.GetComponent<RectTransform>().sizeDelta.x, NodeUISizeOpen);
                this.GetComponent<LayoutElement>().preferredHeight = NodeUISizeOpen;
            }
            else
            {
                this.GetComponent<RectTransform>().sizeDelta = new Vector2(this.GetComponent<RectTransform>().sizeDelta.x, NodeUISizeClose);
                this.GetComponent<LayoutElement>().preferredHeight = NodeUISizeClose;
            }
        }
    }
}