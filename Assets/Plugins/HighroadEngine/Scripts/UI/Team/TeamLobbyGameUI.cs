using MoreMountains.HighroadEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace MoreMountains.HighroadEngine
{
    public class TeamLobbyGameUI : MonoBehaviour
    {
        [Header("GUI Elements")]

        ///UI场景选择区域
        public RectTransform SceneSelectZone;
        ///“选择上一个场景”按钮
        public Button LeftButton;
        /// “选择下一个场景”按钮
        public Button RightButton;
        ///// the text object used to display the scene name
        //public Text SceneName;
        ///用于显示目标场景的图片的图像对象
        public Image SceneImage;
        ///// the start game button
        //public Button StartGameButton;
        ///用于显示蓝队等待文本的文本对象
        public Text BlueWaitPlayersText;
        ///用于显示红队等待文本的文本对象
        public Text RedWaitPlayersText;
        //倒计时文本
        public Text CountDownTimeText;
        //返回按钮
        public Button BackButton;

        public List<LobbyPlayer> buleTeamPlayers;

        public List<LobbyPlayer> redTeamPlayers;

        protected TeamLobbyManager _teamLobbyManager;
        protected int _currentSceneSelected;
        protected int maxTime = 120;
        protected int currentTime;

        /// <summary>
        ///初始化状态。
        /// </summary>
        protected virtual void Start()
        {
            InitManagers();

            InitUI();

            InitStartState();
        }

        /// <summary>
        /// 初始化管理。
        /// </summary>
        protected virtual void InitManagers()
        {
            //查找全局菜单管理器
            _teamLobbyManager = TeamLobbyManager.Instance;
        }

        /// <summary>
        ///初始化到UI元素的链接。
        /// </summary>
        protected virtual void InitUI()
        {
            //初始化按钮动作
            LeftButton.onClick.AddListener(OnLeft);
            RightButton.onClick.AddListener(OnRight);
            //StartGameButton.onClick.AddListener(OnStartGame);
            BackButton.onClick.AddListener(_teamLobbyManager.ReturnToStartScreen);
            BlueWaitPlayersText.text = "";
            RedWaitPlayersText.text = "";
            currentTime = maxTime;
        }

        /// <summary>
        /// 初始化启动状态。
        /// </summary>
        protected virtual void InitStartState()
        {
            //// Init start state
            //StartGameButton.gameObject.SetActive(false);

            // First scene or last used scene by default
            _currentSceneSelected = _teamLobbyManager.TrackSelected;
            ShowSelectedScene();
            StartCoroutine(TimeDecrease());
        }


        /// <summary>
        /// 在更新时，我们检查是否所有玩家都准备好了
        /// </summary>
        protected virtual void Update()
        {
            CountDownTime();
            ShowPlayerCountToBluePanel();
            ShowPlayerCountToRedPanel();
        }
        protected virtual void ShowPlayerCountToBluePanel()
        {
            string newText = "";
            if (_teamLobbyManager.PlayersBlue().Count > 0)
            {
                newText += "蓝队 ";
                newText += _teamLobbyManager.PlayersBlue().Count.ToString() + "/" + "8";
            }
            BlueWaitPlayersText.text = newText;
        }

        protected virtual void ShowPlayerCountToRedPanel()
        {
            string newText = "";          
            if (_teamLobbyManager.PlayersRed().Count > 0)
            {
               newText += "红队 ";
               newText += _teamLobbyManager.PlayersRed().Count.ToString() + "/" + "8";                        
            }           
            RedWaitPlayersText.text = newText;
        }

        /// <summary>
        /// 显示所选场景。
        /// </summary>
        protected virtual void ShowSelectedScene()
        {
            //SceneName.text = _localLobbyManager.AvailableTracksSceneName[_currentSceneSelected];
            SceneImage.sprite = _teamLobbyManager.AvailableTracksSprite[_currentSceneSelected];
        }

        /// <summary>
        ///左键切换地图
        /// </summary>
        public virtual void OnLeft()
        {
            if (_currentSceneSelected == 0)
            {
                _currentSceneSelected = _teamLobbyManager.AvailableTracksSceneName.Length - 1;
            }
            else
            {
                _currentSceneSelected -= 1;
            }
            _teamLobbyManager.TrackSelected = _currentSceneSelected;
            ShowSelectedScene();
        }

        /// <summary>
        /// 右键切换地图
        /// </summary>
        public virtual void OnRight()
        {
            if (_currentSceneSelected == (_teamLobbyManager.AvailableTracksSceneName.Length - 1))
            {
                _currentSceneSelected = 0;
            }
            else
            {
                _currentSceneSelected += 1;
            }
            _teamLobbyManager.TrackSelected = _currentSceneSelected;
            ShowSelectedScene();
        }

        /// <summary>
        /// 描述游戏开始时发生的事情
        /// </summary>
        public void OnStartGame()
        {
            if (_teamLobbyManager.IsReadyToPlay())
            {
                LoadingSceneManager.LoadScene(_teamLobbyManager.AvailableTracksSceneName[_currentSceneSelected]);
            }
        }
        /// <summary>
        /// 如果玩家数量为0，倒计时结束则返回主界面重新选择，否则开始游戏
        /// </summary>
        protected void CountDownTime()
        {
            if(_teamLobbyManager.PlayersBlue().Count >= 3 && _teamLobbyManager.PlayersRed().Count >= 3)
            {
                if (currentTime > 10)
                    currentTime = 10;
            }
            if (currentTime <= 0)
            {
                if (_teamLobbyManager.PlayersBlue().Count > 0&& _teamLobbyManager.PlayersRed().Count > 0)
                {
                    print("StartGame");
                    OnStartGame();
                }
                else if (_teamLobbyManager.PlayersBlue().Count == 0|| _teamLobbyManager.PlayersRed().Count == 0)
                {
                    print("ReturnToStartScreen");
                    _teamLobbyManager.ReturnToStartScreen();
                }
            }
        }

        IEnumerator TimeDecrease()
        {
            while (currentTime > 0)
            {
                yield return new WaitForSeconds(1);
                currentTime--;
                CountDownTimeText.text = "开始倒计时..." + currentTime.ToString() + "S";
            }
        }
    }
}

