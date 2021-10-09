using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ArmorstandAnimator
{
    public class NodeUI : MonoBehaviour
    {
        public bool isOpen;

        // NodeManager
        private NodeManager nodeManager;

        // 操作対象ノード
        [SerializeField]
        private Node targetNode;

        // Component取得用
        private Text nodeName;
        private Toggle toggle;
        private GameObject content;
        private InputField customModelDataInputField;
        private InputField parentNodeInputField;
        private InputField positionX, positionY, positionZ;
        private InputField rotationX, rotationY, rotationZ;

        // NodeUIサイズ決定用
        private const float NodeUISizeClose = 25.0f;
        private const float NodeUISizeOpen = 275.0f;

        // 初期化
        public void Initialize(Node targetNode)
        {
            // 操作対象ノード設定
            this.targetNode = targetNode;

            // Component取得
            nodeManager = GameObject.Find("SceneManager").GetComponent<NodeManager>();
            nodeName = this.transform.Find("Tab").Find("Toggle").Find("Label").GetComponent<Text>();
            toggle = this.transform.Find("Tab").Find("Toggle").GetComponent<Toggle>();
            isOpen = toggle.isOn;
            content = this.transform.Find("Content").gameObject;
            customModelDataInputField = content.transform.Find("CustomModelData").Find("ID").GetComponent<InputField>();
            parentNodeInputField = content.transform.Find("Parent").Find("Name").GetComponent<InputField>();
            positionX = content.transform.Find("Position").Find("X").GetComponent<InputField>();
            positionY = content.transform.Find("Position").Find("Y").GetComponent<InputField>();
            positionZ = content.transform.Find("Position").Find("Z").GetComponent<InputField>();
            rotationX = content.transform.Find("Rotation").Find("X").GetComponent<InputField>();
            rotationY = content.transform.Find("Rotation").Find("Y").GetComponent<InputField>();
            rotationZ = content.transform.Find("Rotation").Find("Z").GetComponent<InputField>();

            // 値取得
            nodeName.text = targetNode.nodeName;
            customModelDataInputField.text = targetNode.customModelData.ToString();
            if (!ReferenceEquals(targetNode.parentNode, null))
                parentNodeInputField.text = targetNode.parentNode.nodeName;
            else
                parentNodeInputField.text = "Root";
            positionX.text = targetNode.pos.x.ToString();
            positionY.text = targetNode.pos.y.ToString();
            positionZ.text = targetNode.pos.z.ToString();
            rotationX.text = targetNode.rotate.x.ToString();
            rotationY.text = targetNode.rotate.y.ToString();
            rotationZ.text = targetNode.rotate.z.ToString();

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

        // CustomModelData変更時
        public void OnCmdChanged()
        {
            targetNode.SetCmd(int.Parse(customModelDataInputField.text));
        }

        // Position変更時
        public void OnPositionChanged()
        {
            var pos = new Vector3(float.Parse(positionX.text), float.Parse(positionY.text), float.Parse(positionZ.text));
            targetNode.SetPosition(pos);
            nodeManager.UpdateNodeTransform();
        }

        // Rotation変更時
        public void OnRotationChanged()
        {
            var rotate = new Vector3(float.Parse(rotationX.text), float.Parse(rotationY.text), float.Parse(rotationZ.text));
            targetNode.SetRotation(rotate);
            nodeManager.UpdateNodeTransform();
        }

        // 親ノード選択
        public void OnSelectParentClicked()
        {
            nodeManager.SetParentNode(targetNode, this);
        }

        // 親ノード決定時
        public void OnParentNodeChanged(string parendNodeName)
        {
            parentNodeInputField.text = parendNodeName;
        }

        // ノード消去
        public void OnRemoveNodeClicked()
        {
            targetNode.RemoveNode();
        }

        // ノード更新
        public void OnUpdateNodeClicked()
        {
            nodeManager.StartUpdateNodeModel(targetNode);
        }

        // ノード名更新
        public void UpdateNodeName()
        {
            this.nodeName.text = targetNode.nodeName;
        }

        // Position変更時(表示のみ)
        public void SetPositionText(Vector3 pos)
        {
            positionX.text = pos.x.ToString();
            positionY.text = pos.y.ToString();
            positionZ.text = pos.z.ToString();
        }
    }
}