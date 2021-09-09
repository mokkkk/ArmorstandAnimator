using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ArmorstandAnimator
{
    public enum AppMode
    {
        Model, Animation
    }

    public class SceneManager : MonoBehaviour
    {
        // 現在のモード
        public AppMode appMode;

        // 各モードのUI
        [SerializeField]
        private GameObject modelModeUI, animModeUI;

        // 全ノードリスト
        private List<Node> nodeList;
        public List<Node> NodeList
        {
            get
            {
                return this.nodeList;
            }
        }

        // 一般設定
        [SerializeField]
        private GeneralSettingUI generalSetting;

        // jsonモデル作成用
        private NodeManager nodeManager;

        // アニメーション作成用
        private AnimationManager animationManager;

        // プロジェクトファイル保存/読込用
        private ProjectFileManager projectFileManager;

        // mcfunction書出用
        private GenerateModelMcfunc modelMcfunc;

        // Start is called before the first frame update
        void Start()
        {
            // 初期化
            nodeList = new List<Node>();

            // Component取得
            nodeManager = this.gameObject.GetComponent<NodeManager>();
            nodeManager.Initialize();
            animationManager = this.gameObject.GetComponent<AnimationManager>();
            projectFileManager = this.gameObject.GetComponent<ProjectFileManager>();
            modelMcfunc = this.gameObject.GetComponent<GenerateModelMcfunc>();

            // ノード追加(テスト用)
            // string[] testPath = { "C:\\Users\\KawashimaLab\\Desktop\\test.json" };
            // nodeManager.CreateNode("Hoge", 0, testPath);
            // nodeManager.CreateNode("Fuga", 1, testPath);
            // nodeManager.CreateNode("Piyo", 2, testPath);
        }

        // Update is called once per frame
        void Update()
        {

        }

        // モード変更
        public void ChangeAppMode(Slider slider)
        {
            var value = slider.value;
            if (value == 0)
            {
                this.appMode = AppMode.Model;
                ClearAnimUI();
                CreateModelUI();
            }
            else if (value == 1)
            {
                this.appMode = AppMode.Animation;
                ClearModelUI();
                CreateAnimUI();
            }
        }

        // asamodelproject保存
        public void SaveProjectFileModel()
        {
            projectFileManager.SaveProjectFileModel(generalSetting, nodeList);
        }

        // asamodelproject読込
        public void LoadProjectFileModel()
        {
            ASAModelProject project;
            // プロジェクトファイル読込
            var res = projectFileManager.LoadProjectFileModel(out project);

            // ファイルを読み込めなかった場合，中断
            if (res < 0)
                return;

            // ノード消去
            foreach (Node n in nodeList)
            {
                Destroy(n.targetNodeUI.gameObject);
                Destroy(n.gameObject);
            }

            // リスト初期化
            this.nodeList = new List<Node>();

            // プロジェクト設定更新
            generalSetting.SetText(project.itemID, project.modelName);
            // ノード作成
            nodeManager.CreateNodeProject(project.nodeList);
        }

        // Export summon function
        public void ExportFuncSummon()
        {
            modelMcfunc.GenerateSummonFunction(generalSetting, nodeList);
        }

        // Export model function
        public void ExportFuncModel()
        {
            modelMcfunc.GenerateModelFunction(generalSetting, nodeList);
        }

        // Node追加
        public void AddNode(Node node)
        {
            this.nodeList.Add(node);
        }

        // Node削除
        public void RemoveNode(Node node)
        {
            this.nodeList.Remove(node);
            // NodeID再割り当て
            int i = 0;
            foreach (Node n in nodeList)
            {
                n.nodeID = i;
                i++;
            }
        }

        // 防具立て表示/非表示
        public void ShowArmorstand()
        {
            foreach (Node n in NodeList)
            {
                n.SetArmorstandVisible(generalSetting.ShowArmorstand);
            }
        }

        // 防具立て表示/非表示
        public void ShowAxis()
        {
            foreach (Node n in NodeList)
            {
                n.SetAxisVisible(generalSetting.ShowAxis);
            }
        }

        // ModelモードUI消去
        private void ClearModelUI()
        {
            // ノードUI消去
            foreach (Node n in nodeList)
            {
                Destroy(n.targetNodeUI.gameObject);
            }
            // UI非表示
            modelModeUI.SetActive(false);
        }

        // ModelモードUI作成
        private void CreateModelUI()
        {
            // UI表示
            modelModeUI.SetActive(true);
            // ノードUI表示
            nodeManager.CreateNodeUI();
        }

        // AnimationモードUI消去
        private void ClearAnimUI()
        {
            // アニメーションUI消去
            foreach (Node n in nodeList)
            {
                Destroy(n.targetAnimationUI.gameObject);
            }
            // UI非表示
            animModeUI.SetActive(false);
        }

        // AnimationモードUI作成
        private void CreateAnimUI()
        {
            // UI表示
            animModeUI.SetActive(true);
            // アニメーションUI表示
            animationManager.CreateAnimationUI();
        }
    }
}