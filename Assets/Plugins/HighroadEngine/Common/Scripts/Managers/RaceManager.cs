using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using MoreMountains.Tools;
using UnityEngine.Events;

namespace MoreMountains.HighroadEngine
{
    /// <summary>
    /// 竞选经理。类负责汽车实例化，相机，排名和UI。
    /// 用于比赛初始化的Start()方法和用于游戏管理的Update()方法。
    /// </summary>
    public class RaceManager : MonoBehaviour
    {
        [Header("Start positions")]
        [MMInformation("Set the size of the <b>Startpositions</b>, then position the Vector3 using either the inspector or by moving the handles directly in scene view. The order of the array will be the order the car positions.\n", MMInformationAttribute.InformationType.Info, false)]
        ///起始位置列表
        public Vector3[] StartPositions; /* 这里需要在游戏开始前获取玩家的数量，自动去排序*/
        [Range(0, 359)]
        public int StartAngleDegree; //旋转角度
        ///如果是这样，人类玩家将在开始时被放置在机器人之后
        public bool BotsFirstInStartingLine = true; //玩家放在机器人后面，但是里面是没有玩家的，全是机器人自动跑

        [Header("Camera")]
        [MMInformation("如果勾选，提供下面的摄像机列表(类型为CameraController)，玩家可以在其中切换。\n", MMInformationAttribute.InformationType.Info, false)]
        public bool DynamicCameras;
        ///玩家可以使用的摄像机列表
        public CameraController[] CameraControllers;

        public List<CarAIControl> exitCars;

        [Header("UI")]
        ///显示开始倒计时的文本对象
        public Text StartGameCountdown;
        /// 游戏结束时显示面板
        public RectTransform EndGamePanel;
        /// 返回按钮返回大厅屏幕
        public Button BackButton;
        /// 返回按钮，当比赛结束时返回大厅屏幕
        public Button BackToMenuButton;
        /// 有多个摄像头时切换摄像头按钮
        public Button CameraChangeButton;
        /// 按钮重生玩家的车辆
        public Button RespawnButton;

        public GameObject scoreBG;
        /// 当前游戏得分
        public Text ScoreText1;
        public Text ScoreText2;
        public Text ScoreText3;

        public Text countDowdText;

        public GameObject finalRankGridPrefab;

        public Sprite[] finalRankGridPrefabImgs;

        private Transform finalRankFather;

        private bool everOneFinished = false;


        [Header("Playing options")]
        /// 如果错误，最后一个检查点是比赛的终点，就像一个拉力赛
        public bool ClosedLoopTrack = true;
        /// 如果为真，当第一个玩家完成比赛时比赛结束
        public bool FirstFinisherEndsRace = true;
        /// 胜利的圈数
        public int Laps = 3;
        [MMInformation("In network mode, Start Game Countdown must be at least 2 seconds to avoid incorrect synchronization.\n", MMInformationAttribute.InformationType.Info, false)]
        ///比赛开始前的倒计时。 
        public int StartGameCountDownTime = 3;
        [MMInformation("If set to true, cars won't collide themselves. Please note that this value is overrided in network mode by the NetworkRaceManager class.\n", MMInformationAttribute.InformationType.Info, false)]
        /// 碰撞在网络游戏中是否活跃
        public bool NoCollisions = false;

        [Header("跟踪配置")]
        [MMInformation("将游戏对象添加到<b>检查点</b>(先设置大小)，然后使用检查器或直接在场景视图中移动手柄来定位检查点对象。数组的顺序将是检查点通过的顺序。\n", MMInformationAttribute.InformationType.Info, false)]

        //public GameObject[] Checkpoints;

        /// 引用AI Waypoints对象
        public GameObject AIWaypoints;

        [Header("Test Mode")]
        /// 机器人玩家车辆游戏对象列表
        public GameObject[] TestBotPlayers;
        /// 车辆游戏对象的字典，使用玩家号码作为索引
        public Dictionary<int, CarAIControl> Players { get; protected set; }

