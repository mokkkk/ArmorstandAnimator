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
        private List<Keyframe> keyframeList;
        public List<Keyframe> KeyframeList
        {
            get
            {
                return this.keyframeList;
            }
        }

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

        // アニメーション用マスク
        [SerializeField]
        private GameObject whileAnimationMask;
        // アニメーション再生中ステータス
        private bool isPreviewPlaying = false;
        private bool stopPreview = false;

        // 選択中のキーフレームindex
        private int selectedKeyframeIndex;

        // アニメーション終了タイム
        private int animationEndTime;

        // キーフレームビュー用
        private const float KeyframeButtonWidth = 25.0f;
        private const float KeyframeButtonMarginOffset = 5f;
        // tick -> 秒
        private const float TickToSec = 0.05f;

        // Start is called before the first frame update
        void Start()
        {
            keyframeList = new List<Keyframe>();
            keyframeButtonList = new List<KeyframeButton>();
        }

        // アニメーションUI表示
        public void CreateAnimationUI()
        {
            // ターゲットキーフレーム設定
            selectedKeyframeIndex = 0;

            // UI作成
            keyframeUI.ClearAnimationUIList();
            var rotations = new List<Vector3>();
            foreach (Node n in sceneManager.NodeList)
            {
                n.CreateUIAnim(animationUIObj, animationUIScrollView);
                keyframeUI.animationUIList.Add(n.targetAnimationUI);
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

            // 内容設定
            SelectKeyframe(selectedKeyframeIndex);

            // アニメーション終了時間更新
            this.animationEndTime = keyframeList[keyframeList.Count - 1].tick;
        }

        // アニメーションファイル読込
        public void CreateAnimationUIProject(ASAAnimationProject project)
        {
            int i = 0;
            foreach (ASAAnimationKeyframe k in project.keyframes)
            {
                var rootPos = new Vector3(k.rootPos[0], k.rootPos[1], k.rootPos[2]);
                var rotations = new List<Vector3>();
                foreach (ASAAnimationRotate r in k.rotates)
                    rotations.Add(new Vector3(r.rotate[0], r.rotate[1], r.rotate[2]));

                var newKeyframe = new Keyframe(i, k.tick, rootPos, rotations);
                keyframeList.Add(newKeyframe);

                i++;
            }
            CreateAnimationUI();
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

        // UI消去
        public void ClearAnimationUIOnLoad()
        {
            // UI消去
            ClearAnimationUI();
            // キーフレーム消去
            this.keyframeList = new List<Keyframe>();
        }

        // キーフレーム追加
        public void AddKeyframe()
        {
            // キーフレーム作成
            var newKeyframe = new Keyframe(keyframeList.Count, keyframeList[keyframeList.Count - 1].tick + 5, keyframeList[selectedKeyframeIndex].rootPos, keyframeList[selectedKeyframeIndex].rotations);
            this.keyframeList.Add(newKeyframe);
            // キーフレームボタン作成
            AddKeyframeButton(newKeyframe);
            // キーフレームビュー更新
            UpdateKeyframeView();
            // アニメーション終了時間更新
            this.animationEndTime = keyframeList[keyframeList.Count - 1].tick;
            // 追加したキーフレームを選択
            SelectKeyframe(keyframeList.Count - 1);
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

        // キーフレーム削除
        public void DeleteKeyframe()
        {
            // キーフレーム削除
            keyframeList.RemoveAt(selectedKeyframeIndex);
            // キーフレームボタン削除
            var kfb = keyframeButtonList[selectedKeyframeIndex];
            Destroy(kfb.gameObject);
            keyframeButtonList.RemoveAt(selectedKeyframeIndex);
            var currentKeyframe = --selectedKeyframeIndex;
            // tick順でキーフレームソート
            SortKeyframeByTick();
            // キーフレームビュー更新
            UpdateKeyframeView();
            // 内容設定
            selectedKeyframeIndex = currentKeyframe;
            SelectKeyframe(selectedKeyframeIndex);
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

        // キーフレームリスト更新
        public int UpdateKeyframeListTick(Keyframe keyframe)
        {
            // ノードTransform更新
            SetNodeRotation(keyframe);
            foreach (Keyframe k in keyframeList)
            {
                // Tickが被らないようにする
                if (k.tick == keyframe.tick)
                    keyframe.tick++;
            }
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
            // 各RootノードでSetNodePosition実行
            foreach (Node rootNode in sceneManager.NodeList)
            {
                if (rootNode.nodeType == NodeType.Root)
                    SetNodePosition(rootNode, keyframe, Vector3.zero);
            }

            // 全ノードの位置をずらす
            foreach (Node n in sceneManager.NodeList)
            {
                n.transform.position += keyframe.rootPos;
            }
        }

        // ノード位置決定
        public void SetNodePosition(Node targetNode, Keyframe keyframe, Vector3 parentRotate)
        {
            // 角度更新
            sceneManager.NodeList[targetNode.nodeID].SetRotation(keyframe.rotations[targetNode.nodeID], parentRotate);

            // 親ノードの位置取得
            var parentPos = Vector3.zero;
            if (!ReferenceEquals(targetNode.parentNode, null))
                parentPos = targetNode.parentNode.transform.position;

            // 位置計算
            var rotatedPos = MatrixRotation.RotationWorld(MatrixRotation.RotationLocal(targetNode.pos, parentRotate), parentRotate);
            rotatedPos += parentPos;

            // 位置更新
            targetNode.transform.position = rotatedPos;

            // 自分の子ノードでSetNodePosition実行
            if (targetNode.childrenNode.Any())
                foreach (Node n in targetNode.childrenNode)
                    SetNodePosition(n, keyframe, parentRotate + targetNode.rotate);
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


        // プレビュー再生
        public void PlayAnimationPreview()
        {
            if (!isPreviewPlaying)
                StartCoroutine("AnimationPreview");
        }

        // プレビュー停止
        public void StopAnimationPreview()
        {
            if (isPreviewPlaying)
                stopPreview = true;
        }


        // アニメーションプレビュー
        private IEnumerator AnimationPreview()
        {
            Debug.Log("Start Animation");

            whileAnimationMask.SetActive(true);
            isPreviewPlaying = true;

            int index = 0;
            while (index < keyframeList.Count - 1 && !stopPreview)
            {
                // tick
                float t = 0.0f;
                // アニメーション時間
                float animationTime = (keyframeList[index + 1].tick - keyframeList[index].tick) * TickToSec;

                // indexからindex+1までアニメーション再生
                while (t < animationTime && !stopPreview)
                {
                    var lerpTime = t / animationTime;
                    var rootX = Mathf.Lerp(keyframeList[index].rootPos.x, keyframeList[index + 1].rootPos.x, lerpTime);
                    var rootY = Mathf.Lerp(keyframeList[index].rootPos.y, keyframeList[index + 1].rootPos.y, lerpTime);
                    var rootZ = Mathf.Lerp(keyframeList[index].rootPos.z, keyframeList[index + 1].rootPos.z, lerpTime);
                    var rootPos = new Vector3(rootX, rootY, rootZ);
                    var rotations = new List<Vector3>();
                    for (int i = 0; i < keyframeList[index].rotations.Count; i++)
                    {
                        var rotateX = Mathf.Lerp(keyframeList[index].rotations[i].x, keyframeList[index + 1].rotations[i].x, lerpTime);
                        var rotateY = Mathf.Lerp(keyframeList[index].rotations[i].y, keyframeList[index + 1].rotations[i].y, lerpTime);
                        var rotateZ = Mathf.Lerp(keyframeList[index].rotations[i].z, keyframeList[index + 1].rotations[i].z, lerpTime);
                        rotations.Add(new Vector3(rotateX, rotateY, rotateZ));
                    }

                    var dummyKey = new Keyframe(0, 0, rootPos, rotations);

                    SetNodeRotation(dummyKey);

                    t += Time.deltaTime;
                    yield return null;
                }

                index++;
            }

            // アニメーション終了
            SelectKeyframe(0);

            whileAnimationMask.SetActive(false);
            isPreviewPlaying = false;
            stopPreview = false;

            Debug.Log("End Animation");
        }
    }
}