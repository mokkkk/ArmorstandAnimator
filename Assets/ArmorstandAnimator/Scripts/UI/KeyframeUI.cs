using System.Collections;
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

        // Component
        [SerializeField]
        private Transform animationUIHolder;
        [SerializeField]
        private InputField index;
        [SerializeField]
        private InputField tick;
        [SerializeField]
        private InputField rootPosX, rootPosY, rootPosZ;

        // animationUIList消去
        public void ClearAnimationUIList()
        {
            this.animationUIList = new List<AnimationUI>();
        }

        // animationUIList取得
        public void GetAnimationUIList()
        {
            foreach (Transform child in animationUIHolder)
            {
                animationUIList.Add(child.GetComponent<AnimationUI>());
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

        // public void OnButtonClicked()
        // {
        //     animationManager.SelectKeyframe(this.keyframe);
        // }

        private void SetInputFieldIntaractive(bool value)
        {
            tick.interactable = value;
            rootPosX.interactable = value;
            rootPosY.interactable = value;
            rootPosZ.interactable = value;
        }
    }
}