using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using SFB;

namespace ArmorstandAnimator
{
    public class GenerateModelMcfunc : MonoBehaviour
    {
        private GeneralSettingUI generalSetting;
        private System.IO.StreamWriter writer;

        private const float ScoreboardMultiple = 1000.0f;

        private const string FuncAuthor = "# This function was generated by ArmorstandAnimator";

        private const string FuncGetMatrixRoot = "function #asa_matrix:matrix";
        private const string FuncGetMatrixNode = "function #asa_matrix:matrix_node";
        private const string FuncGetParentPos = "function #asa_matrix:get_parent_pos";
        private const string FuncGetChildPos = "function #asa_matrix:rotate";
        private const string FuncSetChildPos = "function #asa_matrix:set_child_pos";

        // Summon.mcfunction
        public void GenerateSummonFunction(GeneralSettingUI generalSetting, List<Node> nodeList)
        {
            // ファイルパス決定
            var extensionList = new[]
            {
    new ExtensionFilter( "summon.mcfunction", "mcfunction")
};
            var path = StandaloneFileBrowser.SaveFilePanel("Save File", "", generalSetting.ModelName.ToLower() + "_summon", extensionList);
            // ファイルを選択しなかった場合，中断
            if (path.Equals(""))
                return;

            this.generalSetting = generalSetting;

            // .mcfunction書き込み用
            writer = new System.IO.StreamWriter(path, false);

            // 値初期化
            var anmSpeedDp = $"#asa_{generalSetting.ModelName.ToLower()}_anmspeed";
            string func = $"scoreboard players set {anmSpeedDp} AsaMatrix 1000";
            writer.WriteLine(func);

            var keyframeIndexDp = $"#asa_{generalSetting.ModelName.ToLower()}_kindex";
            func = $"scoreboard players set {keyframeIndexDp} AsaMatrix 0";
            writer.WriteLine(func);

            var currentTickDp = $"#asa_{generalSetting.ModelName.ToLower()}_tick_current";
            func = $"scoreboard players set {currentTickDp} AsaMatrix 0";
            writer.WriteLine(func);

            var endTickDp = $"#asa_{generalSetting.ModelName.ToLower()}_tick_end";
            func = $"scoreboard players set {endTickDp} AsaMatrix 0";
            writer.WriteLine(func);

            // Root
            writer.WriteLine(ArmorstandNbtRoot());

            // Parts
            foreach (Node n in nodeList)
            {
                writer.WriteLine(ArmorstandNbt(n));
            }

            writer.Flush();
            writer.Close();

            Debug.Log("Generated summon mcfunction");
        }

        private string ArmorstandNbtRoot()
        {
            string marker, small = "";
            if (generalSetting.IsMarker)
                marker = "Marker:1b";
            else
                marker = "NoGravity:1b,Invulnerable:1b";

            if (generalSetting.IsSmall)
                small = ",Small:1b";

            var line = $"summon armor_stand ~ ~ ~ {{{marker}{small},Invisible:1b,Tags:[\"{generalSetting.ModelName}Root\"]}}";
            return line;
        }

        private string ArmorstandNbt(Node node)
        {
            string marker, small = "";
            if (generalSetting.IsMarker)
                marker = "Marker:1b";
            else
                marker = "NoGravity:1b,Invulnerable:1b";

            if (generalSetting.IsSmall)
                small = ",Small:1b";

            var line = $"summon armor_stand ~ ~ ~ {{{marker}{small},Invisible:1b,Tags:[\"{generalSetting.ModelName}Parts\",\"{node.nodeName}\"],ArmorItems:[{{}},{{}},{{}},{{id:\"minecraft:{generalSetting.CmdItemID}\",Count:1b,tag:{{CustomModelData:{node.customModelData},Rotate:[0f,0f,0f]}}}}],Pose:{{Head:[{node.rotate.x}f,{node.rotate.y}f,{node.rotate.z}f]}}}}";

            return line;
        }

        // Model.mcfunction
        public void GenerateModelFunction(GeneralSettingUI generalSetting, List<Node> nodeList)
        {
            // ファイルパス決定
            var extensionList = new[]
            {
    new ExtensionFilter( "summon.mcfunction", "mcfunction"),
};
            var path = StandaloneFileBrowser.SaveFilePanel("Save File", "", generalSetting.ModelName.ToLower() + "_model", extensionList);
            // ファイルを選択しなかった場合，中断
            if (path.Equals(""))
                return;

            this.generalSetting = generalSetting;

            // RootNodeのみ抽出
            var rootNodeList = new List<Node>();
            foreach (Node n in nodeList)
            {
                if (n.nodeType == NodeType.Root)
                    rootNodeList.Add(n);
            }

            // .mcfunction書き込み用
            writer = new System.IO.StreamWriter(Application.dataPath + "\\ArmorstandAnimator\\Test\\test_model.mcfunction", false);

            // Root matrix
            writer.WriteLine(GetRootMatrix());

            // 各Rootノードで実行
            foreach (Node rootNode in rootNodeList)
            {
                // Rootノード位置設定
                writer.WriteLine(SetRootNodePosition(rootNode));
                // 回転行列計算
                CalcRotation(rootNode);
            }

            writer.Flush();
            writer.Close();

            Debug.Log("Generated model mcfunction");
        }

