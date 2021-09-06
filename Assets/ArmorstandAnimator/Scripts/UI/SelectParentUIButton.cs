using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ArmorstandAnimator
{
    public class SelectParentUIButton : MonoBehaviour
    {
        public SelectParentUI selectParentUI;
        public Node targetNode;

        [SerializeField]
        private bool isRoot;
        [SerializeField]
        private Button button;

        public void SetNode(Node node, SelectParentUI selectParentUI)
        {
            this.targetNode = node;
            this.selectParentUI = selectParentUI;
            button.transform.Find("Text").GetComponent<Text>().text = node.nodeName;
        }

        public void SetText(string text)
        {
            button.GetComponentInChildren<Text>().text = text;
        }

        // ボタンクリック時
        public void OnClick()
        {
            if (!isRoot)
                selectParentUI.selectedNode = this.targetNode;
            else
                selectParentUI.selectedNode = null;
        }
    }
}