        public List<IPlayerInfo> PlayersInfo = new List<IPlayerInfo>();

        /// 当前竞赛运行时间。用于排名
        protected float _currentGameTime;
        protected float _currentEndRaceTime;

        public List<double> _currentPlayersEndRaceTime = new List<double>();
        protected int _currentCamera;
        /// 我们目前可以使用的相机控制器的子列表。例如，我们在多局部模式中移除单人摄像机
        protected CameraController[] _cameraControllersAvailable;
        protected IGenericLobbyManager _lobbyManager;
        public bool _isPlaying;
        protected bool _testMode = false;
        protected int _finisherBonusScore = 10000;
        protected int _currentFinisherRank = 1;

        // 当游戏在线时，下面的动作用于将行为委托给networkrace类
        //public UnityAction OnDisableControlForPlayers;
        //public UnityAction<BaseController> OnDisableControlForPlayer;
        //public UnityAction OnEnableControlForPlayers;
        public UnityAction<string> OnUpdateCountdownText;
        public UnityAction OnShowEndGameScreen;
        public delegate List<CarAIControl> OnUpdatePlayersListDelegate();
        public OnUpdatePlayersListDelegate OnUpdatePlayersList;

        public static bool whitchGameMod = false;//为真则是组队赛，否是个人赛

        /// <summary>
        /// 我们检查RaceManager对象的初始化是否正确
        /// </summary>
        public virtual void Awake()
        {
            //if (Checkpoints.Length == 0)
            //{
            //    Debug.LogWarning("检查点列表应该在RaceManager游戏对象检查器中初始化。");
            //}

            if (StartPositions.Length == 0)
            {
                Debug.LogWarning("startposition列表为空。您应该提供至少一个起始位置。");
            }

            if (!ClosedLoopTrack && Laps > 1)
            {
                Debug.LogWarning("跟踪是开放的，不循环。圈值为&gt;1在RaceManager检查器中，将被忽略");
            }

            //帮助iOS帧率
            Application.targetFrameRate = 300;
        }

        
        /// <summary>
        ///我们初始化比赛
        /// </summary>
        public virtual void Start()
        {
            _isPlaying = false;

            if (EndGamePanel != null)
            {
                EndGamePanel.gameObject.SetActive(false);
            }

            if (CameraChangeButton != null)
            {
                CameraChangeButton.onClick.RemoveAllListeners();
                CameraChangeButton.onClick.AddListener(OnCameraChange);
            }

            if (ScoreText1 != null)
            {
                ScoreText1.text = "";
            }

            if (ScoreText2 != null)
            {
                ScoreText2.text = "";
            }

            if (ScoreText3 != null)
            {
                ScoreText3.text = "";
            }

            if (RespawnButton != null)
            {
                RespawnButton.gameObject.SetActive(false);
            }

            if(EndGamePanel != null)
            {
                finalRankFather = EndGamePanel.GetChild(1);
            }

            if(countDowdText!=null)
            {
                countDowdText.gameObject.SetActive(false);
            }

            //我们初始化汽车播放器的本地数组
            Players = new Dictionary<int, CarAIControl>();

            _lobbyManager = LocalLobbyManager.Instance;

            //OnDisableControlForPlayers = DisableControlForPlayers;
            //OnDisableControlForPlayer = DisableControlForPlayer;
            //OnEnableControlForPlayers = EnableControlForPlayers;

            OnUpdateCountdownText = UpdateCountdownText;
            OnShowEndGameScreen = ShowEndGameScreen;
            OnUpdatePlayersList = UpdatePlayersList;

            //我们注册回调按钮
            if (BackButton != null)
            {
                BackButton.onClick.RemoveAllListeners();
                BackButton.onClick.AddListener(ReturnToMenu);
            }

            if (BackToMenuButton != null)
            {
                BackToMenuButton.onClick.RemoveAllListeners();
                BackToMenuButton.onClick.AddListener(ReturnToMenu);
            }

            //测试模式(大厅是空的)
            if (LocalLobbyManager.Instance.Players().Count == 0)
            {
                InitTestMode();
            }

            if (whitchGameMod)
            {
                TeamManagerStart();
            }
            else
            {
                LoaclManagerStart();
            }


            // 独立于网络或本地启动，我们隐藏或显示相机更换按钮
            if (_cameraControllersAvailable.Length <= 1)
            {
                if (CameraChangeButton != null)
                {
                    CameraChangeButton.gameObject.SetActive(false);
                }
            }
        }

