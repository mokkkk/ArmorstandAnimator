using System.Collections;
using System.IO;
using System.Collections.Generic;
using UnityEngine;

namespace ArmorstandAnimator
{
    public enum NodeType
    {
        Root, Node, Leaf
    }

    public class Node : MonoBehaviour
    {
        // NodeManager
        public NodeManager nodeManager;
        // 対応するUI
        public NodeUI targetNodeUI;

        // ID
        public int nodeID;

        // ノードの種類
        public NodeType nodeType;

        // ノード名
        public string nodeName;

        // CustomModelData
        public int customModelData;

        // 親ノード
        public Node parentNode;
        public int parentNodeID;
        // 子ノード
        public List<Node> childrenNode;

        // 位置・回転
        public Vector3 pos, rotate;

        // モデル回転用
        private Transform pose2, pose01;
        // モデルTransform
        public Transform element;

        // キューブ
        public List<Transform> elementCubes;

        // 初期化
        public void Initialize(string nodeName, int customModelData, GameObject nodeUIObj, Transform uiParent)
        {
            // Component取得
            pose2 = this.transform.Find("Pose2");
            pose01 = pose2.transform.Find("Pose01");
            element = pose01.transform.Find("Elements");
            foreach (Transform child in element)
                elementCubes.Add(child);
            this.nodeManager = GameObject.Find("SceneManager").GetComponent<NodeManager>();

            // ノード名設定
            this.gameObject.name = nodeName;

            // 値初期化
            this.nodeName = nodeName;
            this.customModelData = customModelData;
            this.nodeType = NodeType.Root;
            this.parentNode = null;
            this.childrenNode = new List<Node>();
            this.pos = this.rotate = Vector3.zero;

            // UI追加
            var newUI = Instantiate(nodeUIObj, Vector3.zero, Quaternion.identity, uiParent);
            newUI.GetComponent<NodeUI>().Initialize(this);
            this.targetNodeUI = newUI.GetComponent<NodeUI>();
        }

        // 初期化(プロジェクトファイル)
        public void InitializeProject(ASAModelNode nodeData, GameObject nodeUIObj, Transform uiParent)
        {
            // Component取得
            pose2 = this.transform.Find("Pose2");
            pose01 = pose2.transform.Find("Pose01");
            element = pose01.transform.Find("Elements");
            foreach (Transform child in element)
                elementCubes.Add(child);
            this.nodeManager = GameObject.Find("SceneManager").GetComponent<NodeManager>();

            // ノード名設定
            this.gameObject.name = nodeData.nodeName;

            // 値初期化
            this.nodeName = nodeData.nodeName;
            this.customModelData = nodeData.cmdID;
            this.nodeType = nodeData.nodeType;
            this.parentNode = null;
            this.childrenNode = new List<Node>();
            this.pos = new Vector3(nodeData.pos[0], nodeData.pos[1], nodeData.pos[2]);
            this.rotate = new Vector3(nodeData.rotate[0], nodeData.rotate[1], nodeData.rotate[2]);
            pose2.localEulerAngles = new Vector3(0.0f, 0.0f, rotate.z);
            pose01.localEulerAngles = new Vector3(rotate.x, rotate.y, 0.0f);
            this.parentNodeID = nodeData.parentNodeID;

            // UI追加
            var newUI = Instantiate(nodeUIObj, Vector3.zero, Quaternion.identity, uiParent);
            newUI.GetComponent<NodeUI>().Initialize(this);
            this.targetNodeUI = newUI.GetComponent<NodeUI>();
        }

        // pos変更
        public void SetPosition(Vector3 pos)
        {
            this.pos = pos;
            nodeManager.SetNodePosition(this);
        }

        // rotate変更
        public void SetRotation(Vector3 rotate)
        {
            this.rotate = rotate;
            pose2.localEulerAngles = new Vector3(0.0f, 0.0f, rotate.z);
            pose01.localEulerAngles = new Vector3(rotate.x, rotate.y, 0.0f);
            nodeManager.SetNodePosition(this);
        }

        // ParentNode変更
        public void SetParentNode(Node parentNode)
        {
            if (ReferenceEquals(parentNode, null))
            {
                this.parentNode = null;
                this.nodeType = NodeType.Root;
                targetNodeUI.OnParentNodeChanged("Root");
            }
            else
            {
                this.parentNode = parentNode;
                this.nodeType = NodeType.Node;
                targetNodeUI.OnParentNodeChanged(parentNode.nodeName);
            }
        }
        // Pos変更時
        // Rotate変更時

        // mcfuncの書き出し
        public int ExportMcfunction()
        {
            var fineName = Application.dataPath + "\\Test\\File\\test.mcfunction";
            List<string> func = new List<string>();

            func.Add("# Test");
            func.Add($"data modify entity @s rotate.Head[0] set value {rotate.x:F0}f");
            func.Add($"data modify entity @s rotate.Head[1] set value {rotate.y:F0}f");
            func.Add($"data modify entity @s rotate.Head[2] set value {rotate.z:F0}f");

            File.WriteAllLines(fineName, func);

            Debug.Log("Export Finished");
            return 0;
        }
    }
}