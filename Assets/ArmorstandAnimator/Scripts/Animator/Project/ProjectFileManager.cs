using System.Collections;
using System.Collections.Generic;
using System;
// using System.IO;
using UnityEngine;
using SFB;

namespace ArmorstandAnimator
{
    // ProjectFile保存用
    [Serializable]
    public class ASAModelProject
    {
        public string itemID;
        public string modelName;
        public ASAModelNode[] nodeList;
    }
    [Serializable]
    public class ASAModelNode
    {
        public int id;
        public string nodeName;
        public int cmdID;
        public NodeType nodeType;
        public int parentNodeID;
        public float[] pos;
        public float[] rotate;
        public ASAModelNodeTransform transform;
        public ASAModelNodeElement[] elements;
    }
    [Serializable]
    public class ASAModelNodeTransform
    {
        public float[] position;
        public float[] rotation;
        public float[] scale;
    }
    [Serializable]
    public class ASAModelNodeElement
    {
        public float[] position;
        public float[] rotation;
        public float[] scale;
    }

    public class ProjectFileManager : MonoBehaviour
    {
        // ProjectFile読込用パス
        private string[] paths;

        // Modelプロジェクトファイル保存
        public void SaveProjectFileModel(GeneralSettingUI generalSetting, List<Node> nodeList)
        {
            // ファイルパス決定
            var extensionList = new[]
            {
    new ExtensionFilter( "Asa Model Project", "asamodel"),
};
            var path = StandaloneFileBrowser.SaveFilePanel("Save File", "", "project", extensionList);
            // ファイルを選択しなかった場合，中断
            if (path.Equals(""))
                return;

            // Jsonシリアライズ用インスタンスに値代入
            ASAModelProject project = new ASAModelProject();
            project.itemID = generalSetting.CmdItemID;
            project.modelName = generalSetting.ModelName;

            var nodeArray = new ASAModelNode[nodeList.Count];
            for (int i = 0; i < nodeArray.Length; i++)
            {
                nodeArray[i] = new ASAModelNode();
                nodeArray[i].id = nodeList[i].nodeID;
                nodeArray[i].nodeName = nodeList[i].nodeName;
                nodeArray[i].cmdID = nodeList[i].customModelData;
                nodeArray[i].nodeType = nodeList[i].nodeType;
                if (nodeList[i].nodeType != NodeType.Root)
                    nodeArray[i].parentNodeID = nodeList[i].parentNode.nodeID;
                else
                    nodeArray[i].parentNodeID = 0;
                nodeArray[i].pos = new float[] { nodeList[i].pos.x, nodeList[i].pos.y, nodeList[i].pos.z };
                nodeArray[i].rotate = new float[] { nodeList[i].rotate.x, nodeList[i].rotate.y, nodeList[i].rotate.z };

                nodeArray[i].transform = new ASAModelNodeTransform();
                nodeArray[i].transform.position = new float[] { nodeList[i].element.localPosition.x, nodeList[i].element.localPosition.y, nodeList[i].element.localPosition.z };
                nodeArray[i].transform.rotation = new float[] { nodeList[i].element.localEulerAngles.x, nodeList[i].element.localEulerAngles.y, nodeList[i].element.localEulerAngles.z };
                nodeArray[i].transform.scale = new float[] { nodeList[i].element.localScale.x, nodeList[i].element.localScale.y, nodeList[i].element.localScale.z };

                nodeArray[i].elements = new ASAModelNodeElement[nodeList[i].elementCubes.Count];
                for (int j = 0; j < nodeArray[i].elements.Length; j++)
                {
                    nodeArray[i].elements[j] = new ASAModelNodeElement();
                    nodeArray[i].elements[j].position = new float[] { nodeList[i].elementCubes[j].localPosition.x, nodeList[i].elementCubes[j].localPosition.y, nodeList[i].elementCubes[j].localPosition.z };
                    nodeArray[i].elements[j].rotation = new float[] { nodeList[i].elementCubes[j].localEulerAngles.x, nodeList[i].elementCubes[j].localEulerAngles.y, nodeList[i].elementCubes[j].localEulerAngles.z };
                    nodeArray[i].elements[j].scale = new float[] { nodeList[i].elementCubes[j].localScale.x, nodeList[i].elementCubes[j].localScale.y, nodeList[i].elementCubes[j].localScale.z };
                }
            }
            project.nodeList = nodeArray;
            // プロジェクトファイル保存
            var projectFileJson = JsonUtility.ToJson(project);
            System.IO.StreamWriter writer = new System.IO.StreamWriter(path, false);
            writer.WriteLine(projectFileJson);
            writer.Flush();
            writer.Close();

            Debug.Log("ProjectFile Saved");
        }

        // jsonファイル選択
        public void SelectJsonFilePath()
        {
            // ファイルダイアログを開く
            var extensions = new[]
            {
    new ExtensionFilter( "Model Files", "asamodel"),
};
            paths = StandaloneFileBrowser.OpenFilePanel("Open File", "", extensions, true);

            // pathInputField.text = paths[0];
        }

        public int LoadProjectFileModel(out ASAModelProject project)
        {
            // // 初期化
            // paths = new string[];
            project = new ASAModelProject();

            // ファイルダイアログを開く
            var extensions = new[]
            {
    new ExtensionFilter( "Model Files", "asamodel"),
};
            paths = StandaloneFileBrowser.OpenFilePanel("Open File", "", extensions, true);

            // ファイルを選択しなかった場合，中断
            if (paths.Length < 1)
                return -1;

            // ファイル読み込み
            string line;
            string inputString = "";
            // ファイルの改行削除，1行に纏める
            System.IO.StreamReader file =
                new System.IO.StreamReader(paths[0]);
            while ((line = file.ReadLine()) != null)
            {
                inputString += line.Replace("\r", "").Replace("\n", "");
            }
            file.Close();

            project = JsonUtility.FromJson<ASAModelProject>(inputString);

            Debug.Log("ProjectFile Loaded");

            return 0;
        }
    }
}