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

        public void OnFileBarClicked()
        {
            if (fileMenu.activeSelf)
                fileMenu.SetActive(false);
            else
                fileMenu.SetActive(true);
        }

        public void OnSaveProjectClicked()
        {
            sceneManager.SaveProjectFileModel();
            fileMenu.SetActive(false);
        }

        public void OnLoadProjectClicked()
        {
            sceneManager.LoadProjectFileModel();
            fileMenu.SetActive(false);
        }

        public void OnImportJsonClicked()
        {
            nodeManager.SetJsonFilePanelVisible();
            fileMenu.SetActive(false);
        }

        public void OnExportFuncSummonClicked()
        {
            sceneManager.ExportFuncSummon();
            fileMenu.SetActive(false);
        }

        public void OnExportFuncModelClicked()
        {
            sceneManager.ExportFuncModel();
            fileMenu.SetActive(false);
        }
    }
}