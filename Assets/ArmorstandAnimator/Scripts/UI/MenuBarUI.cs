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
        private GameObject fileMenuModel, fileMenuAnim, viewMenu;

        [SerializeField]
        private Text armorstandText, groundText, axisText;
        private bool showArmorstand = true, showGround = true, showAxis = false;

        public void OnFileBarClicked()
        {
            if (fileMenuModel.activeSelf || fileMenuAnim.activeSelf)
                SetFileMenuActive(false);
            else
                SetFileMenuActive(true);
            viewMenu.SetActive(false);
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

        public void OnExportDatapackFSClicked()
        {
            sceneManager.ExportFuncAnimationFs();
            fileMenuAnim.SetActive(false);
        }


        public void OnViewBarClicked()
        {
            if (viewMenu.activeSelf)
                viewMenu.SetActive(false);
            else
                viewMenu.SetActive(true);
            fileMenuModel.SetActive(false);
            fileMenuAnim.SetActive(false);
        }

        public void OnShowArmorstandChanged()
        {
            showArmorstand = !showArmorstand;
            if (showArmorstand)
                armorstandText.text = "防具立て非表示";
            else
                armorstandText.text = "防具立て表示";
            sceneManager.ShowArmorstand(showArmorstand);
            viewMenu.SetActive(false);
        }

        public void OnShowAxisChanged()
        {
            showAxis = !showAxis;
            if (showAxis)
                axisText.text = "回転軸非表示";
            else
                axisText.text = "回転軸表示";
            sceneManager.ShowAxis(showAxis);
            viewMenu.SetActive(false);
        }

        public void OnShowGroundChanged()
        {
            showGround = !showGround;
            if (showGround)
                groundText.text = "地面非表示";
            else
                groundText.text = "地面表示";
            sceneManager.ShowGround(showGround);
            viewMenu.SetActive(false);
        }


        public void OnCurrentFileProjectClicked()
        {
            sceneManager.ShowCurrentFileProject();
            fileMenuModel.SetActive(false);
        }
    }
}