        /// <summary>
        /// 初始化测试模式玩家竞速
        /// </summary>
        protected virtual void InitTestMode()
        {
            _testMode = true;

            int currentPosition = 0;

            if (TestBotPlayers != null)
            {
                foreach (var player in TestBotPlayers)
                {
                    if (player.GetComponent<CarAIControl>() != null)
                    {
                        LocalLobbyManager.Instance.AddPlayer(new LobbyPlayer
                        {
                            Position = currentPosition,
                            Name = player.GetComponent<VehicleInformation>().LobbyName + (currentPosition + 1),
                            VehicleName = player.name,
                            VehicleSelectedIndex = -1,
                            IsBot = true
                        });
                        currentPosition++;
                    }
                    else
                    {
                        Debug.LogWarning(player.name + "不能为测试模式实例化。AI测试模式需要一个带有BaseController和VehicleAI Component的预制件。 ");
                    }
                }
            }
        }

        /// <summary>
        /// 初始化玩家和他们的车。
        /// </summary>
        public virtual void LoaclManagerStart()
        {
            InitLocalPlayers();

            UpdateNoPlayersCollisions();

            //我们在开始时禁用玩家控制，让比赛倒计时运行
            StartCoroutine(StartGameCountdownCoroutine());
        }

        public virtual void TeamManagerStart()
        {
            InitTeamPlayers();

            UpdateNoPlayersCollisions();

            //我们在开始时禁用玩家控制，让比赛倒计时运行
            StartCoroutine(StartGameCountdownCoroutine());
        }

        /// <summary>
        /// 在检查器中更新无碰撞标志中的无玩家碰撞。
        /// </summary>
        public virtual void UpdateNoPlayersCollisions()
        {
            if (NoCollisions)
            {
                int vehiclesLayer = LayerMask.NameToLayer("Actors");
                Physics.IgnoreLayerCollision(vehiclesLayer, vehiclesLayer);
            }
        }

