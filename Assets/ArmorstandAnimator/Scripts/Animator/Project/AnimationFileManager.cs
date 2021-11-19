using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
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
        public ASAAnimationEvent[] events;
    }
    [Serializable]
    public class ASAAnimationKeyframe
    {
        public int tick;
        public float[] rootPos;
        public ASAAnimationRotate[] rotates;
        public bool isQuick;
    }
    [Serializable]
    public class ASAAnimationRotate
    {
        public float[] rotate;
    }
    [Serializable]
    public class ASAAnimationEvent
    {
        public string name;
        public int tick;
    }

    public class AnimationFileManager : MonoBehaviour
    {
        // ProjectFile読込用パス
        private string[] paths;

        private const string PathHistoryFileName = "pathhist_animation.json";

        // Animationプロジェクトファイル保存
        public void SaveProjectFileAnim(AnimationSettingUI animationSetting, List<Keyframe> keyframeList, List<EventUI> eventList)
        {
            // ファイルパス決定
            var extensionList = new[]
            {
    new ExtensionFilter( "Asa Animation Project", "asaanim"),
};

            // var path = StandaloneFileBrowser.SaveFilePanel("Save File", "", "animation", extensionList);

            paths = StandaloneFileBrowser.OpenFilePanel("Save File", "", extensionList, false);
            var path = paths[0];

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
                keyframeArray[i].isQuick = keyframeList[i].isQuick;
            }
            project.keyframes = keyframeArray;

            var eventArray = new ASAAnimationEvent[eventList.Count];
            for (int i = 0; i < eventArray.Length; i++)
            {
                eventArray[i] = new ASAAnimationEvent();
                eventArray[i].name = eventList[i].Name;
                eventArray[i].tick = eventList[i].Tick;
            }
            project.events = eventArray;

            // プロジェクトファイル保存
            var projectFileJson = JsonUtility.ToJson(project);
            System.IO.StreamWriter writer = new System.IO.StreamWriter(path, false);
            writer.WriteLine(projectFileJson);
            writer.Flush();
            writer.Close();

            Debug.Log("AnimationFile Saved");

            // アクセス履歴保存
            SavePathHistory(path);
        }

        public int SelectPath(out ASAAnimationProject project)
        {
            // // 初期化
            project = new ASAAnimationProject();

            // ファイルダイアログを開く
            var extensions = new[]
            {
    new ExtensionFilter( "Animation Files", "asaanim"),
};
            paths = StandaloneFileBrowser.OpenFilePanel("Open File", "", extensions, false);

            // ファイルを選択しなかった場合，中断
            if (paths.Length < 1)
                return -1;

            LoadProjectFileAnim(paths[0], out project);

            return 0;
        }

        public int LoadProjectFileAnim(string path, out ASAAnimationProject project)
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

            project = JsonUtility.FromJson<ASAAnimationProject>(inputString);

            Debug.Log("AnimationFile Loaded");

            return 0;
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

        public int LoadProjectFileAnimCurrent(string path, out ASAAnimationProject project)
        {
            // 初期化
            project = new ASAAnimationProject();

            LoadProjectFileAnim(path, out project);

            return 0;
        }
    }
}