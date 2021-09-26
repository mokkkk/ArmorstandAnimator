using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ArmorstandAnimator
{
    public class KeyframeUI : MonoBehaviour
    {
        // AnimationManager
        [SerializeField]
        private AnimationManager animationManager;
        // 対応Keyframe
        public Keyframe targetKeyframe;

        // UIList
        public List<AnimationUI> animationUIList;
        public List<EventUI> eventUIList;

        // UIPanel
        [SerializeField]
        private GameObject KeyframeUIPanel;
        [SerializeField]
        private GameObject eventUIPanel;

        // Component
        [SerializeField]
        private Transform animationUIHolder;
        [SerializeField]
        private InputField index;
        [SerializeField]
        private InputField tick;
        [SerializeField]
        private InputField rootPosX, rootPosY, rootPosZ;
        [SerializeField]
        private Button deleteButton;

        [SerializeField]
        private Text tabText;
        [SerializeField]
        private GameObject eventUIObj;
        [SerializeField]
        private Transform eventUIHolder;

        // animationUIList消去
        public void ClearAnimationUIList()
        {
            this.animationUIList = new List<AnimationUI>();
        }

        // eventUIList消去
        public void ClearEventUIList()
        {
            this.eventUIList = new List<EventUI>();
        }

        // UI表示切替
        public void ChangeUI(Slider slider)
        {
            var value = slider.value;
            if (value == 0)
            {
                tabText.text = "キーフレーム設定";
                KeyframeUIPanel.SetActive(true);
                eventUIPanel.SetActive(false);
            }
            else
            {
                tabText.text = "イベント設定";
                KeyframeUIPanel.SetActive(false);
                eventUIPanel.SetActive(true);
            }
        }

        // UI更新
        public void SetUIContent(Keyframe keyframe)
        {
            // UIの内容をキーフレームから設定
            this.targetKeyframe = keyframe;
            index.text = keyframe.index.ToString();
            tick.text = keyframe.tick.ToString();
            rootPosX.text = keyframe.rootPos.x.ToString();
            rootPosY.text = keyframe.rootPos.y.ToString();
            rootPosZ.text = keyframe.rootPos.z.ToString();
            for (int i = 0; i < animationUIList.Count; i++)
            {
                animationUIList[i].SetRotate(keyframe.rotations[i]);
            }
            // tick=0の時，tickおよびrootPosを変更できなくする
            if (keyframe.tick == 0)
                SetInputFieldIntaractive(false);
            else
                SetInputFieldIntaractive(true);
        }

        // 対応Keyframe更新
        public void UpdateKeyframe()
        {
            // UIの値からキーフレームの値を設定
            var id = targetKeyframe.index;
            var tick = int.Parse(this.tick.text);
            var rootPos = new Vector3(float.Parse(rootPosX.text), float.Parse(rootPosY.text), float.Parse(rootPosZ.text));
            var rotations = new List<Vector3>();
            foreach (AnimationUI ui in animationUIList)
            {
                rotations.Add(ui.GetRotate());
            }
            var newKeyframe = new Keyframe(id, tick, rootPos, rotations);

            // AnimationManagerのキーフレームリストを更新
            var newIndex = animationManager.UpdateKeyframeList(newKeyframe);
            newKeyframe.index = newIndex;
            this.targetKeyframe = newKeyframe;

            // UIの内容更新
            SetUIContent(newKeyframe);
        }

        // 対応Keyframe更新
        public void UpdateKeyframeTick()
        {
            // UIの値からキーフレームの値を設定
            var id = targetKeyframe.index;
            var tick = int.Parse(this.tick.text);
            var rootPos = new Vector3(float.Parse(rootPosX.text), float.Parse(rootPosY.text), float.Parse(rootPosZ.text));
            var rotations = new List<Vector3>();
            foreach (AnimationUI ui in animationUIList)
            {
                rotations.Add(ui.GetRotate());
            }
            var newKeyframe = new Keyframe(id, tick, rootPos, rotations);

            // AnimationManagerのキーフレームリストを更新
            var newIndex = animationManager.UpdateKeyframeListTick(newKeyframe);
            newKeyframe.index = newIndex;
            this.targetKeyframe = newKeyframe;

            // UIの内容更新
            SetUIContent(newKeyframe);
        }

        // EventUI追加
        public void CreateEvent()
        {
            var eventUI = Instantiate(eventUIObj, Vector3.zero, Quaternion.identity, eventUIHolder);
            eventUI.GetComponent<EventUI>().Initialize(this);
            UpdateEventUIList();
        }

        // EventUIリスト更新
        public void UpdateEventUIList()
        {
            this.eventUIList = new List<EventUI>();
            foreach (Transform t in eventUIHolder)
            {
                eventUIList.Add(t.GetComponent<EventUI>());
                // t.transform.parent = transform.root;
            }

            // tick昇順でソート
            var orderList = eventUIList.OrderBy(d => d.Tick);
            eventUIList = new List<EventUI>();

            int i = 0;
            foreach (EventUI k in orderList)
            {
                eventUIList.Add(k);
                // k.transform.parent = eventUIHolder;
                k.transform.SetSiblingIndex(i);
                i++;
            }
        }

        private void SetInputFieldIntaractive(bool value)
        {
            tick.interactable = value;
            rootPosX.interactable = value;
            rootPosY.interactable = value;
            rootPosZ.interactable = value;
            deleteButton.interactable = value;
        }
    }
}