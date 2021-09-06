using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using SFB;

namespace ArmorstandAnimator
{
    public class NodeManager : MonoBehaviour
    {
        // SceneManager
        [SerializeField]
        private SceneManager sceneManager;
        [SerializeField]
        private Transform nodeHolder;

        // ノード作成用
        private CreateNodeObject createNodeObject;

        // ノードView UI描画用
        [SerializeField]
        private Transform nodeUIScrollview;
        [SerializeField]
        private GameObject nodeUIObj;

        // Jsonファイル読み込み用UI
        [SerializeField]
        private GameObject jsonFilePanel;
        // ファイルパス表示
        [SerializeField]
        private InputField pathInputField;
        // パーツ名表示
        [SerializeField]
        private InputField nodeNameInputField;
        // CustomModelData表示
        [SerializeField]
        private InputField customModelDataField;
        // ファイルパス
        private string[] paths;

        // 親ノード選択用UI
        [SerializeField]
        private GameObject parentNodeSettingPanel;
        // 対象ノードUI
        private NodeUI targetNodeUI;

        // モデル作成用
        [SerializeField]
        private GameObject nodeObject;
        [SerializeField]
        private GameObject pivotCube;

        private Transform elementHolder;

        private const float ScaleOffset = 16.0f;
        private const float HeadScaleOffset = 0.622568f;
        private const float PivotCenter = 8.0f;

        public void Initialize()
        {
            // モデル作成用
            createNodeObject = this.gameObject.AddComponent<CreateNodeObject>();
            createNodeObject.nodeObject = this.nodeObject;
            createNodeObject.pivotCube = this.pivotCube;
        }

        // jsonファイル読み込みUI表示切替
        public void SetJsonFilePanelVisible()
        {
            jsonFilePanel.SetActive(!jsonFilePanel.activeSelf);
            if (jsonFilePanel.activeSelf)
                customModelDataField.text = "0";
        }

        // jsonファイル選択
        public void SelectJsonFilePath()
        {
            // ファイルダイアログを開く
            var extensions = new[]
            {
    new ExtensionFilter( "Model Files", "json"),
};
            paths = StandaloneFileBrowser.OpenFilePanel("Open File", "", extensions, true);

            pathInputField.text = paths[0];
        }

        // Json読込UIからノード作成
        public void CreateNode()
        {
            var nodeName = nodeNameInputField.text;
            var customModelData = int.Parse(customModelDataField.text);

            // ID決定
            var id = sceneManager.NodeList.Count;

            // モデル作成
            var newNode = createNodeObject.GenerateJsonModel(paths[0], nodeName, id);

            // ノード初期化
            newNode.transform.parent = nodeHolder;
            newNode.Initialize(nodeName, customModelData, nodeUIObj, nodeUIScrollview);

            // SceneManagerのNodeListにノード追加
            sceneManager.NodeList.Add(newNode);

            // Json読込UI非表示
            SetJsonFilePanelVisible();
        }

        // プロジェクトファイルからノード作成
        public void CreateNodeProject(ASAModelNode[] nodeDataList)
        {
            // モデル作成
            foreach (ASAModelNode nodeData in nodeDataList)
            {
                // モデル作成
                var newNode = createNodeObject.GenerateJsonModelProject(nodeData);

                // ノード初期化
                newNode.transform.parent = nodeHolder;
                newNode.InitializeProject(nodeData, nodeUIObj, nodeUIScrollview);

                // SceneManagerのNodeListにノード追加
                sceneManager.NodeList.Add(newNode);
            }

            // 親ノード取得
            foreach (Node n in sceneManager.NodeList)
            {
                if (n.nodeType != NodeType.Root)
                    foreach (Node m in sceneManager.NodeList)
                        if (m.nodeID == n.parentNodeID)
                            n.SetParentNode(m);
            }
            CreateNodeTree();

            // ノード位置更新
            foreach (Node n in sceneManager.NodeList)
            {
                if (n.nodeType == NodeType.Root)
                    SetNodePosition(n);
            }
        }

