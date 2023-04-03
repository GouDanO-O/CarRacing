using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace MoreMountains.HighroadEngine 
{
    /// <summary>
    /// 这个类管理本地大厅场景中的游戏场景选择和游戏状态。
    /// </summary>
    public class LocalLobbyGameUI : MonoBehaviour 
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
        ///用于显示等待文本的文本对象
        public Text WaitPlayersText;
		//倒计时文本
        public Text CountDownTimeText;
        //返回按钮
        public Button BackButton;

		protected LocalLobbyManager _localLobbyManager;
		protected int _currentSceneSelected;
		protected int maxTime = 120;
		protected int currentTime;

        /// <summary>
        ///初始化状态。
        /// </summary>
        protected virtual void Start() 
		{
			InitManagers ();
			InitUI ();
			InitStartState ();
		}

        /// <summary>
        /// 初始化管理。
        /// </summary>
        protected virtual void InitManagers()
		{
            //查找全局菜单管理器
            _localLobbyManager = LocalLobbyManager.Instance;
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
			BackButton.onClick.AddListener(_localLobbyManager.ReturnToStartScreen);
			WaitPlayersText.text = "";
			currentTime = maxTime;
		}

        /// <summary>
        /// 初始化启动状态。
        /// </summary>
        protected virtual void InitStartState()
		{
            //// Init start state
            //StartGameButton.gameObject.SetActive(false);

            //默认为第一个场景或最后使用的场景
            _currentSceneSelected = _localLobbyManager.TrackSelected;
			ShowSelectedScene();
            StartCoroutine(TimeDecrease());
        }


        /// <summary>
        /// 在更新时，我们检查是否所有玩家都准备好了
        /// </summary>
        protected virtual void Update() 
		{
            CountDownTime();
            ShowPlayerCountToPanel();			
		}
        /// <summary>
        /// 显示已进入游戏玩家的数量
        /// </summary>
        protected virtual void ShowPlayerCountToPanel() 
        {
            string newText = "";
            if (_localLobbyManager.PlayersNotReadyCount() > 0)
            {

                newText = "已就位玩家";
                newText += _localLobbyManager.Players().Count.ToString();
                newText += "/16";

            }
            WaitPlayersText.text = newText;
        }

        /// <summary>
        /// 显示所选场景。
        /// </summary>
        protected virtual void ShowSelectedScene() 
		{
			//SceneName.text = _localLobbyManager.AvailableTracksSceneName[_currentSceneSelected];
			SceneImage.sprite = _localLobbyManager.AvailableTracksSprite[_currentSceneSelected];
		}

        /// <summary>
        ///左键动作
        /// </summary>
        public virtual void OnLeft() 
		{
			if (_currentSceneSelected == 0) 
			{
				_currentSceneSelected = _localLobbyManager.AvailableTracksSceneName.Length - 1;
			}
			else 
			{
				_currentSceneSelected -= 1;
			}
			_localLobbyManager.TrackSelected = _currentSceneSelected;
			ShowSelectedScene();
		}

        /// <summary>
        /// 右键操作
        /// </summary>
        public virtual void OnRight() 
		{
			if (_currentSceneSelected == (_localLobbyManager.AvailableTracksSceneName.Length - 1)) 
			{
				_currentSceneSelected = 0;
			} 
			else
			{
				_currentSceneSelected += 1;
			}
			_localLobbyManager.TrackSelected = _currentSceneSelected;
			ShowSelectedScene();
		}

        /// <summary>
        /// 描述游戏开始时发生的事情
        /// </summary>
        public void OnStartGame() 
		{
			if (_localLobbyManager.IsReadyToPlay()) 
			{
				LoadingSceneManager.LoadScene(_localLobbyManager.AvailableTracksSceneName[_currentSceneSelected]);
			}
		}
        /// <summary>
        /// 如果玩家数量为0，倒计时结束则返回主界面重新选择，否则开始游戏
        /// </summary>
		protected void CountDownTime()
		{
            if (_localLobbyManager.Players().Count >=8)
            {
                if (currentTime > 3)
                {
                    currentTime = 3;
                }
            }
			if (currentTime <= 0)
			{
                if (_localLobbyManager.IsReadyToPlay())
                {
                    OnStartGame();
                }
                else
                {
                    _localLobbyManager.ReturnToStartScreen();
                }
            }
		}

		IEnumerator TimeDecrease()
		{
			while (currentTime>0)
			{
                yield return new WaitForSeconds(1);
                currentTime--;
                CountDownTimeText.text = "开始倒计时..." + currentTime.ToString() + "S";
            }
		}
	}
}