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
        public AnimationUI targetAnimationUI;

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
        public Transform pose2, pose01;
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
            CreateUIModel(nodeUIObj, uiParent);
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
            CreateUIModel(nodeUIObj, uiParent);
        }

        // 対応UI作成
        public void CreateUIModel(GameObject nodeUIObj, Transform uiParent)
        {
            var newUI = Instantiate(nodeUIObj, Vector3.zero, Quaternion.identity, uiParent);
            newUI.GetComponent<NodeUI>().Initialize(this);
            this.targetNodeUI = newUI.GetComponent<NodeUI>();
        }
        public void CreateUIAnim(GameObject animationUIObj, Transform uiParent)
        {
            var newUI = Instantiate(animationUIObj, Vector3.zero, Quaternion.identity, uiParent);
            newUI.GetComponent<AnimationUI>().Initialize(this);
            this.targetAnimationUI = newUI.GetComponent<AnimationUI>();
        }

        // pos変更
        public void SetPosition(Vector3 pos)
        {
            this.pos = pos;
            // nodeManager.SetNodePosition(this);
        }

        // rotate変更
        public void SetRotation(Vector3 rotate)
        {
            this.rotate = rotate;

            var rx = rotate.x;
            var ry = rotate.y;
            var rz = rotate.z;
            pose2.localEulerAngles = new Vector3(0.0f, 0.0f, rz);
            pose01.localEulerAngles = new Vector3(rx, ry, 0.0f);
        }

        // rotate変更
        public void SetRotation(Vector3 rotate, Vector3 parentRotate)
        {
            this.rotate = rotate;
            if (nodeType != NodeType.Root)
            {
                var rx = parentRotate.x + rotate.x;
                var ry = parentRotate.y + rotate.y;
                var rz = parentRotate.z + rotate.z;
                pose2.localEulerAngles = new Vector3(0.0f, 0.0f, rz);
                pose01.localEulerAngles = new Vector3(rx, ry, 0.0f);
            }
            else
            {
                var rx = rotate.x;
                var ry = rotate.y;
                var rz = rotate.z;
                pose2.localEulerAngles = new Vector3(0.0f, 0.0f, rz);
                pose01.localEulerAngles = new Vector3(rx, ry, 0.0f);
            }
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

        // Node消去
        public void RemoveNode()
        {
            nodeManager.RemoveNode(this);
        }

        // 防具立て表示設定
        public void SetArmorstandVisible(bool visible)
        {
            this.transform.Find("ArmorStand").gameObject.SetActive(visible);
        }

        // 回転軸表示設定
        public void SetAxisVisible(bool visible)
        {
            pose2.Find("Axis").gameObject.SetActive(visible);
            pose01.Find("Axis").gameObject.SetActive(visible);
        }

        // NodeName変更
        public void UpdateNodeName(string name)
        {
            this.nodeName = name;
            targetNodeUI.UpdateNodeName();
            elementCubes = new List<Transform>();
            foreach (Transform child in element)
                elementCubes.Add(child);
        }
    }
}