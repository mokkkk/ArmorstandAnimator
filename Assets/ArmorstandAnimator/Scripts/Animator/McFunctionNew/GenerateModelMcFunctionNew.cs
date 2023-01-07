using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using SFB;

namespace ArmorstandAnimator
{
    public class GenerateModelMcFunctionNew : MonoBehaviour
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

        private string ArmorstandNbt(Node node, bool isMultiEntities)
        {
            string pos, marker, small = "";

            var posVec = node.GetPosition();
            pos = $"~{posVec.x} ~{posVec.y} ~{posVec.z}";

            if (generalSetting.IsMarker)
                marker = "Marker:1b";
            else
                marker = "NoGravity:1b,Invulnerable:1b";

            if (generalSetting.IsSmall)
                small = ",Small:1b";

            string line = "";
            
            if(isMultiEntities) 
            {
                line = $"summon armor_stand {pos} {{{marker}{small},Invisible:1b,Tags:[\"{generalSetting.ModelName}Parts\",\"{node.nodeName}\",\"StartSummon\"],ArmorItems:[{{}},{{}},{{}},{{id:\"minecraft:{generalSetting.CmdItemID}\",Count:1b,tag:{{CustomModelData:{node.customModelData},Rotate:[0f,0f,0f]}}}}],Pose:{{Head:[{node.rotate.x}f,{node.rotate.y}f,{node.rotate.z}f]}}}}";
            }
            else
            {
                line = $"summon armor_stand {pos} {{{marker}{small},Invisible:1b,Tags:[\"{generalSetting.ModelName}Parts\",\"{node.nodeName}\"],ArmorItems:[{{}},{{}},{{}},{{id:\"minecraft:{generalSetting.CmdItemID}\",Count:1b,tag:{{CustomModelData:{node.customModelData},Rotate:[0f,0f,0f]}}}}],Pose:{{Head:[{node.rotate.x}f,{node.rotate.y}f,{node.rotate.z}f]}}}}";
            }

            return line;
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
                writer.WriteLine(ArmorstandNbt(n, isMultiEntities));

                if (isMultiEntities)
                {
                    // ID割り当て
                    var func = $"scoreboard players operation @e[type=armor_stand,tag={generalSetting.ModelName}Parts,tag={n.nodeName},tag=StartSummon,limit=1,sort=nearest] AsamID = #asa_entity_id AsamID";
                    writer.WriteLine(func);
                }
            }

            // ID割り当て終了
            if (isMultiEntities)
            {
                var func = $"tag @e[type=armor_stand,tag={generalSetting.ModelName}Parts,tag=StartSummon] remove StartSummon";
                writer.WriteLine(func);
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

            // 各ノードで実行
            int i = 0;
            foreach (Node n in nodeList)
            {
                string selector = "";
                if (generalSetting.MultiEntities)
                    selector = $"@e[type=armor_stand,tag={generalSetting.ModelName}Parts,tag={n.nodeName},tag=AsaTarget,limit=1]";
                else
                    selector = $"@e[type=armor_stand,tag={generalSetting.ModelName}Parts,tag={n.nodeName},limit=1]";
                string func = $"data modify entity {selector} {{}} merge from storage asa_temp: Data[{i}]";
                writer.WriteLine(func);
                i++;
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

        // Kill_target.mcfunction(外部呼出用)
        public void GenerateKillTargetFunction(string path, GeneralSettingUI generalSetting)
        {
            // ファイルパス決定
            path = Path.Combine(path, "kill_target.mcfunction");
            // 設定読み込み
            this.generalSetting = generalSetting;

            // .mcfunction書き込み用
            writer = new System.IO.StreamWriter(path, false);

            // 警告表示
            string func = $"execute unless entity @s[type=armor_stand,tag={generalSetting.ModelName}Root] run tellraw @a {{\"text\":\"kill_targetはRootで実行してください\"}}";
            writer.WriteLine(func);
            func = $"execute if entity @s[type=armor_stand,tag={generalSetting.ModelName}Root] run tag @s add ExecuteKill";
            writer.WriteLine(func);

            // 対象を特定
            func = $"execute if entity @s[tag=ExecuteKill] run function asa_animator:{generalSetting.ModelName.ToLower()}/find_target";
            writer.WriteLine(func);

            // Root
            func = $"execute if entity @s[tag=ExecuteKill] run kill @s";
            writer.WriteLine(func);

            // Parts
            func = $"execute if entity @s[tag=ExecuteKill] run kill @e[type=armor_stand,tag={generalSetting.ModelName}Parts,tag=AsaTarget]";
            writer.WriteLine(func);

            writer.Flush();
            writer.Close();

            Debug.Log("Generated kill_target mcfunction");
        }
    }
}