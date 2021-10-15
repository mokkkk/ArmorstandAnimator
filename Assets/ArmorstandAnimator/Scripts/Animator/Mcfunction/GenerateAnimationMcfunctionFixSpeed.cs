using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using SFB;

namespace ArmorstandAnimator
{
    public class GenerateAnimationMcfunctionFixSpeed : MonoBehaviour
    {
        // モデル用ファンクション
        [SerializeField]
        private GenerateModelMcfunc modelMcfunc;

        // モデル名
        private string modelName;
        // アニメーション名
        private string animationName;

        // 一般設定
        private GeneralSettingUI generalSetting;


        // 署名
        private const string FuncAuthor = "# This function was generated by ArmorstandAnimator";
        // データパックフォルダ名
        private const string DatapackFolderName = "asa_animator";
        // キーフレームフォルダ名
        private const string KeyframesFolderName = "keyframes";

        public void GenerateDatapack(GeneralSettingUI generalSetting, AnimationSettingUI animationSetting, List<Node> nodeList, List<Keyframe> keyframeList)
        {
            // ファイルパス決定
            var extensionList = new[]
            {
    new ExtensionFilter( "folder", "")
};
            var paths = StandaloneFileBrowser.OpenFolderPanel("Save File", "", false);
            // ファイルを選択しなかった場合，中断
            if (paths.Length < 1)
                return;
            var path = paths[0];

            // 設定取得
            this.generalSetting = generalSetting;

            // モデル名，アニメーション名取得
            modelName = generalSetting.ModelName;
            animationName = animationSetting.AnimationName;

            // データパックフォルダ作成
            path = Path.Combine(path, DatapackFolderName);
            Directory.CreateDirectory(path);
            var datapackPath = path;

            // ファンクションフォルダ作成
            path = Path.Combine(path, "functions");
            Directory.CreateDirectory(path);

            // モデル名フォルダ作成
            path = Path.Combine(path, generalSetting.ModelName.ToLower());
            Directory.CreateDirectory(path);

            // summon.mcfunction
            modelMcfunc.GenerateSummonFunction(path, generalSetting, nodeList, true, generalSetting.MultiEntities);
            // model.mcfunction   
            modelMcfunc.GenerateModelFunction(path, generalSetting, nodeList);
            // kill.mcfunction
            modelMcfunc.GenerateKillFunction(path, generalSetting);

            // アニメーション名分割
            string[] anmName = animationSetting.AnimationName.Split('/');
            // アニメーション名フォルダ作成
            if (anmName.Length < 2)
                path = Path.Combine(path, anmName[0]);
            else
                foreach (string s in anmName)
                {
                    path = Path.Combine(path, s);
                }
            Directory.CreateDirectory(path);

            // 再生速度変換済みキーフレーム
            int index = 0;
            var spdKeyframeList = new List<Keyframe>();
            foreach (Keyframe k in keyframeList)
            {
                var newKey = new Keyframe(index, Mathf.FloorToInt(k.tick / animationSetting.AnimationSpeed), k.rootPos, k.rotations);
                spdKeyframeList.Add(newKey);
            }

            // start.mcfunction
            GenerateStartFunction(path, spdKeyframeList[0], nodeList);
            // main.mcfunction
            GenerateMainFunction(path, spdKeyframeList);
            // end.mcfunction
            GenerateEndFunction(path);

            // Keyframesフォルダ作成
            path = Path.Combine(path, KeyframesFolderName);
            Directory.CreateDirectory(path);

            // index.mcfunction
            GenerateKeyframeFunction(path, spdKeyframeList, nodeList);

            Debug.Log("Animation Datapack Exported");

            // フォルダ表示
            System.Diagnostics.Process.Start(datapackPath);
        }

