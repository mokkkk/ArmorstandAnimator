using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using System.IO;
using SFB;

namespace ArmorstandAnimator
{
    // ProjectFile保存用
    [Serializable]
    public class ASAModelProject
    {
        public int fileVersion;
        public string itemID;
        public string modelName;
        public ASAModelNode[] nodeList;
        public bool multiEntities;
        public bool isMarker;
        public bool isSmall;
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
        public float[] rawTranslation;
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

    [Serializable]
    public class ASAPathHistory
    {
        public string[] paths;
    }

    public class ProjectFileManager : MonoBehaviour
    {
        // ProjectFile読込用パス
        private string path;
        private string[] paths;

        private const string PathHistoryFileName = "pathhist_project.json";

        // Modelプロジェクトファイル保存
        public void SaveProjectFileModel(GeneralSettingUI generalSetting, List<Node> nodeList)
        {
            // ファイルパス決定
            var extensionList = new[]
            {
    new ExtensionFilter( "Asa Model Project", "asamodel"),
};
            // path = StandaloneFileBrowser.SaveFilePanel("Save File", "", "project", extensionList);

            paths = StandaloneFileBrowser.OpenFilePanel("Save File", "", extensionList, false);
            path = paths[0];

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

                nodeArray[i].rawTranslation = new float[] { nodeList[i].rawTranslation.x, nodeList[i].rawTranslation.y, nodeList[i].rawTranslation.z };
            }
            project.nodeList = nodeArray;
            project.multiEntities = generalSetting.MultiEntities;
            project.isMarker = generalSetting.IsMarker;
            project.isSmall = generalSetting.IsSmall;

            // v0.9.5~
            project.fileVersion = 2;

            // プロジェクトファイル保存
            var projectFileJson = JsonUtility.ToJson(project);
            System.IO.StreamWriter writer = new System.IO.StreamWriter(path, false);
            writer.WriteLine(projectFileJson);
            writer.Flush();
            writer.Close();

            Debug.Log("ProjectFile Saved");

            // アクセス履歴保存
            SavePathHistory(path);
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
            paths = StandaloneFileBrowser.OpenFilePanel("Open File", "", extensions, false);

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

            if (ReferenceEquals(project.multiEntities, null))
                project.multiEntities = false;
            if (ReferenceEquals(project.isMarker, null))
                project.isMarker = true;
            if (ReferenceEquals(project.isSmall, null))
                project.isSmall = false;
            if (ReferenceEquals(project.fileVersion, null))
                project.fileVersion = 1;

            Debug.Log("ProjectFile Loaded");

            return 0;
        }

        public ASAModelProject SaveProjectFileModelReturn(GeneralSettingUI generalSetting, List<Node> nodeList)
        {
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

                nodeArray[i].rawTranslation = new float[] { nodeList[i].rawTranslation.x, nodeList[i].rawTranslation.y, nodeList[i].rawTranslation.z };
            }
            project.nodeList = nodeArray;
            project.multiEntities = generalSetting.MultiEntities;
            project.isMarker = generalSetting.IsMarker;
            project.isSmall = generalSetting.IsSmall;

            return project;
        }

        // アクセス履歴の一時保存
        private void SavePathHistory(string path)
        {
            var histPath = Path.Combine(Application.persistentDataPath, PathHistoryFileName);

            var jsonLine = new ASAPathHistory();
            if (File.Exists(histPath))
            {
                // ファイル読み込み
                string line;
                System.IO.StreamReader file =
                    new System.IO.StreamReader(histPath);
                line = file.ReadLine();
                jsonLine = JsonUtility.FromJson<ASAPathHistory>(line);
                file.Close();
            }
            else
            {
                jsonLine.paths = new string[0];
            }

            // リストに変換
            var pathHistories = new List<string>();
            for (int i = 0; i < jsonLine.paths.Length; i++)
            {
                pathHistories.Add(jsonLine.paths[i]);
            }

            // 被り探索
            bool exist = false;
            for (int i = 0; i < pathHistories.Count; i++)
            {
                var s = pathHistories[i];
                if (s.Equals(path))
                    exist = true;
                if (exist && i < pathHistories.Count - 1)
                    pathHistories[i] = pathHistories[i + 1];
                if (exist && i == pathHistories.Count - 1)
                    pathHistories[i] = path;
            }

            // パス追加
            if (!exist)
                pathHistories.Add(path);

            // パス削除
            if (pathHistories.Count > 10)
            {
                pathHistories.RemoveAt(0);
            }

            // ToJson
            var asaPathHistory = new ASAPathHistory();
            asaPathHistory.paths = pathHistories.ToArray();
            var p = JsonUtility.ToJson(asaPathHistory);

            System.IO.StreamWriter writer = new System.IO.StreamWriter(histPath);
            writer.WriteLine(p);
            writer.Flush();
            writer.Close();
        }

        public int LoadProjectFileModelCurrent(string path, out ASAModelProject project)
        {
            // ファイル読み込み
            string line;
            string inputString = "";
            // ファイルの改行削除，1行に纏める
            System.IO.StreamReader file =
                new System.IO.StreamReader(path);
            while ((line = file.ReadLine()) != null)
            {
                inputString += line.Replace("\r", "").Replace("\n", "");
            }
            file.Close();

            project = JsonUtility.FromJson<ASAModelProject>(inputString);

            if (ReferenceEquals(project.multiEntities, null))
                project.multiEntities = false;
            if (ReferenceEquals(project.isMarker, null))
                project.isMarker = true;
            if (ReferenceEquals(project.isSmall, null))
                project.isSmall = false;
            if (ReferenceEquals(project.fileVersion, null))
                project.fileVersion = 1;

            Debug.Log("ProjectFile Loaded");

            return 0;
        }
    }
}