        /// <summary>
        /// 初始化种族的玩家。在单/本地模式下实例化游戏对象
        /// </summary>
        protected virtual void InitLocalPlayers()
        {
            // 汽车控制器列表转换。用于相机控制器目标
            List<Transform> cameraBotsTargets = new List<Transform>();

            //开始位置为当前实例化的球员
            int _currentStartPosition = 0;

            // 我们首先用机器人迭代每个大厅玩家
            List<IPlayerInfo> playersAtStart;

            playersAtStart =
                    LocalLobbyManager.Instance.Players()
                    .Select(x => x.Value)
                    .OrderByDescending(x => x.IsBot)
                    .ToList();

            PlayersInfo = playersAtStart;

            scoreBG.GetComponent<RectTransform>().sizeDelta = new Vector2(330, 45 + 25 * playersAtStart.Count);
            scoreBG.SetActive(false);

            
            if (playersAtStart.Count > StartPositions.Length)
            {
                Debug.LogWarning("你只有"
                + StartPositions.Length
                + "开始位置在你的场景，但有 "
                + playersAtStart.Count
                + "球员们准备开始了。要么删除玩家，要么在你的场景中添加更多的开始位置。 ");
            }
            else
            {
                foreach (IPlayerInfo item in playersAtStart)
                {
                    GameObject prefab;

                    if (item.VehicleSelectedIndex >= 0)
                    {
                        prefab = LocalLobbyManager.Instance.AvailableVehiclesPrefabs[item.VehicleSelectedIndex];
                    }
                    else
                    {
                        //测试模式，我们找到一个预制件与资源负荷
                        prefab = Resources.Load("Vehicles/" + item.VehicleName) as GameObject;
                    }

                    // 我们首先为这个玩家实例化car。
                    // car name值用于从Resources/Vehicles文件夹加载预制件。
                    GameObject newPlayer = Instantiate(
                                               prefab,
                                               StartPositions[_currentStartPosition],
                                               Quaternion.Euler(new Vector3(0, StartAngleDegree, 0))
                                           ) as GameObject;
                    //我们将这个新对象添加到玩家列表中
                    CarAIControl car = newPlayer.GetComponent<CarAIControl>();
                   
                    Players[item.Position] = car;
                    exitCars.Add(car);

                    car.name = item.Name;

                    cameraBotsTargets.Add(newPlayer.transform);

                    car.enabled = false;

                    //我们进入下一个起始位置
                    _currentStartPosition++;
                }
            }

            //我们将球员名单添加到摄像机中
            List<CameraController> availableCam = new List<CameraController>();
            foreach (var c in CameraControllers)
            {
                if (c == null)
                {
                    return;
                }
                c.gameObject.SetActive(false);

                c.BotPlayers = cameraBotsTargets.ToArray();
                availableCam.Add(c);
            }

            _cameraControllersAvailable = availableCam.ToArray();

            if (DynamicCameras && _cameraControllersAvailable.Length == 0)
            {
                Debug.LogError("没有找到相机。请确保RaceManager检查器中至少配置了一个摄像头。");
                return;
            }
            else
            {
                if (DynamicCameras)
                {
                    //默认情况下，我们激活第一个相机
                    ChangeActiveCameraController(0);
                }
            }
        }

        protected virtual void InitTeamPlayers()
        {
            // 汽车控制器列表转换。用于相机控制器目标
            List<Transform> cameraBotsTargets = new List<Transform>();

            //开始位置为当前实例化的球员
            int _currentStartPosition = 0;

            // 我们首先用机器人迭代每个大厅玩家
            List<IPlayerInfo> playersAtStart;

            playersAtStart =
                    TeamLobbyManager.Instance.Players()//这里的获取的字典已经经过了组合，红蓝两队组合成了一队
                    .Select(x => x.Value)
                    .OrderByDescending(x => x.IsBot)
                    .ToList();


            if (playersAtStart.Count > StartPositions.Length)
            {
                Debug.LogWarning("你只有"
                + StartPositions.Length
                + "开始位置在你的场景，但有 "
                + playersAtStart.Count
                + "球员们准备开始了。要么删除玩家，要么在你的场景中添加更多的开始位置。 ");
            }
            else
            {
                foreach (IPlayerInfo item in playersAtStart)
                {
                    GameObject prefab;

                    if (item.VehicleSelectedIndex >= 0)
                    {
                        prefab = TeamLobbyManager.Instance.AvailableVehiclesPrefabs[item.VehicleSelectedIndex];
                    }
                    else
                    {
                        //测试模式，我们找到一个预制件与资源负荷
                        prefab = Resources.Load("Vehicles/" + item.VehicleName) as GameObject;
                    }

                    // 我们首先为这个玩家实例化car。
                    // car name值用于从Resources/Vehicles文件夹加载预制件。
                    GameObject newPlayer = Instantiate(
                                               prefab,
                                               StartPositions[_currentStartPosition],
                                               Quaternion.Euler(new Vector3(0, StartAngleDegree, 0))
                                           ) as GameObject;
                    //我们将这个新对象添加到玩家列表中
                    CarAIControl car = newPlayer.GetComponent<CarAIControl>();
                    Players[item.Position] = car;
                    exitCars.Add(car);

                    car.name = item.Name;

                    cameraBotsTargets.Add(newPlayer.transform);

                    //我们进入下一个起始位置
                    _currentStartPosition++;
                }
            }

            //我们将球员名单添加到摄像机中
            List<CameraController> availableCam = new List<CameraController>();
            foreach (var c in CameraControllers)
            {
                if (c == null)
                {
                    return;
                }
                c.gameObject.SetActive(false);

                c.BotPlayers = cameraBotsTargets.ToArray();
                availableCam.Add(c);
            }

            _cameraControllersAvailable = availableCam.ToArray();

            if (DynamicCameras && _cameraControllersAvailable.Length == 0)
            {
                Debug.LogError("没有找到相机。请确保RaceManager检查器中至少配置了一个摄像头。");
                return;
            }
            else
            {
                if (DynamicCameras)
                {
                    //默认情况下，我们激活第一个相机
                    ChangeActiveCameraController(0);
                }
            }
        }

