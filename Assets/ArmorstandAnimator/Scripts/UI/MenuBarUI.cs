using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace ArmorstandAnimator
{
    public class MenuBarUI : MonoBehaviour
    {
        [SerializeField]
        private SceneManager sceneManager;
        [SerializeField]
        private NodeManager nodeManager;

        [SerializeField]
        private Button fileButton;
        [SerializeField]
        private GameObject fileMenu;

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            // var selectedObj = EventSystem.current.currentSelectedGameObject;

            // // Fileメニュー表示
            // if (selectedObj == fileButton.gameObject)
            //     fileMenu.SetActive(true);
            // else
            //     fileMenu.SetActive(false);
        }

        public void OnFileBarClicked()
        {
            if (fileMenu.activeSelf)
                fileMenu.SetActive(false);
            else
                fileMenu.SetActive(true);
        }

        public void OnImportJsonClicked()
        {
            nodeManager.SetJsonFilePanelVisible();

            fileMenu.SetActive(false);
        }
    }
}