        public void GenerateDatapackOnlyAnimation(GeneralSettingUI generalSetting, AnimationSettingUI animationSetting, List<Node> nodeList, List<Keyframe> keyframeList)
        {
            // ファイルパス決定
            var extensionList = new[]
            {
    new ExtensionFilter( "folder", "")
};
            var paths = StandaloneFileBrowser.OpenFolderPanel("Save File", "", false);
            // ファイルを選択しなかった場合，中断
            if (paths.Length < 1)
                return;
            var path = paths[0];

            // モデル名，アニメーション名取得
            modelName = generalSetting.ModelName;
            animationName = animationSetting.AnimationName;

            // アニメーション名フォルダ作成
            path = Path.Combine(path, animationSetting.AnimationName.ToLower());
            Directory.CreateDirectory(path);

            // 再生速度変換済みキーフレーム
            int index = 0;
            var spdKeyframeList = new List<Keyframe>();
            foreach (Keyframe k in keyframeList)
            {
                var newKey = new Keyframe(index, Mathf.FloorToInt(k.tick / animationSetting.AnimationSpeed), k.rootPos, k.rotations);
                spdKeyframeList.Add(newKey);
            }

            // start.mcfunction
            GenerateStartFunction(path, spdKeyframeList[0], nodeList);
            // main.mcfunction
            GenerateMainFunction(path, spdKeyframeList);
            // end.mcfunction
            GenerateEndFunction(path);

            // Keyframesフォルダ作成
            path = Path.Combine(path, KeyframesFolderName);
            Directory.CreateDirectory(path);

            // index.mcfunction
            GenerateKeyframeFunction(path, spdKeyframeList, nodeList);

            Debug.Log("Animation Datapack Exported");
        }


        // start.mcfunction
        private void GenerateStartFunction(string path, Keyframe keyframe, List<Node> nodeList)
        {
            // ファイルパス決定
            path = Path.Combine(path, "start.mcfunction");
            // .mcfunction書き込み用
            var writer = new StreamWriter(path, false);

            // 各ノードのPose.Headのrotationsを設定
            int i = 0;
            foreach (Node n in nodeList)
            {
                // Root:rotationsのまま
                // Node,Leaf:親ノードのRotate参照
                Vector3 rotate = Vector3.zero;
                if (n.nodeType == NodeType.Root)
                {
                    rotate.x = keyframe.rotations[i].x;
                    rotate.y = keyframe.rotations[i].y;
                    rotate.z = keyframe.rotations[i].z;
                }
                else
                {
                    var end = false;
                    Vector3 parentRotate = Vector3.zero;
                    Node parent = n;
                    // 親ノードのRotateを合計
                    while (!end)
                    {
                        parent = parent.parentNode;
                        parentRotate += keyframe.rotations[parent.nodeID];
                        if (parent.nodeType == NodeType.Root)
                            end = true;
                    }
                    // 親ノードRotate + 自分のRotate
                    rotate.x = keyframe.rotations[i].x + parentRotate.x;
                    rotate.y = keyframe.rotations[i].y + parentRotate.y;
                    rotate.z = keyframe.rotations[i].z + parentRotate.z;
                }

                string selector = $"@e[type=armor_stand,tag={modelName}Parts,tag={n.nodeName}]";

                string func = "";
                if (generalSetting.MultiEntities)
                    func = $"execute as {selector} if score @s AsamID = #asa_id_temp AsamID run data merge entity @s {{Pose:{{Head:[{rotate.x}f,{rotate.y}f,{rotate.z}f]}}}}";
                else
                    func = $"execute as {selector} run data merge entity @s {{Pose:{{Head:[{rotate.x}f,{rotate.y}f,{rotate.z}f]}}}}";

                // string func = $"execute as {selector} run data merge entity @s {{Pose:{{Head:[{rotate.x}f,{rotate.y}f,{rotate.z}f]}}}}";
                writer.WriteLine(func);
                i++;
            }

            // 終了
            writer.Flush();
            writer.Close();

            Debug.Log("Generated start mcfunction");
        }