        /// <summary>
        /// 切换当前活动相机到新的索引值
        /// </summary>
        /// <param name="index">Index.</param>
        public virtual void ChangeActiveCameraController(int index)
        {
            for (int i = 0; i < _cameraControllersAvailable.Length; i++)
            {
                if (i == index)
                {
                    _cameraControllersAvailable[i].gameObject.SetActive(true);
                }
                else
                {
                    _cameraControllersAvailable[i].gameObject.SetActive(false);
                }
            }
        }

        /// <summary>
        ///初始化重生按钮。
        /// </summary>
        /// <param name="vehicle">Vehicle.</param>
        public virtual void InitRespawnButton(GameObject vehicle)
        {
            if (RespawnButton != null)
            {
                RespawnButton.onClick.RemoveAllListeners();
                SolidController controller = vehicle.GetComponent<SolidController>();
                if (controller != null)
                {
                    RespawnButton.onClick.AddListener(controller.Respawn);
                    RespawnButton.gameObject.SetActive(true);
                }
            }
        }

        /// <summary>
        ///在更新中，我们对玩家列表进行排序并更新分数
        /// </summary>
        public virtual void Update()
        {
            if (!whitchGameMod)
                LocalGameMode();
            else
                TeamGameMode();
        }

        bool hasEveryoneFinished = false;
        float timeCounter = 10;
        float curTimeCounter = 0;
        private void WaitTimes()//第一名玩家冲线后，进行十秒钟的倒计时，倒计时结束，所有玩家停止运动
        {
            if (timeCounter < 0)
                return;
            curTimeCounter += Time.deltaTime;
            if (curTimeCounter > 1)
            {
                timeCounter--;
                curTimeCounter = 0;
            }
        }
        protected virtual void LocalGameMode()
        {
            //如果游戏开始了
            if (_isPlaying)
            {
                _currentGameTime += Time.deltaTime;

                // 没有UI，我们什么都不计算
                if (EndGamePanel != null)
                {
                    exitCars.Clear();
                    // 我们根据玩家的分数和他们到下一个检查点的距离对他们的列表进行排序
                    var playersRank = OnUpdatePlayersList()
                        .OrderBy(p => p.FinalRank == 0) //首先是完成者
                        .ThenBy(p => p.FinalRank) //按最终等级位置排序
                        .ThenByDescending(p => p.Score) //然后后
                        .ThenBy(p => p.DistanceToNextWaypoint)
                        .ToList();
                    exitCars = playersRank;

                    if (playersRank.Count > 0)
                    {
                        //更新评分界面
                        if (ScoreText1 != null && ScoreText2 != null && ScoreText3 != null)
                        {
                            string newscore1 = "";
                            string newscore2 = "";
                            string newscore3 = "";

                            int position = 1;
                            //我们显示当前分数
                            foreach (var p in playersRank)
                            {
                                newscore1 += position + "\r\n";
                                newscore2 += string.Format("| {0}\r\n",
                                    p.name
                                );
                                if (ClosedLoopTrack)
                                {
                                    newscore3 += string.Format("圈数 {0}/{1}\r\n",
                                        p.currentLap >= Laps ? Laps : p.currentLap + 1,
                                        Laps
                                    );
                                }
                                position++;
                            }

                            ScoreText1.text = newscore1;
                            ScoreText2.text = newscore2;
                            ScoreText3.text = newscore3;
                        }
                        //如果比赛中第一个选手完成比赛，开始倒计时10秒
                        if (playersRank[0].HasJustFinished(1))
                        {
                            hasEveryoneFinished = true;
                            _currentPlayersEndRaceTime.Add(System.Math.Round(_currentGameTime, 2));
                        }

                        if(playersRank[playersRank.Count-1].FinalRank!=0)
                            everOneFinished= true;
                        if (hasEveryoneFinished)
                        {
                            WaitTimes();
                            if (timeCounter > 0&&!everOneFinished)
                            {                                
                                countDowdText.gameObject.SetActive(true);
                                countDowdText.text = "倒计时：" + Mathf.FloorToInt(timeCounter) + "S";
                                bool playerIsFinished = true;
                                foreach (CarAIControl player in playersRank)
                                {
                                    if (player.HasJustFinished(_currentFinisherRank))
                                    {
                                        _currentFinisherRank++;
                                        _currentPlayersEndRaceTime.Add(System.Math.Round(_currentGameTime, 2));
                                    }

                                    if (player.FinalRank == 0)
                                    {
                                        playerIsFinished = false;
                                        break;
                                    }
                                }
                            }
                            else if (everOneFinished)
                            {
                                countDowdText.gameObject.SetActive(false);
                                
                                _isPlaying = false;
                                ShowLocalFinalRanking(playersRank);
                            }
                            else
                            {
                                countDowdText.gameObject.SetActive(false);
                                
                                _isPlaying = false;
                                ShowLocalFinalRanking(playersRank);
                            }
                            
                        }
                    }
                }
            }
        }

