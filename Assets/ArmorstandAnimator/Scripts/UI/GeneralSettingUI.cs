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
        [SerializeField]
        private Toggle showGround;
        public bool ShowGround
        {
            get
            {
                return this.showGround.isOn;
            }
        }

        [SerializeField]
        private Toggle multiEntities;
        public bool MultiEntities
        {
            get
            {
                return this.multiEntities.isOn;
            }
        }
        [SerializeField]
        private Toggle isMarker;
        public bool IsMarker
        {
            get
            {
                return this.isMarker.isOn;
            }
        }
        [SerializeField]
        private Toggle isSmall;
        public bool IsSmall
        {
            get
            {
                return this.isSmall.isOn;
            }
        }

        public void SetText(string itemId, string modelName, bool multiEntities, bool isMarker, bool isSmall, int fileVersion)
        {
            this.cmdItemID.text = itemId;
            this.modelName.text = modelName;
            this.multiEntities.isOn = multiEntities;
            this.isMarker.isOn = isMarker;
            if (fileVersion > 1)
            {
                this.isSmall.isOn = isSmall;
                this.isSmall.interactable = true;
            }
            else
            {
                this.isSmall.isOn = false;
                this.isSmall.interactable = false;
            }
        }

        public void OnShowArmorstandChanged()
        {
            sceneManager.ShowArmorstand();
        }

        public void OnShowAxisChanged()
        {
            sceneManager.ShowAxis();
        }

        public void OnShowGroundChanged()
        {
            sceneManager.ShowGround();
        }

        public void OnIsSmallChanged()
        {
            sceneManager.ChangeArmorstand();
        }
    }
}