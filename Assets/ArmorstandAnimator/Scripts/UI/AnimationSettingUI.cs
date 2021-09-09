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
        public string AnimationName
        {
            get
            {
                return this.animationName.text;
            }
        }
        public void SetText(string animationName)
        {
            this.animationName.text = animationName;
        }
    }
}