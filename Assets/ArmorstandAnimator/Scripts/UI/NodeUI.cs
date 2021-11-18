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
            positionX.SetTextWithoutNotify(targetNode.pos.x.ToString());
            positionY.SetTextWithoutNotify(targetNode.pos.y.ToString());
            positionZ.SetTextWithoutNotify(targetNode.pos.z.ToString());
            rotationX.SetTextWithoutNotify(targetNode.rotate.x.ToString());
            rotationY.SetTextWithoutNotify(targetNode.rotate.y.ToString());
            rotationZ.SetTextWithoutNotify(targetNode.rotate.z.ToString());

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
            float x, y, z;
            var rx = float.TryParse(positionX.text, out x);
            var ry = float.TryParse(positionY.text, out y);
            var rz = float.TryParse(positionZ.text, out z);
            if (!rx || !ry || !rx)
                return;

            var pos = new Vector3(x, y, z);
            targetNode.SetPosition(pos);
            nodeManager.UpdateNodeTransform();
        }

        // Rotation変更時
        public void OnRotationChanged()
        {
            float x, y, z;
            var rx = float.TryParse(rotationX.text, out x);
            var ry = float.TryParse(rotationY.text, out y);
            var rz = float.TryParse(rotationZ.text, out z);
            if (!rx || !ry || !rx)
                return;

            var rotate = new Vector3(x, y, z);
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
            positionX.SetTextWithoutNotify(pos.x.ToString());
            positionY.SetTextWithoutNotify(pos.y.ToString());
            positionZ.SetTextWithoutNotify(pos.z.ToString());
        }

        // Rotation変更時(表示のみ)
        public void SetRotateText(Vector3 rotate)
        {
            rotationX.SetTextWithoutNotify(rotate.x.ToString());
            rotationY.SetTextWithoutNotify(rotate.y.ToString());
            rotationZ.SetTextWithoutNotify(rotate.z.ToString());
        }

        // 選択時
        public void OnNodeSelected()
        {
            nodeManager.OnNodeSelected(this.targetNode);
        }

        // Position変更
        public void UpdatePosition(Vector3 pos)
        {
            positionX.SetTextWithoutNotify(pos.x.ToString());
            positionY.SetTextWithoutNotify(pos.y.ToString());
            positionZ.SetTextWithoutNotify(pos.z.ToString());

            targetNode.SetPosition(pos);
            nodeManager.UpdateNodeTransform();
        }

        // Rotation変更
        public void UpdateRotate(Vector3 rotate)
        {
            rotationX.SetTextWithoutNotify(rotate.x.ToString());
            rotationY.SetTextWithoutNotify(rotate.y.ToString());
            rotationZ.SetTextWithoutNotify(rotate.z.ToString());

            targetNode.SetRotation(rotate);
            nodeManager.UpdateNodeTransform();
        }
    }
}