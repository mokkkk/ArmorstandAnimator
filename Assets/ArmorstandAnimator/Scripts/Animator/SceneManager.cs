using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

namespace ArmorstandAnimator
{
    public enum AppMode
    {
        Model, Animation
    }
    public enum TransformMode
    {
        Position, Rotation
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
        public GeneralSettingUI GeneralSetting
        {
            get
            {
                return this.generalSetting;
            }
        }
        [SerializeField]
        private AnimationSettingUI animationSetting;

        // メニューバー
        [SerializeField]
        private MenuBarUI menuBarUI;

        // マウス用
        [SerializeField]
        private MouseManager mouseManager;

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

        // 最近使用したファイル用
        [SerializeField]
        private GameObject currentFileProjectPanel, currentFileAnimPanel;

        // mcfunction書出用
        private GenerateModelMcfunc modelMcfunc;
        private GenerateAnimationMcfunction animationMcfunc;
        private GenerateAnimationMcfunctionFixSpeed animationMcfuncfs;

        // 表示設定用
        public bool showGround = true, showArmorstand = true, showAxis = false;

        // ノード操作用
        public TransformMode transformMode;
        [SerializeField]
        private Transform transformUI, positionUI, rotationUI, rotationUIPose01;
        public Node currentNode, currentNodeBefore;

        private const string PathHistoryFileNameProject = "pathhist_project.json";
        private const string PathHistoryFileNameAnim = "pathhist_animation.json";

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
            animationMcfuncfs = this.gameObject.GetComponent<GenerateAnimationMcfunctionFixSpeed>();
            mouseManager = this.gameObject.GetComponent<MouseManager>();
            mouseManager.Initialize();

            showGround = true;
            showArmorstand = true;
            showAxis = false;
            currentNode = null;
            currentNodeBefore = null;

            transformMode = TransformMode.Rotation;
        }

        // Update is called once per frame
        void Update()
        {
            mouseManager.Main();

            if (!ReferenceEquals(currentNode, currentNodeBefore))
                OnCurrentNodeChanged();
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
                generalSetting.SetText("", "", false, true, false, 99);
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
            generalSetting.SetText(project.itemID, project.modelName, project.multiEntities, project.isMarker, project.isSmall, project.fileVersion);
            // ノード作成
            nodeManager.CreateNodeProject(project.nodeList);
        }

        // asaanimationproject読込
        public void LoadProjectFileAnim()
        {
            ASAAnimationProject project;
            // プロジェクトファイル読込
            var res = animationFileManager.SelectPath(out project);

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
            if (ReferenceEquals(animationManager.keyframeUI.eventUIList, null))
                Debug.Log("Null");
            animationMcfunc.GenerateDatapack(generalSetting, animationSetting, NodeList, animationManager.KeyframeList, animationManager.keyframeUI.eventUIList);
        }

