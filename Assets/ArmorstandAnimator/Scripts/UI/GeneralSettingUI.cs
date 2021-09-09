using UnityEngine;
using UnityEngine.UI;

namespace ArmorstandAnimator
{
    public class GeneralSettingUI : MonoBehaviour
    {
        [SerializeField]
        private SceneManager sceneManager;
        [SerializeField]
        private InputField cmdItemID;
        public string CmdItemID
        {
            get
            {
                return this.cmdItemID.text;
            }
        }
        [SerializeField]
        private InputField modelName;
        public string ModelName
        {
            get
            {
                return this.modelName.text;
            }
        }
        [SerializeField]
        private Toggle showArmorstand;
        public bool ShowArmorstand
        {
            get
            {
                return this.showArmorstand.isOn;
            }
        }
        [SerializeField]
        private Toggle showAxis;
        public bool ShowAxis
        {
            get
            {
                return this.showAxis.isOn;
            }
        }

        public void SetText(string itemId, string modelName)
        {
            this.cmdItemID.text = itemId;
            this.modelName.text = modelName;
            Debug.Log(itemId);
        }

        public void OnShowArmorstandChanged()
        {
            sceneManager.ShowArmorstand();
        }

        public void OnShowAxisChanged()
        {
            sceneManager.ShowAxis();
        }
    }
}