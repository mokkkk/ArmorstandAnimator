using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using SFB;

namespace ArmorstandAnimator
{
    public class GenerateAnimationMcfunction : MonoBehaviour
    {
        // モデル用ファンクション
        [SerializeField]
        private GenerateModelMcfunc modelMcfunc;

        // モデル名
        private string modelName;
        // アニメーション名
        private string animationName;

        // for mcfunction
        private string anmSpeedDp, keyframeIndexDp, currentTickDp, endTickDp;


        // 署名
        private const string FuncAuthor = "# This function was generated by ArmorstandAnimator";
        // データパックフォルダ名
        private const string DatapackFolderName = "asa_animator";
        // キーフレームフォルダ名
        private const string KeyframesFolderName = "keyframes";
        private const string EventsFolderName = "events";

        public void GenerateDatapack(GeneralSettingUI generalSetting, AnimationSettingUI animationSetting, List<Node> nodeList, List<Keyframe> keyframeList, List<EventUI> eventList)
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

            // データパックフォルダ作成
            path = Path.Combine(path, DatapackFolderName);
            Directory.CreateDirectory(path);

            // ファンクションフォルダ作成
            path = Path.Combine(path, "functions");
            Directory.CreateDirectory(path);

            // モデル名フォルダ作成
            path = Path.Combine(path, modelName.ToLower());
            Directory.CreateDirectory(path);

            // summon.mcfunction
            modelMcfunc.GenerateSummonFunction(path, generalSetting, nodeList, false, generalSetting.MultiEntities);
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

            // ダミープレイヤー名取得
            anmSpeedDp = $"#asa_{modelName.ToLower()}_anmspeed";
            keyframeIndexDp = $"#asa_{modelName.ToLower()}_kindex";
            currentTickDp = $"#asa_{modelName.ToLower()}_tick_current";
            endTickDp = $"#asa_{modelName.ToLower()}_tick_end";

            // start.mcfunction
            GenerateStartFunction(path, keyframeList[0], nodeList);
            // main.mcfunction
            GenerateMainFunction(path, keyframeList);
            // end.mcfunction
            GenerateEndFunction(path);

            // Keyframesフォルダ作成
            var keyframePath = Path.Combine(path, KeyframesFolderName);
            Directory.CreateDirectory(keyframePath);

            // manager.mcfunction
            GenerateKeyframeManagerFunction(keyframePath, keyframeList);
            // index.mcfunction
            GenerateKeyframeFunction(keyframePath, keyframeList, nodeList);

            // Eventsフォルダ作成
            var eventPath = Path.Combine(path, EventsFolderName);
            Directory.CreateDirectory(eventPath);
            GenerateEventManagerFunction(eventPath, keyframeList, eventList);

            Debug.Log("Animation Datapack Exported");
        }

