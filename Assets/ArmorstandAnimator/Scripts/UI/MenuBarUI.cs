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
        private GameObject fileMenuModel, fileMenuAnim;

        public void OnFileBarClicked()
        {
            if (fileMenuModel.activeSelf || fileMenuAnim.activeSelf)
                SetFileMenuActive(false);
            else
                SetFileMenuActive(true);
        }

        public void HideFileMenu()
        {
            SetFileMenuActive(false);
        }

        void SetFileMenuActive(bool value)
        {
            if (sceneManager.appMode == AppMode.Model)
                fileMenuModel.SetActive(value);
            else
                fileMenuAnim.SetActive(value);
        }

        public void OnNewProjectClicked()
        {
            sceneManager.NewProjectWarning();
            fileMenuModel.SetActive(false);
        }

        public void OnSaveProjectClicked()
        {
            sceneManager.SaveProjectFileModel();
            fileMenuModel.SetActive(false);
        }

        public void OnLoadProjectClicked()
        {
            sceneManager.LoadProjectFileModel();
            fileMenuModel.SetActive(false);
        }

        public void OnImportJsonClicked()
        {
            nodeManager.SetJsonFilePanelVisible();
            fileMenuModel.SetActive(false);
        }

        public void OnExportFuncSummonClicked()
        {
            sceneManager.ExportFuncSummon();
            fileMenuModel.SetActive(false);
        }

        public void OnExportFuncModelClicked()
        {
            sceneManager.ExportFuncModel();
            fileMenuModel.SetActive(false);
        }

        public void OnNewAnimationClicked()
        {
            sceneManager.NewAnimationWarning();
            fileMenuAnim.SetActive(false);
        }

        public void OnSaveAnimationClicked()
        {
            sceneManager.SaveProjectFileAnim();
            fileMenuAnim.SetActive(false);
        }

        public void OnLoadAnimationClicked()
        {
            sceneManager.LoadProjectFileAnim();
            fileMenuAnim.SetActive(false);
        }

        public void OnExportDatapackClicked()
        {
            sceneManager.ExportFuncAnimation();
            fileMenuAnim.SetActive(false);
        }

        public void OnExportAnimationClicked()
        {
            sceneManager.ExportFuncOnlyAnimation();
            fileMenuAnim.SetActive(false);
        }
    }
}