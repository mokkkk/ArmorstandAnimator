using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace ArmorstandAnimator
{
    public class KeyframeButton : MonoBehaviour
    {
        public AnimationManager animationManager;
        public int index;
        public int tick;
        private Text indexText;

        public void Initialize(int index, int tick, AnimationManager animationManager)
        {
            this.animationManager = animationManager;
            this.index = index;
            this.tick = tick;
            indexText = this.transform.Find("Label").GetComponent<Text>();
            indexText.text = tick.ToString();
        }

        public void SetValue(int index, int tick)
        {
            this.index = index;
            this.tick = tick;
            indexText.text = tick.ToString();
        }

        // ボタン押し判定
        public void OnButtonClicked()
        {
            // LCtrl:複数選択
            if (Input.GetKey(KeyCode.LeftControl))
                animationManager.AddSelectedKeyframe(this.index);
            else
                animationManager.SelectKeyframe(this.index);
        }

        // マウスクリック判定
        public void OnMouseClicked()
        {
            Debug.Log("Start Click");
        }
    }
}