        // main.mcfunction
        private void GenerateMainFunction(string path, List<Keyframe> keyframeList)
        {
            // ファイルパス決定
            path = Path.Combine(path, "main.mcfunction");
            // .mcfunction書き込み用
            var writer = new StreamWriter(path, false);

            // Author
            writer.WriteLine(FuncAuthor);

            string func = "";
            // ID取得
            if (generalSetting.MultiEntities)
            {
                func = $"scoreboard players operation #asa_id_temp AsamID = @s AsamID";
                writer.WriteLine(func);
            }

            // Rootタイマー増加
            func = $"scoreboard players add @s AsaMatrix 1";
            writer.WriteLine(func);

            // start実行
            var execute = $"execute if score @s AsaMatrix matches 1 run ";
            func = $"function asa_animator:{modelName.ToLower()}/{animationName.ToLower()}/start";
            writer.WriteLine(execute + func);

            for (int i = 0; i < keyframeList.Count - 1; i++)
            {
                // 各Keyframeのfunction実行
                execute = $"execute if score @s AsaMatrix matches {keyframeList[i].tick + 1} run ";
                func = $"function asa_animator:{modelName.ToLower()}/{animationName.ToLower()}/{KeyframesFolderName}/{i}";
                writer.WriteLine(execute + func);

                // Root移動実行
                // アニメーション時間(tick)
                var time = keyframeList[i + 1].tick - keyframeList[i].tick;
                // 移動距離
                var moveX = -(keyframeList[i + 1].rootPos.x - keyframeList[i].rootPos.x) / time;
                var moveY = (keyframeList[i + 1].rootPos.y - keyframeList[i].rootPos.y) / time;
                var moveZ = (keyframeList[i + 1].rootPos.z - keyframeList[i].rootPos.z) / time;
                // 書き込み
                execute = $"execute if score @s AsaMatrix matches {keyframeList[i].tick + 1}..{keyframeList[i + 1].tick} run ";
                func = $"tp @s ^{moveX} ^{moveY} ^{moveZ}";
                writer.WriteLine(execute + func);
            }

            // end実行
            execute = $"execute if score @s AsaMatrix matches {keyframeList[keyframeList.Count - 1].tick + 1}.. run ";
            func = $"function asa_animator:{modelName.ToLower()}/{animationName.ToLower()}/end";
            writer.WriteLine(execute + func);

            // animate実行
            if (generalSetting.MultiEntities)
                execute = $"execute as @e[type=armor_stand,tag={modelName}Parts] if score @s AsamID = #asa_id_temp AsamID run ";
            else
                execute = $"execute as @e[type=armor_stand,tag={modelName}Parts] run ";
            func = $"function #asa_matrix:animate";
            writer.WriteLine(execute + func);

            // model実行
            func = $"function asa_animator:{modelName.ToLower()}/model";
            writer.WriteLine(func);

            // 終了
            writer.Flush();
            writer.Close();

            Debug.Log("Generated main mcfunction");
        }

        // end.mcfunction
        private void GenerateEndFunction(string path)
        {
            // ファイルパス決定
            path = Path.Combine(path, "end.mcfunction");
            // .mcfunction書き込み用
            var writer = new StreamWriter(path, false);

            // Rootのタイマーリセット
            var func = $"scoreboard players set @s AsaMatrix 0";
            writer.WriteLine(func);

            // 各ノードのArmorItems[3].tag.Rotateをリセット
            var selector = $"@e[type=armor_stand,tag={modelName}Parts]";
            if (generalSetting.MultiEntities)
                func = $"execute as {selector} if score @s AsamID = #asa_id_temp AsamID run function #asa_matrix:animate_reset";
            else
                func = $"execute as {selector} run function #asa_matrix:animate_reset";
            writer.WriteLine(func);

            // 終了
            writer.Flush();
            writer.Close();

            Debug.Log("Generated end mcfunction");
        }

