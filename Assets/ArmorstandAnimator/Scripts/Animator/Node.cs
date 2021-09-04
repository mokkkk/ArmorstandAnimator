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
        // ID
        public int nodeID;

        // ノードの種類
        public NodeType nodeType;

        // ノード名
        public string nodeName;

        // 親ノード
        public Node parentNode;
        // 子ノード
        public List<Node> childNode;

        // 位置・回転
        public Vector3 pos, rotate;

        // モデル回転用
        private Transform element, elementsub;

        // Start is called before the first frame update
        void Start()
        {
            element = this.transform.Find("Pose2");
            elementsub = element.transform.Find("Pose01");
        }

        // Update is called once per frame
        void Update()
        {
            SetRotation();
        }

        // rotate から回転角度設定
        void SetRotation()
        {
            element.localEulerAngles = new Vector3(0.0f, 0.0f, rotate.z);
            elementsub.localEulerAngles = new Vector3(rotate.x, rotate.y, 0.0f);
        }

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