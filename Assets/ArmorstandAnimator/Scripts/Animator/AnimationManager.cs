using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace ArmorstandAnimator
{
    // キーフレームのデータ保存用
    public class Keyframe
    {
        // id
        public int id;
        // tick
        public int tick;
        // Rotation
        public List<Vector3> rotations;
    }

    public class AnimationManager : MonoBehaviour
    {
        [SerializeField]
        private SceneManager sceneManager;
        [SerializeField]
        private List<Keyframe> keyframeList;

        [SerializeField]
        private GameObject animationUIObj;
        [SerializeField]
        private Transform animationUIScrollView;

        // Start is called before the first frame update
        void Start()
        {

        }

        // ノードUIのみ表示
        public void CreateAnimationUI()
        {
            foreach (Node n in sceneManager.NodeList)
            {
                n.CreateUIAnim(animationUIObj, animationUIScrollView);
            }
        }

        // アニメーションファイル読込

        // ノードRotate更新
        public void SetNodeRotation(Keyframe keyframe)
        {
            // キーフレームのノード数とモデルのノード数を比較し，同数の場合のみ実行
            for (int i = 0; i < sceneManager.NodeList.Count; i++)
            {
                sceneManager.NodeList[i].SetRotation(keyframe.rotations[i]);
            }
        }

        // ノード位置決定
        public void SetNodePosition(Node targetNode)
        {
            // 親ノードの位置取得
            var parentPos = Vector3.zero;
            if (!ReferenceEquals(targetNode.parentNode, null))
                parentPos = targetNode.parentNode.transform.position;

            // 親ノードの回転取得
            var parentRotation = Vector3.zero;
            if (!ReferenceEquals(targetNode.parentNode, null))
                parentRotation = targetNode.parentNode.rotate;

            // 位置計算
            var rotatedPos = MatrixRotation.RotationWorld(MatrixRotation.RotationLocal(targetNode.pos, parentRotation), parentRotation);
            rotatedPos += parentPos;

            // 位置更新
            targetNode.transform.position = rotatedPos;

            // 自分の子ノードでSetNodePosition実行
            if (targetNode.childrenNode.Any())
                foreach (Node n in targetNode.childrenNode)
                    SetNodePosition(n);
        }
    }
}