using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace ArmorstandAnimator
{
    public class Keyframe
    {
        // index
        public int index;
        // tick
        public int tick;
        // RootPos
        public Vector3 rootPos;
        // Rotation
        public List<Vector3> rotations;

        public Keyframe(int index, int tick, Vector3 rootPos, List<Vector3> rotations)
        {
            this.index = index;
            this.tick = tick;
            this.rootPos = rootPos;
            this.rotations = rotations;
        }
    }

    public class AnimationManager : MonoBehaviour
    {
        [SerializeField]
        private SceneManager sceneManager;

        // キーフレーム管理用
        public KeyframeUI keyframeUI;
        [SerializeField]
        private List<Keyframe> keyframeList;
        public List<Keyframe> KeyframeList
        {
            get
            {
                return this.keyframeList;
            }
        }

        // 一般設定
        // [SerializeField]
        // private AnimationSettingUI animationSetting;

        // アニメーションUI表示用
        [SerializeField]
        private GameObject animationUIObj;
        [SerializeField]
        private Transform animationUIScrollView;

        // キーフレームビュー表示用
        [SerializeField]
        private GameObject keyframeButtonObj;
        [SerializeField]
        private RectTransform keyframeView;
        private List<KeyframeButton> keyframeButtonList;

        // 選択中のキーフレームindex
        private int selectedKeyframeIndex;

        // キーフレームビュー用
        private const float KeyframeButtonWidth = 25.0f;
        private const float KeyframeButtonMarginOffset = 5f;

        // Start is called before the first frame update
        void Start()
        {
            keyframeList = new List<Keyframe>();
            keyframeButtonList = new List<KeyframeButton>();
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Z))
                AddKeyframe();
        }

        // アニメーションUI表示
        public void CreateAnimationUI()
        {
            // ターゲットキーフレーム設定
            selectedKeyframeIndex = 0;

            // UI作成
            var rotations = new List<Vector3>();
            foreach (Node n in sceneManager.NodeList)
            {
                n.CreateUIAnim(animationUIObj, animationUIScrollView);
                rotations.Add(n.rotate);
            }

            // キーフレームのrotationsとノード数を比較し，一致しない場合キーフレームリストを初期化
            if (keyframeList.Any())
            {
                if (rotations.Count != keyframeList[0].rotations.Count)
                {
                    this.keyframeList = new List<Keyframe>();
                    Debug.Log("Reset Keyframe List");
                }
            }

            // キーフレームが存在しない場合，キーフレーム[0]を作成
            if (!keyframeList.Any())
            {
                CreateKeyframe(-1, rotations);
            }

            // キーフレームリスト数だけキーフレームビュー用ボタン作成
            for (int i = 0; i < keyframeList.Count; i++)
            {
                // キーフレームボタン作成
                AddKeyframeButton(keyframeList[i]);
            }
            // キーフレームビュー更新
            UpdateKeyframeView();

            // リスト更新
            keyframeUI.GetAnimationUIList();
            // 内容設定
            SelectKeyframe(selectedKeyframeIndex);
        }

        // UI消去
        public void ClearAnimationUI()
        {
            // アニメーションUI消去
            foreach (Node n in sceneManager.NodeList)
            {
                Destroy(n.targetAnimationUI.gameObject);
            }
            // キーフレームボタン消去
            foreach (KeyframeButton button in keyframeButtonList)
            {
                Destroy(button.gameObject);
            }
            // リスト更新
            keyframeUI.ClearAnimationUIList();
            keyframeButtonList = new List<KeyframeButton>();
        }

        // アニメーションファイル読込

        // キーフレーム追加
        public void AddKeyframe()
        {
            // キーフレーム作成
            var newKeyframe = new Keyframe(keyframeList.Count, keyframeList[keyframeList.Count - 1].tick + 5, Vector3.zero, keyframeList[selectedKeyframeIndex].rotations);
            this.keyframeList.Add(newKeyframe);
            // キーフレームボタン作成
            AddKeyframeButton(newKeyframe);
            // キーフレームビュー更新
            UpdateKeyframeView();
        }

        // キーフレームボタン作成
        private void AddKeyframeButton(Keyframe keyframe)
        {
            var newKeyframeButton = Instantiate(keyframeButtonObj, Vector3.zero, Quaternion.identity, keyframeView.transform);
            var button = newKeyframeButton.GetComponent<KeyframeButton>();
            button.Initialize(keyframe.index, keyframe.tick, this);
            keyframeButtonList.Add(button);
        }

        // キーフレーム作成
        public void CreateKeyframe(int tick, List<Vector3> rotations)
        {
            // キーフレーム作成
            var newKeyframe = new Keyframe(0, tick + 1, Vector3.zero, rotations);
            this.keyframeList.Add(newKeyframe);
        }

        // キーフレーム選択
        public void SelectKeyframe(int index)
        {
            // index更新
            selectedKeyframeIndex = index;
            // 内容設定
            keyframeUI.SetUIContent(this.keyframeList[selectedKeyframeIndex]);
            // ノードTransform更新
            SetNodeRotation(keyframeList[index]);
        }

        // キーフレームリスト更新
        public int UpdateKeyframeList(Keyframe keyframe)
        {
            // ノードTransform更新
            SetNodeRotation(keyframe);
            // キーフレームの値更新
            this.keyframeList[selectedKeyframeIndex] = keyframe;
            // tick順でキーフレームソート
            SortKeyframeByTick();
            // キーフレームビュー更新
            UpdateKeyframeView();
            // 更新後のキーフレームIndexを返す
            return selectedKeyframeIndex;
        }

        // キーフレームリストをtick昇順で並べ替え
        private void SortKeyframeByTick()
        {
            // 現在のキーフレーム
            var currentKeyframe = keyframeList[selectedKeyframeIndex];

            // tick昇順でソート
            var orderList = keyframeList.OrderBy(d => d.tick);
            keyframeList = new List<Keyframe>();

            int i = 0;
            int index = 0;
            foreach (Keyframe k in orderList)
            {
                k.index = i;
                keyframeList.Add(k);
                if (ReferenceEquals(k.rotations, currentKeyframe.rotations))
                    index = i;
                i++;
            }

            // ソート後のindexを設定
            selectedKeyframeIndex = index;
        }

        // ノードRotate更新
        public void SetNodeRotation(Keyframe keyframe)
        {
            for (int i = 0; i < sceneManager.NodeList.Count; i++)
            {
                sceneManager.NodeList[i].SetRotation(keyframe.rotations[i]);
            }

            // 各RootノードでSetNodePosition実行
            foreach (Node rootNode in sceneManager.NodeList)
            {
                if (rootNode.nodeType == NodeType.Root)
                    SetNodePosition(rootNode);
            }

            // 全ノードの位置をずらす
            foreach (Node n in sceneManager.NodeList)
            {
                n.transform.position += keyframe.rootPos;
            }
        }

        // ノード位置決定
        public void SetNodePosition(Node targetNode)
        {
            // 親ノードの位置取得
            var parentPos = Vector3.zero;
            if (!ReferenceEquals(targetNode.parentNode, null))
                parentPos = targetNode.parentNode.transform.position;

            // 親ノードの回転取得
            var parentRotation = Vector3.zero;
            if (!ReferenceEquals(targetNode.parentNode, null))
                parentRotation = targetNode.parentNode.rotate;

            // 位置計算
            var rotatedPos = MatrixRotation.RotationWorld(MatrixRotation.RotationLocal(targetNode.pos, parentRotation), parentRotation);
            rotatedPos += parentPos;

            // 位置更新
            targetNode.transform.position = rotatedPos;

            // 自分の子ノードでSetNodePosition実行
            if (targetNode.childrenNode.Any())
                foreach (Node n in targetNode.childrenNode)
                    SetNodePosition(n);
        }

        // キーフレームビューのボタン位置設定
        private void UpdateKeyframeView()
        {
            int i = 0;
            // 最も右にあるボタン
            float rightButtonPos = 0.0f;
            foreach (KeyframeButton button in keyframeButtonList)
            {
                // ボタンの値を設定
                button.SetValue(i, keyframeList[i].tick);

                rightButtonPos = KeyframeButtonWidth + button.tick * KeyframeButtonMarginOffset;
                button.GetComponent<RectTransform>().anchoredPosition = new Vector2(rightButtonPos, 0.0f);
                i++;
            }

            // キーフレームビューのサイズ設定
            keyframeView.sizeDelta = new Vector2(rightButtonPos + KeyframeButtonWidth, keyframeView.sizeDelta.y);
        }
    }
}