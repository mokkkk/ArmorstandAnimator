using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using SFB;

namespace ArmorstandAnimator
{
    // AnimationFile保存用
    [Serializable]
    public class ASAAnimationProject
    {
        public string animationName;
        public ASAAnimationKeyframe[] keyframes;
    }
    [Serializable]
    public class ASAAnimationKeyframe
    {
        public int tick;
        public float[] rootPos;
        public ASAAnimationRotate[] rotates;
    }
    [Serializable]
    public class ASAAnimationRotate
    {
        public float[] rotate;
    }

    public class AnimationFileManager : MonoBehaviour
    {
        // ProjectFile読込用パス
        private string[] paths;

        // Animationプロジェクトファイル保存
        public void SaveProjectFileAnim(AnimationSettingUI animationSetting, List<Keyframe> keyframeList)
        {
            // ファイルパス決定
            var extensionList = new[]
            {
    new ExtensionFilter( "Asa Animation Project", "asaanim"),
};
            var path = StandaloneFileBrowser.SaveFilePanel("Save File", "", "animation", extensionList);
            // ファイルを選択しなかった場合，中断
            if (path.Equals(""))
                return;

            // Jsonシリアライズ用インスタンスに値代入
            ASAAnimationProject project = new ASAAnimationProject();
            Debug.Log(animationSetting.AnimationName);
            project.animationName = animationSetting.AnimationName;

            var keyframeArray = new ASAAnimationKeyframe[keyframeList.Count];
            for (int i = 0; i < keyframeArray.Length; i++)
            {
                keyframeArray[i] = new ASAAnimationKeyframe();
                keyframeArray[i].tick = keyframeList[i].tick;
                keyframeArray[i].rootPos = new float[] { keyframeList[i].rootPos.x, keyframeList[i].rootPos.y, keyframeList[i].rootPos.z };
                keyframeArray[i].rotates = new ASAAnimationRotate[keyframeList[i].rotations.Count];
                for (int j = 0; j < keyframeArray[i].rotates.Length; j++)
                {
                    keyframeArray[i].rotates[j] = new ASAAnimationRotate();
                    keyframeArray[i].rotates[j].rotate = new float[] { keyframeList[i].rotations[j].x, keyframeList[i].rotations[j].y, keyframeList[i].rotations[j].z };
                }
            }
            project.keyframes = keyframeArray;

            // プロジェクトファイル保存
            var projectFileJson = JsonUtility.ToJson(project);
            System.IO.StreamWriter writer = new System.IO.StreamWriter(path, false);
            writer.WriteLine(projectFileJson);
            writer.Flush();
            writer.Close();

            Debug.Log("AnimationFile Saved");
        }

        public int LoadProjectFileAnim(out ASAAnimationProject project)
        {
            // // 初期化
            // paths = new string[];
            project = new ASAAnimationProject();

            // ファイルダイアログを開く
            var extensions = new[]
            {
    new ExtensionFilter( "Animation Files", "asaanim"),
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

            project = JsonUtility.FromJson<ASAAnimationProject>(inputString);

            Debug.Log("AnimationFile Loaded");

            return 0;
        }
    }
}