        // root_matrix
        private string GetRootMatrix()
        {
            var line = FuncGetMatrixRoot;
            return line;
        }

        // rootnode_position
        private string SetRootNodePosition(Node node)
        {
            string line = "";

            if (generalSetting.MultiEntities)
                line = $"execute as @e[type=armor_stand,tag={generalSetting.ModelName}Parts,tag={node.nodeName}] if score @s AsamID = #asa_id_temp AsamID rotated ~ 0 run tp @s ^{-node.pos.x} ^{node.pos.y} ^{node.pos.z} ~ ~";
            else
                line = $"execute as @e[type=armor_stand,tag={generalSetting.ModelName}Parts,tag={node.nodeName},limit=1] rotated ~ 0 run tp @s ^{-node.pos.x} ^{node.pos.y} ^{node.pos.z} ~ ~";

            return line;
        }

        private void CalcRotation(Node node)
        {
            // parent_pos取得
            writer.WriteLine(GetParentPos(node));
            // 回転行列計算
            writer.WriteLine(GetNodeMatrix(node));

            // 全子ノードの位置更新
            foreach (Node n in node.childrenNode)
            {
                // child_pos取得
                var pos = GetChildPos(n);
                writer.WriteLine(pos[0]);
                writer.WriteLine(pos[1]);
                writer.WriteLine(pos[2]);
                // child_pos回転
                writer.WriteLine(FuncGetChildPos);
                // child_pos更新
                writer.WriteLine(SetChildPos(n));
            }

            // 子ノードのうち，子を持つノードにおいてCalcRotation実行
            foreach (Node n in node.childrenNode)
            {
                if (n.nodeType == NodeType.Node)
                    CalcRotation(n);
            }
        }

        // get parent_pos
        private string GetParentPos(Node node)
        {
            string line = "";
            if (generalSetting.MultiEntities)
                line = $"execute as @e[type=armor_stand,tag={generalSetting.ModelName}Parts,tag={node.nodeName}] if score @s AsamID = #asa_id_temp AsamID at @s run {FuncGetParentPos}";
            else
                line = $"execute as @e[type=armor_stand,tag={generalSetting.ModelName}Parts,tag={node.nodeName},limit=1] at @s run {FuncGetParentPos}";

            return line;
        }

        // node_matrix
        private string GetNodeMatrix(Node node)
        {
            string line = "";
            if (generalSetting.MultiEntities)
                line = $"execute as @e[type=armor_stand,tag={generalSetting.ModelName}Parts,tag={node.nodeName}] if score @s AsamID = #asa_id_temp AsamID run {FuncGetMatrixNode}";
            else
                line = $"execute as @e[type=armor_stand,tag={generalSetting.ModelName}Parts,tag={node.nodeName},limit=1] run {FuncGetMatrixNode}";

            return line;
        }

        // get child_pos
        private string[] GetChildPos(Node node)
        {
            var line = new string[3];

            var x = Mathf.FloorToInt(-node.pos.x * ScoreboardMultiple);
            line[0] = $"scoreboard players set #asa_child_pos_x AsaMatrix {x}";
            var y = Mathf.FloorToInt(node.pos.y * ScoreboardMultiple);
            line[1] = $"scoreboard players set #asa_child_pos_y AsaMatrix {y}";
            var z = Mathf.FloorToInt(node.pos.z * ScoreboardMultiple);
            line[2] = $"scoreboard players set #asa_child_pos_z AsaMatrix {z}";

            return line;
        }

        // set child_pos
        private string SetChildPos(Node node)
        {
            string line = "";
            if (generalSetting.MultiEntities)
                line = $"execute as @e[type=armor_stand,tag={generalSetting.ModelName}Parts,tag={node.nodeName}] if score @s AsamID = #asa_id_temp AsamID run {FuncSetChildPos}";
            else
                line = $"execute as @e[type=armor_stand,tag={generalSetting.ModelName}Parts,tag={node.nodeName},limit=1] run {FuncSetChildPos}";

            return line;
        }


