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