        public void ExportFuncAnimationFs()
        {
            animationMcfuncfs.GenerateDatapack(generalSetting, animationSetting, NodeList, animationManager.KeyframeList);
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
        public void ShowArmorstand(bool showArmorstand)
        {
            this.showArmorstand = showArmorstand;
            foreach (Node n in NodeList)
            {
                n.SetArmorstandVisible(showArmorstand, generalSetting.IsSmall);
            }
            ShowAxis(this.showAxis);
        }

        // 回転軸表示/非表示
        public void ShowAxis(bool showAxis)
        {
            this.showAxis = showAxis;
            foreach (Node n in NodeList)
            {
                n.SetCubeMaterial(showAxis);
                n.SetAxisVisible(false);

                if (ReferenceEquals(n, currentNode))
                    n.SetAxisVisible(showAxis);
            }
        }

        public void SetCurrentNode(Node node)
        {
            this.currentNode = node;
            if (showAxis)
            {
                foreach (Node n in NodeList)
                {
                    n.SetAxisVisible(false);

                    if (ReferenceEquals(n, currentNode))
                        n.SetAxisVisible(showAxis);
                }
            }
        }

        // 地面表示/非表示
        public void ShowGround(bool showGround)
        {
            this.showGround = showGround;
            groundPlane.SetActive(showGround);
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

        // Armorstand Small 0b/1b 切替
        public void ChangeArmorstand()
        {
            var project = projectFileManager.SaveProjectFileModelReturn(generalSetting, nodeList);

            // ノード消去
            foreach (Node n in nodeList)
            {
                Destroy(n.targetNodeUI.gameObject);
                Destroy(n.gameObject);
            }

            // リスト初期化
            this.nodeList = new List<Node>();

            // プロジェクト設定更新
            generalSetting.SetText(project.itemID, project.modelName, project.multiEntities, project.isMarker, project.isSmall, 99);
            // ノード作成
            nodeManager.ChangeArmorstand(project.nodeList, project.isSmall);

            // Armorstand表示設定
            ShowArmorstand(showArmorstand);
        }


        // 最近使用したプロジェクトファイル
        public void ShowCurrentFileProject()
        {
            var histPath = Path.Combine(Application.persistentDataPath, PathHistoryFileNameProject);

            var jsonLine = new ASAPathHistory();
            if (File.Exists(histPath))
            {
                // ファイル読み込み
                string line;
                System.IO.StreamReader file =
                    new System.IO.StreamReader(histPath);
                line = file.ReadLine();
                jsonLine = JsonUtility.FromJson<ASAPathHistory>(line);
                file.Close();
            }
            else
            {
                jsonLine.paths = new string[0];
            }

            currentFileProjectPanel.SetActive(true);
            currentFileProjectPanel.GetComponent<CurrentFileProject>().Initialize(jsonLine.paths);
        }

        // asamodelproject読込
        public void LoadProjectFileModelCurrent(string path)
        {
            ASAModelProject project;
            // プロジェクトファイル読込
            var res = projectFileManager.LoadProjectFileModelCurrent(path, out project);

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
            generalSetting.SetText(project.itemID, project.modelName, project.multiEntities, project.isMarker, project.isSmall, project.fileVersion);
            // ノード作成
            nodeManager.CreateNodeProject(project.nodeList);
        }

        // 最近使用したアニメーションファイル
        public void ShowCurrentFileAnimation()
        {
            var histPath = Path.Combine(Application.persistentDataPath, PathHistoryFileNameAnim);

            var jsonLine = new ASAPathHistory();
            if (File.Exists(histPath))
            {
                // ファイル読み込み
                string line;
                System.IO.StreamReader file =
                    new System.IO.StreamReader(histPath);
                line = file.ReadLine();
                jsonLine = JsonUtility.FromJson<ASAPathHistory>(line);
                file.Close();
            }
            else
            {
                jsonLine.paths = new string[0];
            }

            currentFileAnimPanel.SetActive(true);
            currentFileAnimPanel.GetComponent<CurrentFileAnim>().Initialize(jsonLine.paths);
        }

        // asaanimationproject読込
        public void LoadProjectFileAnimCurrent(string path)
        {
            ASAAnimationProject project;
            // プロジェクトファイル読込
            var res = animationFileManager.LoadProjectFileAnimCurrent(path, out project);

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

        // CurrentNode変更時
        private void OnCurrentNodeChanged()
        {
            Debug.Log("On Change");
            if (!ReferenceEquals(currentNodeBefore, null))
                currentNodeBefore.SetCubeMaterialTarget(false);
            if (!ReferenceEquals(currentNode, null))
            {
                SetUIHandleVisible(true);
                currentNode.SetCubeMaterialTarget(true);
            }
            else
            {
                SetUIHandleVisible(false);
            }
            this.currentNodeBefore = currentNode;
        }

        // ハンドル表示切替
        private void SetUIHandleVisible(bool visible)
        {
            if (visible && !ReferenceEquals(currentNode, null))
            {
                transformUI.gameObject.SetActive(true);
                transformUI.position = currentNode.transform.position;
                if (transformMode == TransformMode.Rotation)
                {
                    positionUI.gameObject.SetActive(false);
                    rotationUI.gameObject.SetActive(true);
                    rotationUIPose01.localEulerAngles = currentNode.pose2.transform.localEulerAngles;
                }
                else if (transformMode == TransformMode.Position)
                {
                    positionUI.gameObject.SetActive(true);
                    rotationUI.gameObject.SetActive(false);
                }
            }
            else
            {
                transformUI.gameObject.SetActive(false);
            }
        }

        // 移動用ハンドル表示切替
        public void MouseNodePositionColor(Axis axis, bool reset)
        {
            if (reset)
            {
                positionUI.Find("X").GetComponent<LineRenderer>().startColor = Color.red;
                positionUI.Find("Y").GetComponent<LineRenderer>().startColor = Color.green;
                positionUI.Find("Z").GetComponent<LineRenderer>().startColor = Color.blue;
                positionUI.Find("X").GetComponent<LineRenderer>().endColor = Color.red;
                positionUI.Find("Y").GetComponent<LineRenderer>().endColor = Color.green;
                positionUI.Find("Z").GetComponent<LineRenderer>().endColor = Color.blue;
            }
            else
            {
                if (axis == Axis.X)
                {
                    positionUI.Find("X").GetComponent<LineRenderer>().startColor = Color.white;
                    positionUI.Find("X").GetComponent<LineRenderer>().endColor = Color.white;
                }
                if (axis == Axis.Y)
                {
                    positionUI.Find("Y").GetComponent<LineRenderer>().startColor = Color.white;
                    positionUI.Find("Y").GetComponent<LineRenderer>().endColor = Color.white;
                }
                if (axis == Axis.Z)
                {
                    positionUI.Find("Z").GetComponent<LineRenderer>().startColor = Color.white;
                    positionUI.Find("Z").GetComponent<LineRenderer>().endColor = Color.white;
                }
            }
        }

        // 回転用ハンドル表示切替
        public void MouseNodeRotationColor(Axis axis, bool reset)
        {
            if (reset)
            {
                rotationUIPose01.Find("X").GetComponent<LineRenderer>().startColor = Color.red;
                rotationUIPose01.Find("Y").GetComponent<LineRenderer>().startColor = Color.green;
                rotationUI.Find("Z").GetComponent<LineRenderer>().startColor = Color.blue;
                rotationUIPose01.Find("X").GetComponent<LineRenderer>().endColor = Color.red;
                rotationUIPose01.Find("Y").GetComponent<LineRenderer>().endColor = Color.green;
                rotationUI.Find("Z").GetComponent<LineRenderer>().endColor = Color.blue;
            }
            else
            {
                if (axis == Axis.X)
                {
                    rotationUIPose01.Find("X").GetComponent<LineRenderer>().startColor = Color.white;
                    rotationUIPose01.Find("X").GetComponent<LineRenderer>().endColor = Color.white;
                }
                if (axis == Axis.Y)
                {
                    rotationUIPose01.Find("Y").GetComponent<LineRenderer>().startColor = Color.white;
                    rotationUIPose01.Find("Y").GetComponent<LineRenderer>().endColor = Color.white;
                }
                if (axis == Axis.Z)
                {
                    rotationUI.Find("Z").GetComponent<LineRenderer>().startColor = Color.white;
                    rotationUI.Find("Z").GetComponent<LineRenderer>().endColor = Color.white;
                }
            }
        }

        // マウスによるノード移動
        public void MouseNodePosition(Vector3 pos)
        {
            // Nodeの値設定
            if (appMode == AppMode.Model)
                currentNode.targetNodeUI.UpdatePosition(pos);
            // else if (appMode == AppMode.Animation)
            //     currentNode.targetAnimationUI.UpdateRotate(pos);
        }

        // マウスによるノード回転
        public void MouseNodeRotation(Axis axis, float deg)
        {
            // 角度取得
            var newRotate = new Vector3(currentNode.rotate.x, currentNode.rotate.y, currentNode.rotate.z);
            if (axis == Axis.X)
                newRotate.x = deg;
            if (axis == Axis.Y)
                newRotate.y = deg;
            if (axis == Axis.Z)
                newRotate.z = deg;
            currentNode.rotate = newRotate;
            // 回転軸更新
            rotationUIPose01.localEulerAngles = currentNode.pose2.transform.localEulerAngles;

            // Nodeの値設定
            if (appMode == AppMode.Model)
                currentNode.targetNodeUI.UpdateRotate(newRotate);
            else if (appMode == AppMode.Animation)
                currentNode.targetAnimationUI.UpdateRotate(newRotate);
        }
    }
}