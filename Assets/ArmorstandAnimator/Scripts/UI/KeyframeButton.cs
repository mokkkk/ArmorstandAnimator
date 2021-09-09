using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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

        public void OnButtonClicked()
        {
            animationManager.SelectKeyframe(this.index);
        }
    }
}