using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ArmorstandAnimator
{
    public class SelectParentUI : MonoBehaviour
    {
        public Node targetNode;
        public Node selectedNode;

        // UI作成用
        [SerializeField]
        private GameObject nodeButton;

        [SerializeField]
        private Transform content;
        [SerializeField]
        private GameObject rootButton;

        // ボタン表示
        public void AddButton(Node node)
        {
            var obj = Instantiate(nodeButton, Vector3.zero, Quaternion.identity, content);

            obj.GetComponent<SelectParentUIButton>().SetNode(node, this);
        }

        // ボタン消去
        public void DeleteUI()
        {
            foreach (Transform child in content)
            {
                if (child.gameObject != rootButton)
                    Destroy(child.gameObject);
            }
        }
    }
}