        // start.mcfunction
        private void GenerateStartFunction(string path, Keyframe keyframe, List<Node> nodeList)
        {
            // ファイルパス決定
            path = Path.Combine(path, "start.mcfunction");
            // .mcfunction書き込み用
            var writer = new StreamWriter(path, false);

            // currentTick初期化
            string func = $"scoreboard players set {currentTickDp} AsaMatrix 0";
            writer.WriteLine(func);

            // eventTick初期化
            func = $"function asa_animator:{modelName.ToLower()}/{animationName.ToLower()}/{EventsFolderName}/set_tick";
            writer.WriteLine(func);

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
                func = $"execute as {selector} run data merge entity @s {{Pose:{{Head:[{rotate.x}f,{rotate.y}f,{rotate.z}f]}}}}";
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

            // Rootタイマー増加
            string func = $"scoreboard players add {currentTickDp} AsaMatrix 1";
            writer.WriteLine(func);

            // Rootタイマー >= EndTick時， keyframe/manager実行
            func = $"execute if score {currentTickDp} AsaMatrix >= {endTickDp} AsaMatrix run function asa_animator:{modelName.ToLower()}/{animationName.ToLower()}/{KeyframesFolderName}/manager";
            writer.WriteLine(func);

            // event実行
            func = $"function asa_animator:{modelName.ToLower()}/{animationName.ToLower()}/{EventsFolderName}/manager";
            writer.WriteLine(func);

            // animate実行
            var execute = $"execute as @e[type=armor_stand,tag={modelName}Parts] run ";
            func = $"function #asa_matrix:animate";
            writer.WriteLine(execute + func);

            // move実行
            func = $"scoreboard players operation #asa_move_pos_x AsaMatrix = #asa_{modelName.ToLower()}_move_pos_x AsaMatrix";
            writer.WriteLine(func);
            func = $"scoreboard players operation #asa_move_pos_y AsaMatrix = #asa_{modelName.ToLower()}_move_pos_y AsaMatrix";
            writer.WriteLine(func);
            func = $"scoreboard players operation #asa_move_pos_z AsaMatrix = #asa_{modelName.ToLower()}_move_pos_z AsaMatrix";
            writer.WriteLine(func);
            func = $"function #asa_matrix:move";
            writer.WriteLine(func);

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

            // 値初期化
            string func = $"scoreboard players set {keyframeIndexDp} AsaMatrix 0";
            writer.WriteLine(func);
            func = $"scoreboard players set {currentTickDp} AsaMatrix 0";
            writer.WriteLine(func);
            func = $"scoreboard players set {endTickDp} AsaMatrix 0";
            writer.WriteLine(func);

            // 各ノードのArmorItems[3].tag.Rotateをリセット
            var selector = $"@e[type=armor_stand,tag={modelName}Parts]";
            func = $"execute as {selector} run function #asa_matrix:animate_reset";
            writer.WriteLine(func);

            // 終了
            writer.Flush();
            writer.Close();

            Debug.Log("Generated end mcfunction");
        }

