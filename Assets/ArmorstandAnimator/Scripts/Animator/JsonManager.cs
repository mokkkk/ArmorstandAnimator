using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
using SFB;

namespace ArmorstandAnimator
{
    // Json読み込み用
    [Serializable]
    public class JsonModel
    {
        public JsonElement[] elements;
        public JsonDisplay display;
    }
    [Serializable]
    public class JsonElement
    {
        public float[] from;
        public float[] to;
        public JsonRotation rotation;
    }
    [Serializable]
    public class JsonRotation
    {
        public float angle;
        public string axis;
        public float[] origin;
    }
    [Serializable]
    public class JsonDisplay
    {
        public JsonHeadDisplay head;
    }
    [Serializable]
    public class JsonHeadDisplay
    {
        public float[] translation;
        public float[] rotation;
        public float[] scale;
    }

    public class JsonManager : MonoBehaviour
    {
        // SceneManager
        [SerializeField]
        private SceneManager sceneManager;

        // Jsonファイル読み込み用UI
        [SerializeField]
        private GameObject jsonFilePanel;
        // ファイルパス表示
        [SerializeField]
        private InputField pathInputField;
        // パーツ名表示
        [SerializeField]
        private InputField nodeNameInputField;

        // ファイルパス
        private string[] paths;

        // モデル作成用
        [SerializeField]
        private GameObject nodeObject;
        [SerializeField]
        private GameObject pivotCube;

        private Transform elementHolder;

        private const float ScaleOffset = 16.0f;
        private const float HeadScaleOffset = 0.622568f;
        private const float PivotCenter = 8.0f;

        // jsonファイル読み込みUI表示切替
        public void SetJsonFilePanelVisible()
        {
            jsonFilePanel.SetActive(!jsonFilePanel.activeSelf);
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

        // jsonモデル作成
        public void CreateNode()
        {
            sceneManager.CreateNode(nodeNameInputField.text, paths);
            SetJsonFilePanelVisible();
        }

        // jsonファイルからモデルを作る
        public Node GenerateJsonModel(string nodeName, string path, int nodeListCount)
        {
            // component, transform取得
            var newNode = Instantiate(nodeObject, Vector3.zero, Quaternion.identity);
            var node = newNode.GetComponent<Node>();
            elementHolder = newNode.transform.Find("Pose2").Find("Pose01").Find("Elements");

            // ノード名設定
            node.nodeName = nodeName;
            newNode.name = node.nodeName;

            // ID設定
            node.nodeID = nodeListCount;

            // Json読み込み
            ReadJson(path);

            return node;
        }

        // モデルJsonファイル読み込み
        private void ReadJson(string path)
        {
            // ファイル読み込み
            string line;
            string inputString = "";

            // Read the file and display it line by line.  
            System.IO.StreamReader file =
                new System.IO.StreamReader(path);
            while ((line = file.ReadLine()) != null)
            {
                inputString += line.Replace("\r", "").Replace("\n", "");
            }

            file.Close();

            // ファイル読み込み
            // string inputString = Resources.Load<TextAsset>("test").ToString();
            // デシリアライズ
            JsonModel inputJson = JsonUtility.FromJson<JsonModel>(inputString);

            // キューブ生成
            foreach (JsonElement element in inputJson.elements)
            {
                CreateCube(element);
            }

            // Scale Offset
            elementHolder.localScale /= ScaleOffset;
            elementHolder.localScale *= HeadScaleOffset;

            // head.rotation
            if (inputJson.display.head.rotation != null)
            {
                elementHolder.localRotation = Quaternion.Euler(new Vector3(-inputJson.display.head.rotation[0], -inputJson.display.head.rotation[1], inputJson.display.head.rotation[2]));
            }

            // head.translation
            if (inputJson.display.head.translation != null)
            {
                var headTranslation = new Vector3(inputJson.display.head.translation[0] / ScaleOffset * HeadScaleOffset, inputJson.display.head.translation[1] / ScaleOffset * HeadScaleOffset, -inputJson.display.head.translation[2] / ScaleOffset * HeadScaleOffset);
                elementHolder.localPosition += headTranslation;
            }

            // head.scale
            if (inputJson.display.head.scale != null)
            {
                var headScale = new Vector3(elementHolder.localScale.x * inputJson.display.head.scale[0], elementHolder.localScale.y * inputJson.display.head.scale[1], elementHolder.localScale.z * inputJson.display.head.scale[2]);
                elementHolder.localScale = headScale;
            }

            Debug.Log("Cube Created");
        }

        // キューブ生成
        private void CreateCube(JsonElement element)
        {
            var cube = Instantiate(pivotCube, Vector3.zero, Quaternion.identity);
            cube.transform.parent = elementHolder;

            //  RightHand to LeftHand
            var from = new Vector3(element.from[0], element.from[1], element.to[2] + 2.0f * (PivotCenter - element.to[2]));
            var to = new Vector3(element.to[0], element.to[1], element.from[2] + 2.0f * (PivotCenter - element.from[2]));

            Debug.Log("from : " + from + "        to : " + to);

            // Scale
            var scale = Vector3.zero;
            scale.x = to.x - from.x;
            scale.y = to.y - from.y;
            scale.z = to.z - from.z;
            cube.transform.localScale = scale;

            // transform
            var pos = new Vector3(from.x, from.y, from.z);
            pos.x -= PivotCenter;
            pos.y -= PivotCenter;
            pos.z -= PivotCenter;
            cube.transform.localPosition = pos;

            // rotation
            if (element.rotation.axis != null)
            {
                // create pivot
                var pivot = new GameObject();
                pivot.transform.parent = elementHolder;
                var pivotPos = Vector3.zero;
                pivotPos.x = element.rotation.origin[0] - PivotCenter;
                pivotPos.y = element.rotation.origin[1] - PivotCenter;
                pivotPos.z = element.rotation.origin[2] + 2.0f * (PivotCenter - element.rotation.origin[2]) - PivotCenter;
                pivot.transform.localPosition = pivotPos;

                // rotate
                cube.transform.parent = pivot.transform;
                switch (element.rotation.axis)
                {
                    case "x":
                        pivot.transform.localRotation = Quaternion.Euler(new Vector3(-element.rotation.angle, 0.0f, 0.0f));
                        break;
                    case "y":
                        pivot.transform.localRotation = Quaternion.Euler(new Vector3(0.0f, -element.rotation.angle, 0.0f));
                        break;
                    case "z":
                        pivot.transform.localRotation = Quaternion.Euler(new Vector3(0.0f, 0.0f, element.rotation.angle));
                        break;
                }
                cube.transform.parent = elementHolder;
                Destroy(pivot);
            }
        }
    }
}