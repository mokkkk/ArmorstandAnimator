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
        // private const float HeadScaleOffset = 0.6225681f;
        private const float HeadScaleOffset = 0.625f;
        private const float HeadScaleOffsetSmall = 0.4635f;
        private const float TranslationOffset = 14.0f;
        /* Small Armorstand : 
        Transform Offset ： 0 1 0
        Height Offset : ~ ~0.7 ~
        Object Transform Position ： 0 -0.575 0
        */
        private const float PivotCenter = 8.0f;
        private const float DefaultLocalPositionY = 0.5f;

        // モデルJsonファイル読み込み
        public Node GenerateJsonModel(string path, string nodeName, int id, bool isSmall)
        {
            // component, transform取得
            var newNode = Instantiate(nodeObject, Vector3.zero, Quaternion.identity);
            var node = newNode.GetComponent<Node>();
            node.nodeManager = nodeManager;
            var elementHolder = newNode.transform.Find("Pose2").Find("Pose01").Find("Elements");
            var armorstandNormal = newNode.transform.Find("ArmorStand");
            var armorstandSmall = newNode.transform.Find("ArmorStandSmall");

            // ID設定
            node.nodeID = id;

            // 初期値設定
            if (!isSmall)
                elementHolder.localPosition = Vector3.up * 16.0f * 0.0390625f / 2.0f;
            else
                elementHolder.localPosition = Vector3.up * 16.0f * 0.02896875f / 2.0f;
            node.rawTranslation = Vector3.zero;

            // ファイル読み込み
            string line;
            string inputString = "";
            // ファイルの改行削除，1行に纏める
            System.IO.StreamReader file = new System.IO.StreamReader(path);
            while ((line = file.ReadLine()) != null)
            {
                inputString += line.Replace("\r", "").Replace("\n", "");
            }
            file.Close();

            // デシリアライズ
            JsonModel inputJson = new JsonModel();
            inputJson = JsonUtility.FromJson<JsonModel>(inputString);

            // キューブ生成
            foreach (JsonElement element in inputJson.elements)
            {
                GenerateCube(element, elementHolder);
            }

            // Scale Offset
            elementHolder.localScale /= ScaleOffset;
            if (!isSmall)
            {
                elementHolder.localScale *= HeadScaleOffset;
                armorstandSmall.gameObject.SetActive(false);
            }
            else
            {
                elementHolder.localScale *= HeadScaleOffsetSmall;
                armorstandNormal.gameObject.SetActive(false);
            }

            // head
            if (!ReferenceEquals(inputJson.display.head, null))
            {
                // head.rotation
                if (inputJson.display.head.rotation != null)
                {
                    elementHolder.localEulerAngles = new Vector3(-inputJson.display.head.rotation[0], -inputJson.display.head.rotation[1], inputJson.display.head.rotation[2]);
                }

                // head.translation
                if (inputJson.display.head.translation != null)
                {
                    node.rawTranslation = new Vector3(inputJson.display.head.translation[0], inputJson.display.head.translation[1], inputJson.display.head.translation[2]);
                    var headTranslation = Vector3.zero;
                    if (!isSmall)
                        headTranslation = new Vector3(inputJson.display.head.translation[0] / ScaleOffset * HeadScaleOffset, inputJson.display.head.translation[1] / ScaleOffset * HeadScaleOffset, -inputJson.display.head.translation[2] / ScaleOffset * HeadScaleOffset);
                    else
                        headTranslation = new Vector3(inputJson.display.head.translation[0] / ScaleOffset * HeadScaleOffsetSmall, inputJson.display.head.translation[1] / ScaleOffset * HeadScaleOffsetSmall, -inputJson.display.head.translation[2] / ScaleOffset * HeadScaleOffsetSmall);
                    elementHolder.localPosition += headTranslation;
                }

                // head.scale
                if (inputJson.display.head.scale != null)
                {
                    var headScale = new Vector3(elementHolder.localScale.x * inputJson.display.head.scale[0], elementHolder.localScale.y * inputJson.display.head.scale[1], elementHolder.localScale.z * inputJson.display.head.scale[2]);
                    elementHolder.localScale = headScale;
                }
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
        public Node GenerateJsonModelProject(ASAModelNode nodeData, bool isSmall)
        {
            // component, transform取得
            var newNode = Instantiate(nodeObject, Vector3.zero, Quaternion.identity);
            var node = newNode.GetComponent<Node>();
            node.nodeManager = nodeManager;
            var elementHolder = newNode.transform.Find("Pose2").Find("Pose01").Find("Elements");
            // RawTranslation取得
            if (!ReferenceEquals(nodeData.rawTranslation, null))
                node.rawTranslation = new Vector3(nodeData.rawTranslation[0], nodeData.rawTranslation[1], nodeData.rawTranslation[2]);
            else
                node.rawTranslation = Vector3.zero;

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
            if (ReferenceEquals(nodeData.rawTranslation, null))
                elementHolder.localPosition = new Vector3(nodeData.transform.position[0], nodeData.transform.position[1], nodeData.transform.position[2]);
            else
                elementHolder.localPosition = HeadTranslation(node.rawTranslation, isSmall);

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

        // ノード更新
        public void UpdateJsonModel(string path, Node targetNode, bool isSmall)
        {
            // component, transform取得
            var elementHolder = targetNode.transform.Find("Pose2").Find("Pose01").Find("Elements");
            // モデル削除
            var tList = new List<Transform>();
            foreach (Transform t in elementHolder)
            {
                tList.Add(t);
            }
            foreach (Transform t in tList)
            {
                t.parent = transform.root;
                Destroy(t.gameObject);
            }

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
            JsonModel inputJson = new JsonModel();
            inputJson = JsonUtility.FromJson<JsonModel>(inputString);

            // Transform初期化
            targetNode.transform.position = Vector3.zero;
            targetNode.transform.rotation = Quaternion.identity;
            targetNode.pose2.localRotation = Quaternion.identity;
            targetNode.pose01.localRotation = Quaternion.identity;
            elementHolder.localRotation = Quaternion.identity;
            elementHolder.localPosition = new Vector3(0.0f, DefaultLocalPositionY, 0.0f);
            elementHolder.localScale = Vector3.one;

            // キューブ生成
            foreach (JsonElement element in inputJson.elements)
            {
                GenerateCube(element, elementHolder);
            }

            // Scale Offset
            elementHolder.localScale /= ScaleOffset;
            if (!isSmall)
            {
                elementHolder.localScale *= HeadScaleOffset;
            }
            else
            {
                elementHolder.localScale *= HeadScaleOffsetSmall;
            }

            // 初期値設定
            if (!isSmall)
                elementHolder.localPosition = Vector3.up * 16.0f * 0.0390625f / 2.0f;
            else
                elementHolder.localPosition = Vector3.up * 16.0f * 0.02896875f / 2.0f;

            // head
            if (!ReferenceEquals(inputJson.display.head, null))
            {
                // head.rotation
                if (inputJson.display.head.rotation != null)
                {
                    elementHolder.localEulerAngles = new Vector3(-inputJson.display.head.rotation[0], -inputJson.display.head.rotation[1], inputJson.display.head.rotation[2]);
                }

                // head.translation
                if (inputJson.display.head.translation != null)
                {
                    targetNode.rawTranslation = new Vector3(inputJson.display.head.translation[0], inputJson.display.head.translation[1], inputJson.display.head.translation[2]);
                    var headTranslation = Vector3.zero;
                    if (!isSmall)
                        headTranslation = new Vector3(inputJson.display.head.translation[0] / ScaleOffset * HeadScaleOffset, inputJson.display.head.translation[1] / ScaleOffset * HeadScaleOffset, -inputJson.display.head.translation[2] / ScaleOffset * HeadScaleOffset);
                    else
                        headTranslation = new Vector3(inputJson.display.head.translation[0] / ScaleOffset * HeadScaleOffsetSmall, inputJson.display.head.translation[1] / ScaleOffset * HeadScaleOffsetSmall, -inputJson.display.head.translation[2] / ScaleOffset * HeadScaleOffsetSmall);
                    elementHolder.localPosition += headTranslation;
                }

                // head.scale
                if (inputJson.display.head.scale != null)
                {
                    var headScale = new Vector3(elementHolder.localScale.x * inputJson.display.head.scale[0], elementHolder.localScale.y * inputJson.display.head.scale[1], elementHolder.localScale.z * inputJson.display.head.scale[2]);
                    elementHolder.localScale = headScale;
                }
            }
        }

        // Small切替
        public Node ChangeNodeSize(ASAModelNode nodeData, bool isSmall)
        {
            // component, transform取得
            var newNode = Instantiate(nodeObject, Vector3.zero, Quaternion.identity);
            var node = newNode.GetComponent<Node>();
            node.nodeManager = nodeManager;
            var elementHolder = newNode.transform.Find("Pose2").Find("Pose01").Find("Elements");

            // ID設定
            node.nodeID = nodeData.id;

            // 初期値設定
            if (!isSmall)
                elementHolder.localPosition = Vector3.up * 16.0f * 0.0390625f / 2.0f;
            else
                elementHolder.localPosition = Vector3.up * 16.0f * 0.02896875f / 2.0f;

            // キューブ生成
            foreach (ASAModelNodeElement element in nodeData.elements)
            {
                GenerateCubeProject(element, elementHolder);
            }

            // head.rotation
            elementHolder.localEulerAngles = new Vector3(nodeData.transform.rotation[0], nodeData.transform.rotation[1], nodeData.transform.rotation[2]);

            // head.translation
            var headTranslation = Vector3.zero;
            node.rawTranslation = new Vector3(nodeData.rawTranslation[0], nodeData.rawTranslation[1], nodeData.rawTranslation[2]);
            if (!isSmall)
                headTranslation = new Vector3(nodeData.rawTranslation[0] / ScaleOffset * HeadScaleOffset, nodeData.rawTranslation[1] / ScaleOffset * HeadScaleOffset, -nodeData.rawTranslation[2] / ScaleOffset * HeadScaleOffset);
            else
                headTranslation = new Vector3(nodeData.rawTranslation[0] / ScaleOffset * HeadScaleOffsetSmall, nodeData.rawTranslation[1] / ScaleOffset * HeadScaleOffsetSmall, -nodeData.rawTranslation[2] / ScaleOffset * HeadScaleOffsetSmall);
            elementHolder.localPosition += headTranslation;

            // head.scale
            elementHolder.localScale = new Vector3(nodeData.transform.scale[0], nodeData.transform.scale[1], nodeData.transform.scale[2]);

            // Scale Offset
            if (!isSmall)
                elementHolder.localScale /= HeadScaleOffsetSmall;
            else
                elementHolder.localScale /= HeadScaleOffset;
            elementHolder.localScale *= ScaleOffset;

            elementHolder.localScale /= ScaleOffset;
            if (!isSmall)
                elementHolder.localScale *= HeadScaleOffset;
            else
                elementHolder.localScale *= HeadScaleOffsetSmall;

            return node;
        }

        // head
        private Vector3 HeadTranslation(Vector3 rawTranslation, bool isSmall)
        {
            var defaultTranslation = Vector3.zero;
            if (!isSmall)
                defaultTranslation = Vector3.up * 16.0f * 0.0390625f / 2.0f;
            else
                defaultTranslation = Vector3.up * 16.0f * 0.02896875f / 2.0f;

            var headTranslation = Vector3.zero;

            if (!isSmall)
                headTranslation = new Vector3(rawTranslation.x / ScaleOffset * HeadScaleOffset, rawTranslation.y / ScaleOffset * HeadScaleOffset, -rawTranslation.z / ScaleOffset * HeadScaleOffset);
            else
                headTranslation = new Vector3(rawTranslation.x / ScaleOffset * HeadScaleOffsetSmall, rawTranslation.y / ScaleOffset * HeadScaleOffsetSmall, -rawTranslation.z / ScaleOffset * HeadScaleOffsetSmall);

            defaultTranslation += headTranslation;

            return defaultTranslation;
        }
    }
}