        // keyframes/manager.mcfunction
        private void GenerateKeyframeManagerFunction(string path, List<Keyframe> keyframeList)
        {
            // ファイルパス
            string funcPath = "";
            // .mcfunction書き込み用
            StreamWriter writer;

            // パス決定
            funcPath = Path.Combine(path, $"manager.mcfunction");
            writer = new StreamWriter(funcPath, false);
            string func;

            for (int i = 0; i < keyframeList.Count - 1; i++)
            {
                // start実行
                if (i == 0)
                {
                    func = $"execute if score {keyframeIndexDp} AsaMatrix matches {i} run function asa_animator:{modelName.ToLower()}/{animationName.ToLower()}/start";
                    writer.WriteLine(func);
                }
                // 対応キーフレーム実行
                func = $"execute unless entity @s[tag=RotateChanged] if score {keyframeIndexDp} AsaMatrix matches {i} run function asa_animator:{modelName.ToLower()}/{animationName.ToLower()}/{KeyframesFolderName}/{i}";
                writer.WriteLine(func);
            }
            // End実行
            func = $"execute unless entity @s[tag=RotateChanged] if score {keyframeIndexDp} AsaMatrix matches {keyframeList.Count - 1}.. run function asa_animator:{modelName.ToLower()}/{animationName.ToLower()}/end";
            writer.WriteLine(func);

            // タイマーリセット
            func = $"scoreboard players set {currentTickDp} AsaMatrix 0";
            writer.WriteLine(func);

            // タグリセット
            func = $"tag @s remove RotateChanged";
            writer.WriteLine(func);

            // 終了
            writer.Flush();
            writer.Close();

            Debug.Log("Generated keyframe manager mcfunction");
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

                // タグ追加
                string func = $"tag @s add RotateChanged";
                writer.WriteLine(func);

                // indexインクリメント
                func = $"scoreboard players add {keyframeIndexDp} AsaMatrix 1";
                writer.WriteLine(func);

                // endTick計算
                func = $"scoreboard players set {endTickDp} AsaMatrix {time}";
                writer.WriteLine(func);
                func = $"scoreboard players operation {endTickDp} AsaMatrix *= #asam_const_1000 AsaMatrix";
                writer.WriteLine(func);
                func = $"scoreboard players operation {endTickDp} AsaMatrix /= {anmSpeedDp} AsaMatrix";
                writer.WriteLine(func);

                // Root移動設定
                // 移動距離
                var moveX = Mathf.FloorToInt(-(keyframeList[i + 1].rootPos.x - keyframeList[i].rootPos.x) * 1000 / time);
                var moveY = Mathf.FloorToInt((keyframeList[i + 1].rootPos.y - keyframeList[i].rootPos.y) * 1000 / time);
                var moveZ = Mathf.FloorToInt((keyframeList[i + 1].rootPos.z - keyframeList[i].rootPos.z) * 1000 / time);
                // 再生速度により変換
                func = $"scoreboard players set #asa_{modelName.ToLower()}_move_pos_x AsaMatrix {moveX}";
                writer.WriteLine(func);
                func = $"scoreboard players operation #asa_{modelName.ToLower()}_move_pos_x AsaMatrix *= {anmSpeedDp} AsaMatrix";
                writer.WriteLine(func);
                func = $"scoreboard players operation #asa_{modelName.ToLower()}_move_pos_x AsaMatrix /= #asam_const_1000 AsaMatrix";
                writer.WriteLine(func);

                func = $"scoreboard players set #asa_{modelName.ToLower()}_move_pos_y AsaMatrix {moveY}";
                writer.WriteLine(func);
                func = $"scoreboard players operation #asa_{modelName.ToLower()}_move_pos_y AsaMatrix *= {anmSpeedDp} AsaMatrix";
                writer.WriteLine(func);
                func = $"scoreboard players operation #asa_{modelName.ToLower()}_move_pos_y AsaMatrix /= #asam_const_1000 AsaMatrix";
                writer.WriteLine(func);

                func = $"scoreboard players set #asa_{modelName.ToLower()}_move_pos_z AsaMatrix {moveZ}";
                writer.WriteLine(func);
                func = $"scoreboard players operation #asa_{modelName.ToLower()}_move_pos_z AsaMatrix *= {anmSpeedDp} AsaMatrix";
                writer.WriteLine(func);
                func = $"scoreboard players operation #asa_{modelName.ToLower()}_move_pos_z AsaMatrix /= #asam_const_1000 AsaMatrix";
                writer.WriteLine(func);

                // storage用意
                func = "data merge storage asa_matrix: {Rotate:[0f,0f,0f]}";
                writer.WriteLine(func);

                // Rotate設定
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
                    // 再生速度により変換
                    func = $"scoreboard players set #asa_temp AsaMatrix {rotateX}";
                    writer.WriteLine(func);
                    func = $"scoreboard players operation #asa_temp AsaMatrix *= {anmSpeedDp} AsaMatrix";
                    writer.WriteLine(func);
                    func = $"scoreboard players operation #asa_temp AsaMatrix /= #asam_const_1000 AsaMatrix";
                    writer.WriteLine(func);
                    func = $"execute store result storage asa_matrix: Rotate[0] float 1 run scoreboard players get #asa_temp AsaMatrix";
                    writer.WriteLine(func);

                    func = $"scoreboard players set #asa_temp AsaMatrix {rotateY}";
                    writer.WriteLine(func);
                    func = $"scoreboard players operation #asa_temp AsaMatrix *= {anmSpeedDp} AsaMatrix";
                    writer.WriteLine(func);
                    func = $"scoreboard players operation #asa_temp AsaMatrix /= #asam_const_1000 AsaMatrix";
                    writer.WriteLine(func);
                    func = $"execute store result storage asa_matrix: Rotate[1] float 1 run scoreboard players get #asa_temp AsaMatrix";
                    writer.WriteLine(func);

                    func = $"scoreboard players set #asa_temp AsaMatrix {rotateZ}";
                    writer.WriteLine(func);
                    func = $"scoreboard players operation #asa_temp AsaMatrix *= {anmSpeedDp} AsaMatrix";
                    writer.WriteLine(func);
                    func = $"scoreboard players operation #asa_temp AsaMatrix /= #asam_const_1000 AsaMatrix";
                    writer.WriteLine(func);
                    func = $"execute store result storage asa_matrix: Rotate[2] float 1 run scoreboard players get #asa_temp AsaMatrix";
                    writer.WriteLine(func);
                    // animate_setparam実行
                    string selector = $"@e[type=armor_stand,tag={modelName}Parts,tag={n.nodeName}]";
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

        // events/manager.mcfunction
        private void GenerateEventManagerFunction(string path, List<Keyframe> keyframeList, List<EventUI> eventList)
        {
            // ファイルパス
            string funcPath = "";
            // .mcfunction書き込み用
            StreamWriter writer;

            // パス決定
            funcPath = Path.Combine(path, $"set_tick.mcfunction");
            writer = new StreamWriter(funcPath, false);
            string func;

            int index = 0;
            foreach (EventUI e in eventList)
            {
                int eventTick = e.Tick;
                int keyframeIndex = 0;

                // キーフレームindex探索
                for (int i = 0; i < keyframeList.Count - 1; i++)
                {
                    if (e.Tick >= keyframeList[i].tick)
                    {
                        keyframeIndex = keyframeList[i].index;
                    }
                }

                // tick
                eventTick -= keyframeList[keyframeIndex].tick;

                // 再生速度により変換
                func = $"scoreboard players set #asa_{modelName.ToLower()}_etick_{index} AsaMatrix {eventTick}";
                writer.WriteLine(func);
                func = $"scoreboard players operation #asa_{modelName.ToLower()}_etick_{index} AsaMatrix *= #asam_const_1000 AsaMatrix";
                writer.WriteLine(func);
                func = $"scoreboard players operation #asa_{modelName.ToLower()}_etick_{index} AsaMatrix /= {anmSpeedDp} AsaMatrix";
                writer.WriteLine(func);

                index++;
            }

            // 終了
            writer.Flush();
            writer.Close();


            // パス決定
            funcPath = Path.Combine(path, $"manager.mcfunction");
            writer = new StreamWriter(funcPath, false);

            index = 0;
            foreach (EventUI e in eventList)
            {
                int eventTick = e.Tick;
                string eventName = e.Name;
                int keyframeIndex = 0;

                // キーフレームindex探索
                for (int i = 0; i < keyframeList.Count - 1; i++)
                {
                    if (e.Tick >= keyframeList[i].tick)
                    {
                        keyframeIndex = keyframeList[i].index;
                    }
                }

                // tick
                eventTick -= keyframeList[keyframeIndex].tick;

                // 再生速度により変換
                func = $"# execute if score {keyframeIndexDp} AsaMatrix matches {keyframeIndex + 1} if score {currentTickDp} AsaMatrix = #asa_{modelName.ToLower()}_etick_{index} AsaMatrix run {eventName}";
                writer.WriteLine(func);

                index++;
            }

            // 終了
            writer.Flush();
            writer.Close();


            Debug.Log("Generated keyframe manager mcfunction");
        }

        /*public void GenerateDatapackOnlyAnimation(GeneralSettingUI generalSetting, AnimationSettingUI animationSetting, List<Node> nodeList, List<Keyframe> keyframeList)
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
            // GenerateStartFunction(path, spdKeyframeList[0], nodeList);
            // main.mcfunction
            // GenerateMainFunction(path, spdKeyframeList);
            // end.mcfunction
            // GenerateEndFunction(path);

            // Keyframesフォルダ作成
            // path = Path.Combine(path, KeyframesFolderName);
            // Directory.CreateDirectory(path);

            // index.mcfunction
            // GenerateKeyframeFunction(path, spdKeyframeList, nodeList);

            Debug.Log("Animation Datapack Exported");
        }*/
    }
}