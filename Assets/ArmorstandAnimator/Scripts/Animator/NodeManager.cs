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

        // ノードモデル変更用
        private Node updateTargetNode;
        [SerializeField]
        private GameObject updateJsonFilePanel;
        [SerializeField]
        private InputField updatePathInputField;
        [SerializeField]
        private InputField updateNodeNameInputField;
        [SerializeField]
        private InputField updateCustomModelDataField;

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
            UpdateNodeTransform();
        }

        // ノードUIのみ表示
        public void CreateNodeUI()
        {
            foreach (Node n in sceneManager.NodeList)
            {
                n.CreateUIModel(nodeUIObj, nodeUIScrollview);
            }
        }

        // ノードUI消去
        public void ClearNodeUI()
        {
            foreach (Node n in sceneManager.NodeList)
            {
                Destroy(n.targetNodeUI.gameObject);
            }
        }

        // ノード消去
        public void RemoveNode(Node node)
        {
            // ノードリストから対応ノード消去
            sceneManager.RemoveNode(node);
            // 対象ノードを親にしているノードの親をRootに設定
            foreach (Node n in sceneManager.NodeList)
            {
                if (ReferenceEquals(n.parentNode, node))
                {
                    n.SetParentNode(null);
                }
            }
            // 親子関係整理
            CreateNodeTree();

            // ノードオブジェクト削除
            Destroy(node.targetNodeUI.gameObject);
            Destroy(node.gameObject);
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
            // ノード親子関係整理
            CreateNodeTree();
            // ノード位置，角度更新
            UpdateNodeTransform();
            SetParentNodeSettingPanelVisible();
        }

        // 親ノード選択UI表示切替
        public void SetParentNodeSettingPanelVisible()
        {
            parentNodeSettingPanel.GetComponent<SelectParentUI>().targetNode = null;
            parentNodeSettingPanel.GetComponent<SelectParentUI>().DeleteUI();
            parentNodeSettingPanel.SetActive(!parentNodeSettingPanel.activeSelf);
        }

        // ノード角度決定
        public void SetNodeRotation(Node targetNode)
        {
            // 角度計算
            targetNode.SetRotation(targetNode.rotate);

            // ノード位置更新
            UpdateNodeTransform();
        }

        // ノード位置決定
        public void SetNodePosition(Node targetNode, Vector3 parentRotate)
        {
            // 角度計算
            targetNode.SetRotation(targetNode.rotate, parentRotate);

            // 親ノードの位置取得
            var parentPos = Vector3.zero;
            if (!ReferenceEquals(targetNode.parentNode, null))
                parentPos = targetNode.parentNode.transform.position;

            // 位置計算
            var rotatedPos = MatrixRotation.RotationWorld(MatrixRotation.RotationLocal(targetNode.pos, parentRotate), parentRotate);
            rotatedPos += parentPos;

            // 位置更新
            targetNode.transform.position = rotatedPos;

            // 自分の子ノードでSetNodePosition実行
            if (targetNode.childrenNode.Any())
                foreach (Node n in targetNode.childrenNode)
                {
                    SetNodePosition(n, parentRotate + targetNode.rotate);
                }
        }

        // ノード位置更新
        public void UpdateNodeTransform()
        {
            // Rootノードからノード位置更新
            foreach (Node n in sceneManager.NodeList)
            {
                if (n.nodeType == NodeType.Root)
                    SetNodePosition(n, Vector3.zero);
            }
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


        // ノードのモデル更新
        public void StartUpdateNodeModel(Node targetNode)
        {
            this.updateTargetNode = targetNode;

            // Json読込UI表示
            SetJsonFilePanelVisibleUpdate();
        }

        //  ノードのモデル更新UI表示切替
        public void SetJsonFilePanelVisibleUpdate()
        {
            updateJsonFilePanel.SetActive(!updateJsonFilePanel.activeSelf);
            if (updateJsonFilePanel.activeSelf)
            {
                updateNodeNameInputField.text = updateTargetNode.nodeName.ToString();
                updateCustomModelDataField.text = updateTargetNode.customModelData.ToString();
            }
            else
            {
                this.updateTargetNode = null;
            }
        }

        // jsonファイル選択
        public void SelectJsonFilePathUpdate()
        {
            // ファイルダイアログを開く
            var extensions = new[]
            {
    new ExtensionFilter( "Model Files", "json"),
};
            paths = StandaloneFileBrowser.OpenFilePanel("Open File", "", extensions, true);

            updatePathInputField.text = paths[0];
        }

        // ノードのモデル更新
        public void UpdateNodeModel()
        {
            var nodeName = updateNodeNameInputField.text;
            var customModelData = int.Parse(updateCustomModelDataField.text);

            // モデル更新
            createNodeObject.UpdateJsonModel(paths[0], updateTargetNode);
            updateTargetNode.UpdateNodeName(nodeName);

            // Json読込UI非表示
            SetJsonFilePanelVisibleUpdate();

            // モデル位置更新
            UpdateNodeTransform();
        }
    }
}