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
    }
}