        protected virtual void TeamGameMode()//将两个队伍分别显示
        {
            if (_isPlaying)
            {
                _currentGameTime += Time.deltaTime;

                if (EndGamePanel != null)
                {
                    var playersRank = UpdatePlayersList()
                        .OrderBy(p => p.FinalRank == 0)
                        .ThenBy(p => p.FinalRank)
                        .ThenByDescending(p => p.Score)
                        .ThenBy(p => p.DistanceToNextWaypoint)
                        .ToList();

                    exitCars = playersRank;
                    if (playersRank.Count > 0)
                    {
                        if (ScoreText1 != null && ScoreText2 != null && ScoreText3 != null)
                        {
                            string newscore1 = "";
                            string newscore2 = "";
                            string newscore3 = "";

                            int position = 1;
                            //我们显示当前分数
                            foreach (var p in playersRank)
                            {
                                newscore1 += position + "\r\n";
                                newscore2 += string.Format("| {0}\r\n",
                                    p.name
                                );
                                if (ClosedLoopTrack)
                                {
                                    newscore3 += string.Format("圈数 {0}/{1}\r\n",
                                        p.currentLap >= Laps ? Laps : p.currentLap + 1,
                                        Laps
                                    );
                                }
                                position++;
                            }

                            ScoreText1.text = newscore1;
                            ScoreText2.text = newscore2;
                            ScoreText3.text = newscore3;
                        }
                    }

                    //如果比赛中第一个选手完成比赛，开始倒计时10秒
                    if (playersRank[0].HasJustFinished(1))
                    {
                        hasEveryoneFinished = true;
                        _currentPlayersEndRaceTime.Add(System.Math.Round(_currentGameTime, 2));
                    }

                    if (playersRank[playersRank.Count - 1].FinalRank != 0)
                        everOneFinished = true;
                    if (hasEveryoneFinished)
                    {
                        WaitTimes();
                        if (timeCounter > 0 && !everOneFinished)
                        {
                            countDowdText.gameObject.SetActive(true);
                            countDowdText.text = "倒计时：" + Mathf.FloorToInt(timeCounter) + "S";
                            bool playerIsFinished = true;
                            foreach (CarAIControl player in playersRank)
                            {
                                if (player.HasJustFinished(_currentFinisherRank))
                                {
                                    _currentFinisherRank++;
                                    _currentPlayersEndRaceTime.Add(System.Math.Round(_currentGameTime, 2));
                                }

                                if (player.FinalRank == 0)
                                {
                                    playerIsFinished = false;
                                    break;
                                }
                            }
                        }
                        else if (everOneFinished)
                        {
                            countDowdText.gameObject.SetActive(false);
                            //OnDisableControlForPlayers();
                            _isPlaying = false;
                            ShowLocalFinalRanking(playersRank);
                        }
                        else
                        {
                            countDowdText.gameObject.SetActive(false);
                            //OnDisableControlForPlayers();
                            _isPlaying = false;
                            ShowLocalFinalRanking(playersRank);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 返回球员列表
        /// </summary>
        /// <returns>The players list.</returns>
        protected virtual List<CarAIControl> UpdatePlayersList()
        {
            List<CarAIControl> cars = new List<CarAIControl>();
            foreach (var car in Players.Values)
            {
                cars.Add(car);
            }
            return cars;
        }

        /// <summary>
        ///显示最终排名和返回按钮
        /// </summary>
        /// <param name="playersRank">Players rank.</param>
        int i = 0;

        protected virtual void ShowLocalFinalRanking(List<CarAIControl> playersRank)
        {
            OnShowEndGameScreen();
            int counter =_currentPlayersEndRaceTime.Count;
            for (int i = 0; i < playersRank.Count; i++)
            {
                if (i < 3)
                {
                    Transform scoreGrid = Instantiate(finalRankGridPrefab, finalRankFather).transform;
                    scoreGrid.GetChild(1).GetComponent<Image>().sprite = finalRankGridPrefabImgs[i];
                    scoreGrid.GetChild(1).GetChild(0).GetComponent<Text>().text = (i + 1).ToString();
                    //scoreGrid.GetChild(2).GetComponent<RawImage>().texture = playersRank[i].
                    scoreGrid.GetChild(3).GetComponent<Text>().text = playersRank[i].name;
                    if (i < counter)
                    {
                        scoreGrid.GetChild(4).GetChild(0).GetComponent<Text>().text = _currentPlayersEndRaceTime[i] + "";
                        scoreGrid.GetChild(5).GetChild(0).GetComponent<Text>().text = "0";
                    }
                    else
                        continue;

                }
                else
                {
                    Transform scoreGrid = Instantiate(finalRankGridPrefab, finalRankFather).transform;
                    scoreGrid.GetChild(1).GetComponent<Image>().sprite = finalRankGridPrefabImgs[3];
                    scoreGrid.GetChild(1).GetChild(0).GetComponent<Text>().text = (i + 1).ToString();
                    //scoreGrid.GetChild(2).GetComponent<RawImage>().texture = playersRank[i].
                    scoreGrid.GetChild(3).GetComponent<Text>().text = playersRank[i].name;

                    if (i < counter)
                    {
                        scoreGrid.GetChild(4).GetChild(0).GetComponent<Text>().text = _currentPlayersEndRaceTime[i] + "";
                        scoreGrid.GetChild(5).GetChild(0).GetComponent<Text>().text = "0";
                    }
                    else
                        continue;
                }             
            }          
        }

        protected virtual void ShowTeamFinalRanking(List<BaseController> playersRank)
        {
            OnShowEndGameScreen();
            int counter = _currentPlayersEndRaceTime.Count;
            for (int i = 0; i < playersRank.Count; i++)
            {
                if (i < 3)
                {
                    Transform scoreGrid = Instantiate(finalRankGridPrefab, finalRankFather).transform;
                    scoreGrid.GetChild(1).GetComponent<Image>().sprite = finalRankGridPrefabImgs[i];
                    scoreGrid.GetChild(1).GetChild(0).GetComponent<Text>().text = (i + 1).ToString();
                    //scoreGrid.GetChild(2).GetComponent<RawImage>().texture = playersRank[i].
                    scoreGrid.GetChild(3).GetComponent<Text>().text = playersRank[i].name;
                    if (i < counter)
                    {
                        scoreGrid.GetChild(4).GetChild(0).GetComponent<Text>().text = _currentPlayersEndRaceTime[i] + "";
                        scoreGrid.GetChild(5).GetChild(0).GetComponent<Text>().text = "0";
                    }
                    else
                        continue;

                }
                else
                {
                    Transform scoreGrid = Instantiate(finalRankGridPrefab, finalRankFather).transform;
                    scoreGrid.GetChild(1).GetComponent<Image>().sprite = finalRankGridPrefabImgs[3];
                    scoreGrid.GetChild(1).GetChild(0).GetComponent<Text>().text = (i + 1).ToString();
                    //scoreGrid.GetChild(2).GetComponent<RawImage>().texture = playersRank[i].
                    scoreGrid.GetChild(3).GetComponent<Text>().text = playersRank[i].name;

                    if (i < counter)
                    {
                        scoreGrid.GetChild(4).GetChild(0).GetComponent<Text>().text = _currentPlayersEndRaceTime[i] + "";
                        scoreGrid.GetChild(5).GetChild(0).GetComponent<Text>().text = "0";
                    }
                    else
                        continue;
                }
            }
        }

        /// <summary>
        ///显示结束游戏屏幕排名和退出按钮
        /// </summary>
        /// <param name="text">Ranking.</param>
        protected virtual void ShowEndGameScreen()
        {
            EndGamePanel.gameObject.SetActive(true);
        }

        /// <summary>
        /// 返回大厅
        /// </summary>
        public virtual void ReturnToMenu()
        {
            if (_testMode)
            {
                Debug.LogWarning("In Test Mode, you can't quit current scene.");
                return;
            }

            _lobbyManager.ReturnToLobby();
        }

        /// <summary>
        /// 开始游戏倒计时协同程序。
        /// </summary>
        /// <returns>yield enumerator</returns>
        public virtual IEnumerator StartGameCountdownCoroutine()
        {
            // 没有UI，我们不需要倒计时
            if (EndGamePanel != null)
            {
                float remainingCountDown = StartGameCountDownTime;

                // 而至少1秒
                while (remainingCountDown > 1)
                {
                    // we yield this loop
                    yield return null;

                    remainingCountDown -= Time.deltaTime;
                    //新的剩余时间为int值
                    int newFloorTime = Mathf.FloorToInt(remainingCountDown);
                    OnUpdateCountdownText("Start in " + newFloorTime);
                }
            }

            // 剩余计数现在是&lt;1秒,
            // 因为它意味着int值为0，所以我们开始游戏

            _isPlaying = true;
            foreach (CameraController cam in _cameraControllersAvailable)
            {
                cam.GameHasStarted = true;
            }

            EnableCarMove();

            OnUpdateCountdownText("GO !");
            _currentGameTime = 0f;
            scoreBG.gameObject.SetActive(true);
            yield return new WaitForSeconds(1);

            //1秒后，我们隐藏倒计时文本对象
            OnUpdateCountdownText("");           
        }

        private void EnableCarMove()
        {
            for (int i = 0; i < exitCars.Count; i++)
            {
                exitCars[i].GetComponent<CarAIControl>().enabled = true;
            }
        }

        /// <summary>
        /// 更新倒计时文本。
        /// </summary>
        /// <param name="text">Text.</param>
        public virtual void UpdateCountdownText(string text)
        {
            if (StartGameCountdown == null)
            {
                return;
            }
            if (text == "")
            {

                StartGameCountdown.gameObject.SetActive(false);
            }
            else
            {
                StartGameCountdown.text = text;
            }
        }
        /// <summary>
        ///当玩家改变当前选择的相机时调用。
        /// 循环通过每个可用的相机
        /// </summary>
        protected virtual void OnCameraChange()
        {
            _currentCamera = (_currentCamera + 1) % CameraControllers.Length;
            ChangeActiveCameraController(_currentCamera);
        }
    }
}