        // keyframes/index.mcfunction
        private void GenerateKeyframeFunction(string path, List<Keyframe> keyframeList, List<Node> nodeList)
        {
            // ファイルパス
            string funcPath = "";
            // .mcfunction書き込み用
            StreamWriter writer;

            // 各ノードのArmorItems[3].tag.Rotateをkeyframeのrotationsに設定
            for (int i = 0; i < keyframeList.Count - 1; i++)
            {
                // パス決定
                funcPath = Path.Combine(path, $"{i}.mcfunction");
                writer = new StreamWriter(funcPath, false);

                // アニメーション時間(tick)
                var time = keyframeList[i + 1].tick - keyframeList[i].tick;

                int j = 0;
                foreach (Node n in nodeList)
                {
                    // 現キーフレームのrotate
                    Vector3 rotateCurrent = Vector3.zero;
                    if (n.nodeType == NodeType.Root)
                    {
                        rotateCurrent.x = keyframeList[i].rotations[j].x;
                        rotateCurrent.y = keyframeList[i].rotations[j].y;
                        rotateCurrent.z = keyframeList[i].rotations[j].z;
                    }
                    else
                    {
                        var end = false;
                        Vector3 parentRotate = Vector3.zero;
                        Node parent = n;
                        // 親ノードのRotateを合計
                        while (!end)
                        {
                            parent = parent.parentNode;
                            parentRotate += keyframeList[i].rotations[parent.nodeID];
                            if (parent.nodeType == NodeType.Root)
                                end = true;
                        }
                        // 親ノードRotate + 自分のRotate
                        rotateCurrent.x = keyframeList[i].rotations[j].x + parentRotate.x;
                        rotateCurrent.y = keyframeList[i].rotations[j].y + parentRotate.y;
                        rotateCurrent.z = keyframeList[i].rotations[j].z + parentRotate.z;
                    }

                    // 次キーフレームのrotate
                    Vector3 rotateNext = Vector3.zero;
                    if (n.nodeType == NodeType.Root)
                    {
                        rotateNext.x = keyframeList[i + 1].rotations[j].x;
                        rotateNext.y = keyframeList[i + 1].rotations[j].y;
                        rotateNext.z = keyframeList[i + 1].rotations[j].z;
                    }
                    else
                    {
                        var end = false;
                        Vector3 parentRotate = Vector3.zero;
                        Node parent = n;
                        // 親ノードのRotateを合計
                        while (!end)
                        {
                            parent = parent.parentNode;
                            parentRotate += keyframeList[i + 1].rotations[parent.nodeID];
                            if (parent.nodeType == NodeType.Root)
                                end = true;
                        }
                        // 親ノードRotate + 自分のRotate
                        rotateNext.x = keyframeList[i + 1].rotations[j].x + parentRotate.x;
                        rotateNext.y = keyframeList[i + 1].rotations[j].y + parentRotate.y;
                        rotateNext.z = keyframeList[i + 1].rotations[j].z + parentRotate.z;
                    }

                    // rotations取得
                    var rotateX = Mathf.FloorToInt((rotateNext.x - rotateCurrent.x) * 1000 / time);
                    var rotateY = Mathf.FloorToInt((rotateNext.y - rotateCurrent.y) * 1000 / time);
                    var rotateZ = Mathf.FloorToInt((rotateNext.z - rotateCurrent.z) * 1000 / time);
                    string func = $"data modify storage asa_matrix: Rotate set value [{rotateX}f,{rotateY}f,{rotateZ}f]";
                    writer.WriteLine(func);
                    // animate_setparam実行
                    string selector = $"@e[type=armor_stand,tag={modelName}Parts,tag={n.nodeName}]";
                    if (generalSetting.MultiEntities)
                        func = $"execute as {selector} if score @s AsamID = #asa_id_temp AsamID run function #asa_matrix:animate_setparam";
                    else
                        func = $"execute as {selector} run function #asa_matrix:animate_setparam";
                    writer.WriteLine(func);
                    j++;
                }

                // 終了
                writer.Flush();
                writer.Close();
            }

            Debug.Log("Generated keyframe mcfunction");
        }
    }
}