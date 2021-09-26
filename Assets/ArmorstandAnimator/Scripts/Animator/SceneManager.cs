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
        [SerializeField]
        private AnimationSettingUI animationSetting;

        // メニューバー
        [SerializeField]
        private MenuBarUI menuBarUI;

        // 地面
        [SerializeField]
        private GameObject groundPlane;

        // jsonモデル作成用
        private NodeManager nodeManager;

        // アニメーション作成用
        private AnimationManager animationManager;

        // プロジェクトファイル保存/読込用
        [SerializeField]
        private GameObject warningPanelModel, warningPanelAnim;
        private ProjectFileManager projectFileManager;
        private AnimationFileManager animationFileManager;

        // mcfunction書出用
        private GenerateModelMcfunc modelMcfunc;
        private GenerateAnimationMcfunction animationMcfunc;

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
            animationFileManager = this.gameObject.GetComponent<AnimationFileManager>();
            modelMcfunc = this.gameObject.GetComponent<GenerateModelMcfunc>();
            animationMcfunc = this.gameObject.GetComponent<GenerateAnimationMcfunction>();
        }

        // Update is called once per frame
        void Update()
        {
        }

        // モード変更
        public void ChangeAppMode(Slider slider)
        {
            menuBarUI.HideFileMenu();
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

        public void NewProjectWarning()
        {
            warningPanelModel.SetActive(true);
        }

        public void CreateNewProject(bool value)
        {
            if (value)
            {
                // ノード消去
                foreach (Node n in nodeList)
                {
                    Destroy(n.targetNodeUI.gameObject);
                    Destroy(n.gameObject);
                }

                // リスト初期化
                this.nodeList = new List<Node>();

                // プロジェクト設定更新
                generalSetting.SetText("", "");
            }

            // 警告非表示
            warningPanelModel.SetActive(false);
        }

        public void NewAnimationWarning()
        {
            warningPanelAnim.SetActive(true);
        }

        public void CreateNewAnimation(bool value)
        {
            if (value)
            {
                // Keyframe消去
                animationManager.ClearAnimationUIOnLoad();
                // アニメーション設定更新
                animationSetting.SetText("");
                // UI新規作成
                animationManager.CreateAnimationUI();
            }

            // 警告非表示
            warningPanelAnim.SetActive(false);
        }

        // asamodelproject保存
        public void SaveProjectFileModel()
        {
            projectFileManager.SaveProjectFileModel(generalSetting, nodeList);
        }

        // asaanimproject保存
        public void SaveProjectFileAnim()
        {
            animationFileManager.SaveProjectFileAnim(animationSetting, animationManager.KeyframeList, animationManager.keyframeUI.eventUIList);
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

        // asaanimationproject読込
        public void LoadProjectFileAnim()
        {
            ASAAnimationProject project;
            // プロジェクトファイル読込
            var res = animationFileManager.LoadProjectFileAnim(out project);

            // ファイルを読み込めなかった場合，中断
            if (res < 0)
                return;

            // Keyframe消去
            animationManager.ClearAnimationUIOnLoad();

            // アニメーション設定更新
            animationSetting.SetText(project.animationName);
            // キーフレーム作成
            animationManager.CreateAnimationUIProject(project);
            // イベントリスト作成
            animationManager.keyframeUI.CreateEventUIList(project.events);
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

        // Export animation datapack
        public void ExportFuncAnimation()
        {
            animationMcfunc.GenerateDatapack(generalSetting, animationSetting, NodeList, animationManager.KeyframeList, animationManager.keyframeUI.eventUIList);
        }

        // Export animation datapack (animation only)
        public void ExportFuncOnlyAnimation()
        {
            // animationMcfunc.GenerateDatapackOnlyAnimation(generalSetting, animationSetting, NodeList, animationManager.KeyframeList);
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

        // 地面表示/非表示
        public void ShowGround()
        {
            groundPlane.SetActive(generalSetting.ShowGround);
        }

        // ModelモードUI消去
        private void ClearModelUI()
        {
            // ノードUI消去
            nodeManager.ClearNodeUI();
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
            animationManager.ClearAnimationUI();
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