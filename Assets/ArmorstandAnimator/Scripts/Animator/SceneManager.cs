using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ArmorstandAnimator
{
    public class SceneManager : MonoBehaviour
    {

        // 全パーツリスト
        [SerializeField]
        private List<Node> nodeList;

        // ノードView UI描画用
        [SerializeField]
        private Transform nodeUIScrollview;
        [SerializeField]
        private GameObject nodeUI;

        // jsonモデル作成用
        private JsonManager jsonManager;

        // Start is called before the first frame update
        void Start()
        {
            // Component取得
            jsonManager = this.gameObject.GetComponent<JsonManager>();

            // ノード追加
            // CreateNode();
            // CreateNode();
            // CreateNode();
            // CreateNode();
            // ReadModelJsonFile();
        }

        // Update is called once per frame
        void Update()
        {

        }

        // ノード追加
        public void CreateNode(string nodeName, string[] paths)
        {
            // モデル作成
            var nodeListCount = this.nodeList.Count;
            var newNode = jsonManager.GenerateJsonModel(nodeName, paths[0], nodeListCount);
            this.nodeList.Add(newNode);

            // UI追加
            var newUI = Instantiate(nodeUI, Vector3.zero, Quaternion.identity, nodeUIScrollview);
            newUI.GetComponent<NodeUI>().Initialize(newNode);
            newUI.transform.Find("Tab").Find("Toggle").Find("Label").GetComponent<Text>().text = nodeName;
        }
    }
}