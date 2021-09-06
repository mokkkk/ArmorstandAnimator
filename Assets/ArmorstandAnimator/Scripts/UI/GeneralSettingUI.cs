using UnityEngine;
using UnityEngine.UI;

namespace ArmorstandAnimator
{
    public class GeneralSettingUI : MonoBehaviour
    {
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

        public void SetText(string itemId, string modelName)
        {
            this.cmdItemID.text = itemId;
            this.modelName.text = modelName;
            Debug.Log(itemId);
        }
    }
}