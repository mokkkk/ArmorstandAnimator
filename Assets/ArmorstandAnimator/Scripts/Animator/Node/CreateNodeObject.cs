using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

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

    public class CreateNodeObject : MonoBehaviour
    {
        // NodeManager
        public NodeManager nodeManager;
        // モデル作成用
        public GameObject nodeObject, pivotCube;

        private const float ScaleOffset = 16.0f;
        private const float HeadScaleOffset = 0.622568f;
        private const float PivotCenter = 8.0f;

        // モデルJsonファイル読み込み
        public Node GenerateJsonModel(string path, string nodeName, int id)
        {
            // component, transform取得
            var newNode = Instantiate(nodeObject, Vector3.zero, Quaternion.identity);
            var node = newNode.GetComponent<Node>();
            node.nodeManager = nodeManager;
            var elementHolder = newNode.transform.Find("Pose2").Find("Pose01").Find("Elements");

            // ID設定
            node.nodeID = id;

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

            // デシリアライズ
            JsonModel inputJson = JsonUtility.FromJson<JsonModel>(inputString);

            // キューブ生成
            foreach (JsonElement element in inputJson.elements)
            {
                GenerateCube(element, elementHolder);
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

            return node;
        }

        // キューブ生成
        public void GenerateCube(JsonElement element, Transform elementHolder)
        {
            var cube = Instantiate(pivotCube, Vector3.zero, Quaternion.identity);
            cube.transform.parent = elementHolder;

            //  RightHand to LeftHand
            var from = new Vector3(element.from[0], element.from[1], element.to[2] + 2.0f * (PivotCenter - element.to[2]));
            var to = new Vector3(element.to[0], element.to[1], element.from[2] + 2.0f * (PivotCenter - element.from[2]));

            // Scale
            var scale = Vector3.zero;
            scale.x = to.x - from.x;
            scale.y = to.y - from.y;
            scale.z = to.z - from.z;
            if (scale.x == 0)
                scale.x = 0.001f;
            if (scale.y == 0)
                scale.y = 0.001f;
            if (scale.z == 0)
                scale.z = 0.001f;
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
                // transform childに引っかからないよう，一度親子関係を解除
                pivot.transform.parent = transform.root;
                Destroy(pivot);
            }
        }

        // プロジェクトファイルからモデル作成
        public Node GenerateJsonModelProject(ASAModelNode nodeData)
        {
            // component, transform取得
            var newNode = Instantiate(nodeObject, Vector3.zero, Quaternion.identity);
            var node = newNode.GetComponent<Node>();
            node.nodeManager = nodeManager;
            var elementHolder = newNode.transform.Find("Pose2").Find("Pose01").Find("Elements");

            // ID設定
            node.nodeID = nodeData.id;

            // キューブ生成
            foreach (ASAModelNodeElement element in nodeData.elements)
            {
                GenerateCubeProject(element, elementHolder);
            }

            // head.rotation
            elementHolder.localEulerAngles = new Vector3(nodeData.transform.rotation[0], nodeData.transform.rotation[1], nodeData.transform.rotation[2]);

            // head.translation
            elementHolder.localPosition = new Vector3(nodeData.transform.position[0], nodeData.transform.position[1], nodeData.transform.position[2]);

            // head.scale
            elementHolder.localScale = new Vector3(nodeData.transform.scale[0], nodeData.transform.scale[1], nodeData.transform.scale[2]);

            return node;
        }

        // プロジェクトファイルからキューブ生成
        public void GenerateCubeProject(ASAModelNodeElement element, Transform elementHolder)
        {
            var cube = Instantiate(pivotCube, Vector3.zero, Quaternion.identity);
            cube.transform.parent = elementHolder;

            // Scale
            cube.transform.localScale = new Vector3(element.scale[0], element.scale[1], element.scale[2]);

            // transform
            cube.transform.localPosition = new Vector3(element.position[0], element.position[1], element.position[2]);

            // rotation
            cube.transform.localEulerAngles = new Vector3(element.rotation[0], element.rotation[1], element.rotation[2]);
        }
    }
}