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
        // isQuick
        public bool isQuick;

        public Keyframe(int index, int tick, Vector3 rootPos, List<Vector3> rotations, bool isQuick)
        {
            this.index = index;
            this.tick = tick;
            this.rootPos = rootPos;
            this.rotations = rotations;
            this.isQuick = isQuick;
        }
    }

    public class AnimationManager : MonoBehaviour
    {
        [SerializeField]
        private SceneManager sceneManager;
        [SerializeField]
        private AnimationSettingUI animationSetting;

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
        public int SelectedKeyframeIndex
        {
            get
            {
                return this.selectedKeyframeIndex;
            }
        }

        // 複数選択中キーフレームindex
        private List<int> selectedKeyframeList;

        // アニメーション終了タイム
        private int animationEndTime;

        // コピペ用
        private Keyframe copiedKeyframe;

        // キーフレーム補完用
        [SerializeField]
        private UnityEngine.UI.InputField separateCount;

        // キーフレームビュー用
        private const float KeyframeButtonWidth = 25.0f;
        private const float KeyframeButtonMarginOffset = 5f;
        // tick -> 秒
        private const float TickToSec = 0.05f;
        private const float SmallArmorStandHeightOffset = -0.7f;

        // Start is called before the first frame update
        void Start()
        {
            keyframeList = new List<Keyframe>();
            keyframeButtonList = new List<KeyframeButton>();
            animationSetting.SetSpeed(1.0f);
            keyframeUI.ClearEventUIList();
            separateCount.text = "2";
            selectedKeyframeList = new List<int>();
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

            // キーフレームのrotationsとノード数を比較
            if (keyframeList.Any())
            {
                if (keyframeList[0].rotations.Count > 0 && rotations.Count != keyframeList[0].rotations.Count)
                {
                    // Rotations < ノード数
                    if (rotations.Count < keyframeList[0].rotations.Count)
                    {
                        var diff = keyframeList[0].rotations.Count - rotations.Count;

                        foreach (Keyframe k in keyframeList)
                        {
                            for (int i = 0; i < diff; i++)
                                k.rotations.RemoveAt(k.rotations.Count - 1);
                        }
                    }
                    // Rotations > ノード数
                    else if (rotations.Count > keyframeList[0].rotations.Count)
                    {
                        var diff = rotations.Count - keyframeList[0].rotations.Count;
                        foreach (Keyframe k in keyframeList)
                        {
                            for (int i = 0; i < diff; i++)
                            {
                                k.rotations.Add(Vector3.zero);
                            }
                        }
                    }
                }
                else if (keyframeList[0].rotations.Count <= 0)
                {
                    keyframeList = new List<Keyframe>();
                }
            }

            // キーフレームが存在しない場合，キーフレーム[0]を作成
            if (!keyframeList.Any())
            {
                copiedKeyframe = CreateKeyframe(-1, rotations);
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

            // 選択中キーフレームリセット
            selectedKeyframeList = new List<int>();
            UpdateKeyframeView();
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

                var newKeyframe = new Keyframe(i, k.tick, rootPos, rotations, k.isQuick);
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
            // ローテーション用リスト新規作成
            var newRotations = new List<Vector3>();
            foreach (Vector3 rotate in keyframeList[selectedKeyframeIndex].rotations)
                newRotations.Add(rotate);
            // キーフレーム作成
            var newKeyframe = new Keyframe(keyframeList.Count, keyframeList[keyframeList.Count - 1].tick + 5, keyframeList[selectedKeyframeIndex].rootPos, newRotations, false);
            this.keyframeList.Add(newKeyframe);
            // キーフレームボタン作成
            AddKeyframeButton(newKeyframe);
            // キーフレームビュー更新
            UpdateKeyframeView();
            // アニメーション終了時間更新
            this.animationEndTime = keyframeList[keyframeList.Count - 1].tick;
            // 追加したキーフレームを選択
            SelectKeyframe(keyframeList.Count - 1);

            // 選択中キーフレームリセット
            selectedKeyframeList = new List<int>();
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
        public Keyframe CreateKeyframe(int tick, List<Vector3> rotations)
        {
            // キーフレーム作成
            var newKeyframe = new Keyframe(0, tick + 1, Vector3.zero, rotations, false);
            this.keyframeList.Add(newKeyframe);
            return newKeyframe;
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

            // 選択中キーフレームリセット
            selectedKeyframeList = new List<int>();
            UpdateKeyframeView();
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

            // 選択中キーフレームリセット
            selectedKeyframeList = new List<int>();
            UpdateKeyframeView();
        }

        // キーフレームリスト更新
        public int UpdateKeyframeList(Keyframe keyframe)
        {
            // ノードTransform更新
            SetNodeRotation(keyframe);
            // ローテーション用リスト新規作成
            var newRotations = new List<Vector3>();
            foreach (Vector3 rotate in keyframe.rotations)
                newRotations.Add(rotate);
            // キーフレームの値更新
            this.keyframeList[selectedKeyframeIndex].index = keyframe.index;
            this.keyframeList[selectedKeyframeIndex].rootPos = keyframe.rootPos;
            this.keyframeList[selectedKeyframeIndex].rotations = newRotations;
            this.keyframeList[selectedKeyframeIndex].tick = keyframe.tick;
            this.keyframeList[selectedKeyframeIndex].isQuick = keyframe.isQuick;
            // this.keyframeList[selectedKeyframeIndex] = keyframe;
            // Debug.Log(keyframeList[selectedKeyframeIndex]);
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
            int i = 0;
            foreach (Keyframe k in keyframeList)
            {
                // Tickが被らないようにする
                if (k.tick == keyframe.tick && i != selectedKeyframeIndex)
                    keyframe.tick++;
                i++;
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

            // IsSmall == true の場合，全ノードを下にオフセット
            if (sceneManager.GeneralSetting.IsSmall)
            {
                foreach (Node n in sceneManager.NodeList)
                {
                    n.transform.localPosition += Vector3.up * SmallArmorStandHeightOffset;
                }
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

                // ボタン色設定（IsQuick）
                if (keyframeList[i].isQuick)
                    button.transform.Find("Button").GetComponent<UnityEngine.UI.Image>().color = new Color(0.0f, 0.8806345f, 1.0f, 1.0f);
                else
                    button.transform.Find("Button").GetComponent<UnityEngine.UI.Image>().color = new Color(1.0f, 0.7526976f, 0.0f, 1.0f);

                // ボタン色設定
                if (selectedKeyframeList.IndexOf(i) > -1)
                    button.transform.Find("Button").GetComponent<UnityEngine.UI.Image>().color = new Color(0.0f, 0f, 1.0f, 1.0f);

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

            isPreviewPlaying = true;

            // 再生速度変換済みキーフレーム
            int index = 0;
            var spdKeyframeList = new List<Keyframe>();
            foreach (Keyframe k in keyframeList)
            {
                var newKey = new Keyframe(index, Mathf.FloorToInt(k.tick / animationSetting.AnimationSpeed), k.rootPos, k.rotations, false);
                spdKeyframeList.Add(newKey);
            }

            index = 0;
            while (index < spdKeyframeList.Count - 1 && !stopPreview)
            {
                // tick
                float t = 0.0f;
                // アニメーション時間
                float animationTime = (spdKeyframeList[index + 1].tick - spdKeyframeList[index].tick) * TickToSec;

                // indexからindex+1までアニメーション再生
                while (t < animationTime && !stopPreview)
                {
                    var lerpTime = t / animationTime;
                    var rootX = Mathf.Lerp(spdKeyframeList[index].rootPos.x, spdKeyframeList[index + 1].rootPos.x, lerpTime);
                    var rootY = Mathf.Lerp(spdKeyframeList[index].rootPos.y, spdKeyframeList[index + 1].rootPos.y, lerpTime);
                    var rootZ = Mathf.Lerp(spdKeyframeList[index].rootPos.z, spdKeyframeList[index + 1].rootPos.z, lerpTime);
                    var rootPos = new Vector3(rootX, rootY, rootZ);
                    var rotations = new List<Vector3>();
                    for (int i = 0; i < spdKeyframeList[index].rotations.Count; i++)
                    {
                        var rotateX = Mathf.Lerp(spdKeyframeList[index].rotations[i].x, spdKeyframeList[index + 1].rotations[i].x, lerpTime);
                        var rotateY = Mathf.Lerp(spdKeyframeList[index].rotations[i].y, spdKeyframeList[index + 1].rotations[i].y, lerpTime);
                        var rotateZ = Mathf.Lerp(spdKeyframeList[index].rotations[i].z, spdKeyframeList[index + 1].rotations[i].z, lerpTime);
                        rotations.Add(new Vector3(rotateX, rotateY, rotateZ));
                    }

                    var dummyKey = new Keyframe(0, 0, rootPos, rotations, false);

                    SetNodeRotation(dummyKey);

                    if (animationSetting.IsTickAnimation)
                    {
                        t += TickToSec;
                        yield return new WaitForSeconds(0.05f);
                    }
                    else
                    {
                        t += Time.deltaTime;
                        yield return null;
                    }
                }

                index++;
            }

            // アニメーション終了
            SelectKeyframe(0);

            isPreviewPlaying = false;
            stopPreview = false;

            Debug.Log("End Animation");
        }

        // キーフレームコピー
        public void CopyKeyframe()
        {
            this.copiedKeyframe = this.keyframeList[selectedKeyframeIndex];
        }

        // キーフレームペースト
        public void PasteKeyframe()
        {
            if (!ReferenceEquals(copiedKeyframe, null))
            {
                var tickCurrent = this.keyframeList[selectedKeyframeIndex].tick;
                var rootPosCurrent = copiedKeyframe.rootPos;
                if (selectedKeyframeIndex == 0)
                    rootPosCurrent = Vector3.zero;

                var newRotations = new List<Vector3>();
                foreach (Vector3 rotate in copiedKeyframe.rotations)
                    newRotations.Add(rotate);

                var newKeyframe = new Keyframe(selectedKeyframeIndex, tickCurrent, rootPosCurrent, newRotations, false);

                // 内容設定
                keyframeUI.SetUIContent(newKeyframe);
                UpdateKeyframeList(newKeyframe);
            }
        }

        public void StartMirrotKeyframe()
        {
            if (selectedKeyframeList.Any())
            {
                var tempIndex = selectedKeyframeIndex;
                foreach (int index in selectedKeyframeList)
                {
                    selectedKeyframeIndex = index;
                    MirrorKeyframe(keyframeList[index]);
                }
                selectedKeyframeIndex = tempIndex;
                SelectKeyframe(selectedKeyframeIndex);
            }
            else
            {
                MirrorKeyframe(keyframeList[selectedKeyframeIndex]);
            }
        }

        // キーフレーム反転（feature）
        void MirrorKeyframe(Keyframe k)
        {
            // ローテーション用リスト新規作成
            var newRotations = new List<Vector3>();
            foreach (Vector3 rotate in k.rotations)
                newRotations.Add(rotate);

            var newKeyframe = new Keyframe(selectedKeyframeIndex, k.tick, k.rootPos, newRotations, k.isQuick);
            int length = sceneManager.NodeList.Count();

            // rootPos反転
            newKeyframe.rootPos = new Vector3(-newKeyframe.rootPos.x, newKeyframe.rootPos.y, newKeyframe.rootPos.z);

            for (int i = 0; i < length; i++)
            {
                bool isMirror = false;
                var nameA = sceneManager.NodeList[i].nodeName;

                // L to R
                int index = nameA.LastIndexOf('L');

                // LでのSplitに成功
                if (index > 0)
                {
                    string[] splitStrL = { nameA.Remove(index), nameA.Substring(index + 1) };
                    for (int j = 0; j < length; j++)
                    {
                        var nameB = sceneManager.NodeList[j].nodeName;
                        index = nameB.LastIndexOf('R');

                        // RでのSplitに成功
                        if (index > 0)
                        {
                            string[] splitStrR = { nameB.Remove(index), nameB.Substring(index + 1) };

                            // 名前が対応している場合
                            if (splitStrL.SequenceEqual(splitStrR))
                            {
                                isMirror = true;
                                if (i < j)
                                {
                                    var tempRotateA = newKeyframe.rotations[i];
                                    var tempRotateB = newKeyframe.rotations[j];

                                    newKeyframe.rotations[i] = new Vector3(tempRotateB.x, -tempRotateB.y, -tempRotateB.z);
                                    newKeyframe.rotations[j] = new Vector3(tempRotateA.x, -tempRotateA.y, -tempRotateA.z);
                                    Debug.Log($"Mirror: {nameA} & {nameB}");
                                }
                                else
                                {
                                    Debug.Log($"{nameA} already update");
                                }
                            }
                        }
                    }
                }


                // R to L
                index = nameA.LastIndexOf('R');

                // LでのSplitに成功
                if (index > 0)
                {
                    string[] splitStrL = { nameA.Remove(index), nameA.Substring(index + 1) };
                    for (int j = 0; j < length; j++)
                    {
                        var nameB = sceneManager.NodeList[j].nodeName;
                        index = nameB.LastIndexOf('L');

                        // LでのSplitに成功
                        if (index > 0)
                        {
                            string[] splitStrR = { nameB.Remove(index), nameB.Substring(index + 1) };

                            // 名前が対応している場合
                            if (splitStrL.SequenceEqual(splitStrR))
                            {
                                isMirror = true;
                                if (i < j)
                                {
                                    var tempRotateA = newKeyframe.rotations[i];
                                    var tempRotateB = newKeyframe.rotations[j];

                                    newKeyframe.rotations[i] = new Vector3(tempRotateB.x, -tempRotateB.y, -tempRotateB.z);
                                    newKeyframe.rotations[j] = new Vector3(tempRotateA.x, -tempRotateA.y, -tempRotateA.z);
                                    Debug.Log($"Mirror: {nameA} & {nameB}");
                                }
                                else
                                {
                                    Debug.Log($"{nameA} already update");
                                }
                            }
                        }
                    }
                }

                if (!isMirror)
                {
                    var tempRotate = newKeyframe.rotations[i];
                    newKeyframe.rotations[i] = new Vector3(tempRotate.x, -tempRotate.y, -tempRotate.z);
                }
            }

            // 内容設定
            keyframeUI.SetUIContent(newKeyframe);
            UpdateKeyframeList(newKeyframe);
        }

        // キーフレーム分割（feature）
        public void SeparateKeyframe()
        {
            if (selectedKeyframeIndex >= keyframeList.Count)
                return;

            var count = int.Parse(separateCount.text);
            if (count < 2)
                return;

            var keyframeA = keyframeList[selectedKeyframeIndex];
            var keyframeB = keyframeList[selectedKeyframeIndex + 1];

            // indexをずらす
            for (int i = keyframeA.index + 1; i < keyframeList.Count; i++)
                keyframeList[i].index += count - 1;

            for (int i = 1; i < count; i++)
            {
                // tick計算
                var newTick = keyframeA.tick + Mathf.FloorToInt((keyframeB.tick - keyframeA.tick) / count * (count - i));
                // rootPos計算
                var rootPosDiff = keyframeB.rootPos - keyframeA.rootPos;
                var newRootPos = keyframeA.rootPos + (rootPosDiff / count * (count - i));
                // ローテーション用リスト新規作成
                var newRotations = new List<Vector3>();
                for (int j = 0; j < keyframeA.rotations.Count; j++)
                {
                    var rotateDiff = keyframeB.rotations[j] - keyframeA.rotations[j];
                    var rotate = keyframeA.rotations[j] + (rotateDiff / count * (count - i));
                    newRotations.Add(rotate);
                }
                // キーフレーム作成
                var newKeyframe = new Keyframe(keyframeA.index + i, newTick, newRootPos, newRotations, false);
                this.keyframeList.Add(newKeyframe);
                // キーフレームボタン作成
                AddKeyframeButton(newKeyframe);
            }
            // 追加したキーフレームを選択
            SelectKeyframe(keyframeList.Count - 1);
            // tick順でキーフレームソート
            SortKeyframeByTick();
            // キーフレームビュー更新
            UpdateKeyframeView();
            // アニメーション終了時間更新
            this.animationEndTime = keyframeList[keyframeList.Count - 1].tick;
        }

        // ノード選択
        public void OnNodeSelected(Node node)
        {
            sceneManager.SetCurrentNode(node);
        }

        // rootpos更新
        public void UpdateRootPos(Vector3 pos)
        {
            keyframeUI.UpdateRootPos(pos);
        }

        // キーフレーム複数選択
        public void AddSelectedKeyframe(int index)
        {
            // リスト内にすでに要素が存在する場合，選択解除
            if (selectedKeyframeList.IndexOf(index) > -1)
            {
                selectedKeyframeList.Remove(index);
                return;
            }

            // SelectedKeyframeListに対象キーフレーム追加
            selectedKeyframeList.Add(index);
            // KeyframeView更新
            UpdateKeyframeView();
        }

        // キー入力検知
        public void CheckKey()
        {
            // キーフレームTick移動ショートカット
            if (Input.GetKeyDown(KeyCode.RightArrow))
                if (Input.GetKey(KeyCode.LeftControl))
                    ScMoveKeyframe(1, true);
                else if (Input.GetKey(KeyCode.LeftShift))
                    ScMoveKeyframe(5, true);
                else
                    ScMoveKeyframe(10, true);

            if (Input.GetKeyDown(KeyCode.LeftArrow))
                if (Input.GetKey(KeyCode.LeftControl))
                    ScMoveKeyframe(1, false);
                else if (Input.GetKey(KeyCode.LeftShift))
                    ScMoveKeyframe(5, false);
                else
                    ScMoveKeyframe(10, false);
        }

        // キーフレーム移動
        void ScMoveKeyframe(int add, bool right)
        {
            if (selectedKeyframeList.Any())
            {
                int i = 0, offset = 0;

                foreach (int index in selectedKeyframeList)
                {
                    // Index = 0の場合移動しない
                    if (index == 0)
                        continue;

                    // Tickが被らないようにする
                    var currentTick = keyframeList[index].tick;
                    var tickMax = currentTick + add;
                    var tickMin = currentTick - add;
                    Keyframe k;

                    if (index != keyframeList.Count - 1)
                    {
                        i = index + 1;
                        k = keyframeList[i];
                        if (right && k.tick <= tickMax && selectedKeyframeList.IndexOf(i) < 0)
                        {
                            var tempOffset = tickMax - k.tick + 1;
                            if (offset < tempOffset)
                                offset = tempOffset;
                        }
                    }

                    i = index - 1;
                    k = keyframeList[i];
                    if (!right && k.tick >= tickMin && selectedKeyframeList.IndexOf(i) < 0)
                    {
                        var tempOffset = k.tick - tickMin + 1;
                        if (offset < tempOffset)
                            offset = tempOffset;
                    }
                }

                add -= offset;

                foreach (int index in selectedKeyframeList)
                {
                    // Index = 0の場合移動しない
                    if (index == 0)
                        continue;

                    var currentTick = keyframeList[index].tick;
                    if (right)
                        currentTick += add;
                    else
                        currentTick -= add;

                    keyframeList[index].tick = currentTick;
                }

                // UI更新
                if (selectedKeyframeList.IndexOf(selectedKeyframeIndex) > -1)
                    keyframeUI.UpdateTick(keyframeList[selectedKeyframeIndex].tick);
            }
            else
            {
                // Index = 0の場合移動しない
                if (selectedKeyframeIndex == 0)
                    return;

                var currentTick = keyframeList[selectedKeyframeIndex].tick;
                var tickMax = currentTick + add;
                var tickMin = currentTick - add;

                int i = 0, offset = 0;
                Keyframe k;
                // Tickが被らないようにする
                if (selectedKeyframeIndex != keyframeList.Count - 1)
                {
                    i = selectedKeyframeIndex + 1;
                    k = keyframeList[i];
                    if (right && k.tick <= tickMax && i != selectedKeyframeIndex)
                    {
                        var tempOffset = tickMax - k.tick + 1;
                        if (offset < tempOffset)
                            offset = tempOffset;
                    }
                }
                i = selectedKeyframeIndex - 1;
                k = keyframeList[i];
                if (!right && k.tick >= tickMin && i != selectedKeyframeIndex)
                {
                    var tempOffset = k.tick - tickMin + 1;
                    if (offset < tempOffset)
                        offset = tempOffset;
                }

                add -= offset;

                if (right)
                    currentTick += add;
                else
                    currentTick -= add;

                keyframeList[selectedKeyframeIndex].tick = currentTick;

                // UI更新
                keyframeUI.UpdateTick(keyframeList[selectedKeyframeIndex].tick);
            }

            // キーフレームビュー更新
            UpdateKeyframeView();
        }
    }
}