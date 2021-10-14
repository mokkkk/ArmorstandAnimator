using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ArmorstandAnimator
{
    public class CurrentFileProject : MonoBehaviour, CurrentFile
    {
        [SerializeField]
        private GameObject currentPathButton;
        [SerializeField]
        private Transform content;

        private string selectedPath;

        [SerializeField]
        private SceneManager sceneManager;

        public void Initialize(string[] paths)
        {
            foreach (string path in paths)
            {
                var button = Instantiate(currentPathButton, Vector3.zero, Quaternion.identity, content);
                button.GetComponent<CurrentFileButton>().Initialize(this, path);
            }
        }

        public void SelectPath(string path)
        {
            this.selectedPath = path;
        }

        public void DecidePath()
        {
            if (!System.IO.File.Exists(selectedPath))
            {
                Debug.Log(selectedPath);
                return;
            }

            foreach (Transform t in content)
            {
                Destroy(t.gameObject);
            }
            sceneManager.LoadProjectFileModelCurrent(selectedPath);
            this.gameObject.SetActive(false);
        }

        public void Cancel()
        {
            foreach (Transform t in content)
            {
                Destroy(t.gameObject);
            }
            this.gameObject.SetActive(false);
        }
    }
}