        // Summon.mcfunction(外部呼出用)
        public void GenerateSummonFunction(string path, GeneralSettingUI generalSetting, List<Node> nodeList, bool isFixSpeed, bool isMultiEntities)
        {
            // ファイルパス決定
            path = Path.Combine(path, "summon.mcfunction");
            // 設定読み込み
            this.generalSetting = generalSetting;

            // .mcfunction書き込み用
            writer = new System.IO.StreamWriter(path, false);

            if (!isFixSpeed)
            {
                // 値初期化
                var anmSpeedDp = $"#asa_{generalSetting.ModelName.ToLower()}_anmspeed";
                var func = $"scoreboard players set {anmSpeedDp} AsaMatrix 1000";
                writer.WriteLine(func);

                var keyframeIndexDp = $"#asa_{generalSetting.ModelName.ToLower()}_kindex";
                func = $"scoreboard players set {keyframeIndexDp} AsaMatrix 0";
                writer.WriteLine(func);

                var currentTickDp = $"#asa_{generalSetting.ModelName.ToLower()}_tick_current";
                func = $"scoreboard players set {currentTickDp} AsaMatrix 0";
                writer.WriteLine(func);

                var endTickDp = $"#asa_{generalSetting.ModelName.ToLower()}_tick_end";
                func = $"scoreboard players set {endTickDp} AsaMatrix 0";
                writer.WriteLine(func);
            }

            // Root
            writer.WriteLine(ArmorstandNbtRoot());

            if (isMultiEntities)
            {
                // ID割り当て
                var func = $"scoreboard players add #asa_entity_id AsamID 1";
                writer.WriteLine(func);

                func = $"execute if score #asa_entity_id AsamID matches 2147483647.. run scoreboard players set #asa_entity_id AsamID -2147483648";
                writer.WriteLine(func);

                func = $"scoreboard players operation @e[type=armor_stand,tag={generalSetting.ModelName}Root,limit=1,sort=nearest] AsamID = #asa_entity_id AsamID";
                writer.WriteLine(func);
            }

            // Parts
            foreach (Node n in nodeList)
            {
                writer.WriteLine(ArmorstandNbt(n));

                if (isMultiEntities)
                {
                    // ID割り当て
                    var func = $"scoreboard players operation @e[type=armor_stand,tag={generalSetting.ModelName}Parts,tag={n.nodeName},limit=1,sort=nearest] AsamID = #asa_entity_id AsamID";
                    writer.WriteLine(func);
                }
            }

            writer.Flush();
            writer.Close();

            Debug.Log("Generated summon mcfunction");
        }

        // Model.mcfunction(外部呼出用)
        public void GenerateModelFunction(string path, GeneralSettingUI generalSetting, List<Node> nodeList)
        {
            // ファイルパス決定
            path = Path.Combine(path, "model.mcfunction");

            this.generalSetting = generalSetting;
            var isMultiEntities = generalSetting.MultiEntities;

            // .mcfunction書き込み用
            writer = new System.IO.StreamWriter(path);

            // ID取得
            if (isMultiEntities)
            {
                var func = $"scoreboard players operation #asa_id_temp AsamID = @s AsamID";
                writer.WriteLine(func);
            }

            // RootNodeのみ抽出
            var rootNodeList = new List<Node>();
            foreach (Node n in nodeList)
            {
                if (n.nodeType == NodeType.Root)
                    rootNodeList.Add(n);
            }

            // Root matrix
            writer.WriteLine(GetRootMatrix());

            // 各Rootノードで実行
            foreach (Node rootNode in rootNodeList)
            {
                // Rootノード位置設定
                writer.WriteLine(SetRootNodePosition(rootNode));
                // 回転行列計算
                CalcRotation(rootNode);
            }

            writer.Flush();
            writer.Close();

            Debug.Log("Generated model mcfunction");
        }

        // Kill.mcfunction(外部呼出用)
        public void GenerateKillFunction(string path, GeneralSettingUI generalSetting)
        {
            // ファイルパス決定
            path = Path.Combine(path, "kill.mcfunction");
            // 設定読み込み
            this.generalSetting = generalSetting;

            // .mcfunction書き込み用
            writer = new System.IO.StreamWriter(path, false);

            // Root
            var func = $"kill @e[type=armor_stand,tag={generalSetting.ModelName}Root]";
            writer.WriteLine(func);

            // Parts
            func = $"kill @e[type=armor_stand,tag={generalSetting.ModelName}Parts]";
            writer.WriteLine(func);

            writer.Flush();
            writer.Close();

            Debug.Log("Generated kill mcfunction");
        }
    }
}