        // 親ノードの選択
        public void SetParentNode(Node targetNode, NodeUI targetNodeUI)
        {
            // UI表示
            parentNodeSettingPanel.SetActive(true);
            // ターゲット設定
            var selectParentUI = parentNodeSettingPanel.GetComponent<SelectParentUI>();
            selectParentUI.targetNode = targetNode;
            selectParentUI.selectedNode = null;
            this.targetNodeUI = targetNodeUI;

            // ノード一覧表示
            foreach (Node n in sceneManager.NodeList)
            {
                // 対象ノード以外を表示
                if (n != targetNode)
                    selectParentUI.AddButton(n);
            }
        }

        // 親ノード決定
        public void DecideParentNode()
        {
            // 対象ノードの親ノードを決定
            var selectParentUI = parentNodeSettingPanel.GetComponent<SelectParentUI>();
            selectParentUI.targetNode.SetParentNode(selectParentUI.selectedNode);
            // targetNodeUI.OnParentNodeChanged(selectParentUI.selectedNode);
            // ノード親子関係整理
            CreateNodeTree();
            // ノード位置更新
            SetNodePosition(selectParentUI.targetNode);
            SetParentNodeSettingPanelVisible();
        }

        // 親ノード選択UI表示切替
        public void SetParentNodeSettingPanelVisible()
        {
            parentNodeSettingPanel.GetComponent<SelectParentUI>().targetNode = null;
            parentNodeSettingPanel.GetComponent<SelectParentUI>().DeleteUI();
            parentNodeSettingPanel.SetActive(!parentNodeSettingPanel.activeSelf);
        }

        // ノード位置決定
        public void SetNodePosition(Node targetNode)
        {
            // 親ノードの位置取得
            var parentPos = Vector3.zero;
            if (!ReferenceEquals(targetNode.parentNode, null))
                parentPos = targetNode.parentNode.transform.position;

            // 親ノードの回転取得
            var parentRotation = Vector3.zero;
            if (!ReferenceEquals(targetNode.parentNode, null))
                parentRotation = targetNode.parentNode.rotate;

            // 位置計算
            var rotatedPos = MatrixRotation.RotationWorld(MatrixRotation.RotationLocal(targetNode.pos, parentRotation), parentRotation);
            rotatedPos += parentPos;

            // 位置更新
            targetNode.transform.position = rotatedPos;

            // 自分の子ノードでSetNodePosition実行
            if (targetNode.childrenNode.Any())
                foreach (Node n in targetNode.childrenNode)
                    SetNodePosition(n);
        }

        // ノード親子関係整理
        void CreateNodeTree()
        {
            // NodeType，ChildrenNode初期化
            foreach (Node n in sceneManager.NodeList)
            {
                n.childrenNode = new List<Node>();
                n.nodeType = NodeType.Root;
            }

            // 親NodeのChildrenNodeに自分を追加
            foreach (Node n in sceneManager.NodeList)
            {
                if (!ReferenceEquals(n.parentNode, null))
                {
                    n.parentNode.childrenNode.Add(n);
                    n.nodeType = NodeType.Node;
                }
            }

            // LeafNode決定
            foreach (Node n in sceneManager.NodeList)
            {
                if (n.nodeType == NodeType.Node && !n.childrenNode.Any())
                    n.nodeType = NodeType.Leaf;
            }
        }

        // ノード作成(デバッグ用)
        public void CreateNode(string nodeName, int customModelData, string[] paths)
        {
            // ID決定
            var id = sceneManager.NodeList.Count;

            // モデル作成
            var newNode = createNodeObject.GenerateJsonModel(paths[0], nodeName, id);

            // ノード初期化
            newNode.transform.parent = nodeHolder;
            newNode.Initialize(nodeName, customModelData, nodeUIObj, nodeUIScrollview);

            // SceneManagerのNodeListにノード追加
            sceneManager.NodeList.Add(newNode);
        }
    }
}