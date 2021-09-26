using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ArmorstandAnimator
{
    public class EventUI : MonoBehaviour
    {
        private KeyframeUI keyframeUI;
        [SerializeField]
        private InputField nameInputField, tickInputField;
        public string Name
        {
            get
            {
                return nameInputField.text;
            }
        }
        public int Tick
        {
            get
            {
                return int.Parse(tickInputField.text);
            }
        }

        public void Initialize(KeyframeUI keyframeUI)
        {
            this.keyframeUI = keyframeUI;
            this.nameInputField.text = "Event";
            this.tickInputField.text = "0";
        }

        public void OnValueChanged()
        {
            keyframeUI.UpdateEventUIList();
        }

        public void DestroyEvents()
        {
            this.transform.parent = transform.root;
            keyframeUI.UpdateEventUIList();
            Destroy(this.gameObject);
        }
    }
}