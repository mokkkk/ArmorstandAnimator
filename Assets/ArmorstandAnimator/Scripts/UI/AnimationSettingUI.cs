using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ArmorstandAnimator
{
    public class AnimationSettingUI : MonoBehaviour
    {
        [SerializeField]
        private InputField animationName;
        [SerializeField]
        private InputField animationSpeed;
        [SerializeField]
        private Toggle isTickAnimation;
        public string AnimationName
        {
            get
            {
                return this.animationName.text;
            }
        }
        public float AnimationSpeed
        {
            get
            {
                return float.Parse(this.animationSpeed.text);
            }
        }
        public bool IsTickAnimation
        {
            get
            {
                return this.isTickAnimation.isOn;
            }
        }
        public void SetText(string animationName)
        {
            this.animationName.text = animationName;
        }

        public void SetSpeed(float animationSpeed)
        {
            this.animationSpeed.text = animationSpeed.ToString();
        }
    }
}