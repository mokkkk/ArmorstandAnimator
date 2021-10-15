using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ArmorstandAnimator
{
    public class CurrentFileButton : MonoBehaviour
    {
        private string path;
        private CurrentFile currentFile;

        [SerializeField]
        private Text pathText;

        public void Initialize(CurrentFile currentFile, string path)
        {
            this.currentFile = currentFile;
            this.path = path;

            // テキスト決定
            var name = path.Split('\\');
            // var text = name[name.Length - 1] + $"({path})";
            var text = name[name.Length - 1];
            pathText.text = text;
        }

        public void SelectPath()
        {
            currentFile.SelectPath(this.path);
        }
    }
}