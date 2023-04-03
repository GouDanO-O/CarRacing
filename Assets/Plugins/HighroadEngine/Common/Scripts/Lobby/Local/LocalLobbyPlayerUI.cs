using UnityEngine;
using System.Collections;
using UnityEngine.UI;

namespace MoreMountains.HighroadEngine 
{
    /// <summary>
    /// 这个类在大厅/菜单屏幕上管理一个播放器。
    /// 实现用于UI操作的IActorInput接口
    /// </summary>
    public class LocalLobbyPlayerUI : MonoBehaviour, IActorInput 
	{
		[Header("Slot data")]
		/// 玩家加入时UI元素的次序---可省略，因为玩家无法控制，所以发送弹幕加进来就是机器人
		public int Position;
		///// 玩家加入时按的按钮，每个玩家的按钮不同
		//public string JoinActionInputName;
		/// Reference to menu scene manager
		public LocalLobbyGameUI MenuSceneManager;

		[Header("GUI Elements")]
		// UI player name zone
		public Text PlayerNameText;

		public RawImage PlayerRawImage;

		public Texture initialTexture;

		// UI new player zone
		//public RectTransform AddPlayerZone;
		//public Button NewPlayerButton;
		//public Button NewBotButton;

		//// UI choose vehicle zone
		//public RectTransform ChooseVehicleZone;
		public Button RemovePlayerButton;
		//public Button LeftButton;
		//public Button RightButton;
		//public Text VehicleText;
		public RawImage VehicleImage;
		//public Button ReadyButton;
		public Button testButton;

		//public Sprite ReadyButtonSprite;
		//public Sprite CancelReadyButtonSprite;

		protected LocalLobbyManager _localLobbyManager;
		protected bool _ready; //玩家是否准备好玩游戏或仍在选择车辆（判断当前位序是否处于空闲状态）
        //protected bool _isBot;
        protected int _currentVehicleIndex = -1;

		/// <summary>
		/// Initialisation
		/// </summary>
		public virtual void Start() 
		{
			InitManagers();

            //// 我们将这个播放器注册到输入管理器中。
            //InputManager.Instance.SetPlayer(Position, this);

			InitUI();

			InitStartState();
		}

		/// <summary>
		/// Initializes managers
		/// </summary>
		protected virtual void InitManagers()
		{
			// Get reference to Local Lobby Manager
			_localLobbyManager = LocalLobbyManager.Instance;
		}

		/// <summary>
		/// Initializes links to UI elements
		/// </summary>
		protected virtual void InitUI()
		{
			////初始化按钮委托
			//NewPlayerButton.onClick.AddListener(OnAddPlayerButton);
			//NewBotButton.onClick.AddListener(OnAddBotButton);
			RemovePlayerButton.onClick.AddListener(OnRemovePlayer);
			//LeftButton.onClick.AddListener(OnLeft);
			//RightButton.onClick.AddListener(OnRight);
			//ReadyButton.onClick.AddListener(delegate {OnReady(true);});
			//testButton.onClick.AddListener(OnAddBotButton);
		}

        /// <summary>
        /// 初始化启动状态。
        /// </summary>
        protected virtual void InitStartState()
		{
			//*******此处接收服务器发来的用户姓名，还需要显示用户的头像**********/
			PlayerNameText.text = "UnSet"+(Position);

            //// the player join text is populated with the Text value from inspector. Make sure Unity Input config is setup
            //// with the same value.
            ///玩家加入文本由inspector的text值填充。确保Unity输入配置已设置用相同的值。           
            //NewPlayerButton.GetComponentInChildren<Text>().text = "Press\n" + JoinActionInputName + "\nto join";

            _ready = false;
            //ChooseVehicleZone.gameObject.SetActive(false);
            //AddPlayerZone.gameObject.SetActive(true);

            // 如果播放器已经存在，当从音轨返回到菜单时，我们将加载数据并显示其状态
            if (_localLobbyManager.ContainsPlayer(Position)) 
			{
				IPlayerInfo player = _localLobbyManager.GetPlayer(Position);
				_currentVehicleIndex = player.VehicleSelectedIndex;

				//if (player.IsBot)
				//{
					OnAddBotButton();
				//}
				//else
				//{
				//	OnAddPlayerButton();
				//	// the player needs to be ready again
				//	_localLobbyManager.RemovePlayer(Position);
				//}
			}

   //         // 在移动模式中，在开始时，玩家1已经加入了游戏
   //         // 其他玩家只能作为机器人玩家加入
   //         if (InputManager.Instance.MobileDevice)
			//{
			//	if (Position == 0) 
			//	{			
			//		if (AddPlayerZone.gameObject.activeSelf)
			//		{
			//			OnAddPlayerButton();
			//		}
			//	}
			//	else 
			//	{
			//		NewPlayerButton.gameObject.SetActive(false);
			//	}
			//}

			ShowSelectedVehicle();
		}

        /// <summary>
        /// 显示从“大厅管理器可用车辆”中选定的车辆
        /// </summary>
        protected virtual void ShowSelectedVehicle() 
		{
			if (_currentVehicleIndex != -1)
			{
				VehicleInformation info = _localLobbyManager.AvailableVehiclesPrefabs[_currentVehicleIndex].GetComponent<VehicleInformation>();

				//VehicleText.text = info.LobbyName;
				VehicleImage.texture = info.lobbyTexture;

				_localLobbyManager.registerLobbyPlayer(Position, _currentVehicleIndex);
			}
		}

        /// <summary>
        /// 描述按下“添加玩家”按钮时发生的情况
        /// </summary>
  //      public virtual void OnAddPlayerButton() 
		//{
		//	AddLobbyPlayer(false);
		//}

        /// <summary>
        /// 描述按下Add Bot按钮时发生的情况（应该是玩家进入游戏，添加到UI中）
        /// </summary>
        public virtual void OnAddBotButton() 
		{
			AddLobbyPlayer(true);
			_ready = true;
            AddLocalPlayerToLobby();
        }

		private void Update()
		{
			WheatherHasPlayer();
		}

		public virtual void WheatherHasPlayer()
		{
			if (_ready)
			{
				gameObject.transform.GetChild(1).gameObject.SetActive(false);
			}
			else
			{
                gameObject.transform.GetChild(1).gameObject.SetActive(true);
            }
		}

        /// <summary>
        /// 在大厅添加新玩家时的内部逻辑
        /// </summary>
        /// <param name="isBot">If set to <c>true</c> player is a bot.</param>
        protected virtual void AddLobbyPlayer(bool isBot)
		{
            //_isBot = isBot;

            // 我们寻找下一辆可用的车辆模型
            if (_currentVehicleIndex == -1)
			{
				int vehicle = _localLobbyManager.FindFreeVehicle();
				_currentVehicleIndex = vehicle;
			}

			ShowSelectedVehicle();

			//ChooseVehicleZone.gameObject.SetActive(true);
			//AddPlayerZone.gameObject.SetActive(false);

			//if (_isBot)
			//{
			//	//PlayerNameText.text = "Bot #" + (Position + 1);
			//	//隐藏就绪按钮
			//	ReadyButton.gameObject.SetActive(false);

                // 玩家加入进来就处于就绪状态
                //AddLocalPlayerToLobby();
			//}
			//else 
			//{
			//	PlayerNameText.text = "Player #" + (Position + 1);
			//	CancelPlayer();
			//}
		}

        /// <summary>
        /// 当玩家被移除时会发生什么 //当接收服务器发来的玩家退出消息，执行下面
        /// </summary>
        public virtual void OnRemovePlayer() 
		{
			//ChooseVehicleZone.gameObject.SetActive(false);
			//AddPlayerZone.gameObject.SetActive(true);
			_localLobbyManager.RemovePlayer(Position);
			_localLobbyManager.unregisterLobbyPlayer(Position);
			_currentVehicleIndex = -1;
			PlayerNameText.text = "Player #" + (Position + 1);
			PlayerRawImage.texture = initialTexture;
			_ready = false;
		}

        /// <summary>
        ///按下左按钮时发生的情况，
        /// </summary>
        public virtual void OnLeft() 
		{
   //         //玩家1可以改变轨道级别，如果准备好了
   //         if (Position == 0 && _ready) 
			//{
			//	MenuSceneManager.OnLeft();
			//	return;
			//}

			//if (_currentVehicleIndex == 0)
			//{
			//	_currentVehicleIndex = _localLobbyManager.AvailableVehiclesPrefabs.Length - 1;
			//} 
			//else 
			//{
			//	_currentVehicleIndex -= 1;
			//}

			//ShowSelectedVehicle();

			////if (_isBot)
			////{
   //             // 机器人总是处于准备状态，我们添加它
   //             AddLocalPlayerToLobby();
			////}
		}

        /// <summary>
        /// 描述按下右按钮时发生的情况，玩家更换车辆模型
        /// </summary>
        public virtual void OnRight() 
		{
   //         // 玩家1可以改变轨道级别，如果准备好了
   //         if (Position == 0 && _ready) 
			//{
			//	MenuSceneManager.OnRight();
			//	return;
			//}

			//if (_currentVehicleIndex == (_localLobbyManager.AvailableVehiclesPrefabs.Length - 1)) 
			//{
			//	_currentVehicleIndex = 0;
			//} 
			//else 
			//{
			//	_currentVehicleIndex += 1;
			//}

			//ShowSelectedVehicle();

			////if (_isBot)
			////{
			//	// Bot is always in ready state, we add it
			//	AddLocalPlayerToLobby();
			////}
		}

        /// <summary>
        /// 描述按下ready按钮时发生的情况
        /// </summary>
        /// <param name="fromGUI">如果这个参数设置为true，我们将不会激活玩家1的特定情况
        /// 可以开始比赛了。
        ///这让我们能够将键盘和操纵杆控制与移动触摸和鼠标控制分开 </param>
        public virtual void OnReady() 
		{
			//if (!_ready) 
			//{
			//             // 玩家进入准备状态
			//             //LeftButton.gameObject.SetActive(false);
			//             //RightButton.gameObject.SetActive(false);
			//             //RemovePlayerButton.gameObject.SetActive(false);
			//             _ready = true;

			//	//ReadyButton.transform.Find("Text").GetComponent<Text>().text = "- ready -";
			//	//ReadyButton.image.sprite = CancelReadyButtonSprite;

			
			//} 
			//else 
			//{
			//	if (!fromGUI)
			//	{
			//		if (Position == 0 && _ready)
			//		{
			//			MenuSceneManager.OnStartGame();
			//			return;
			//		}
			//	}

			//	CancelPlayer();
			//}
		}

        /// <summary>
        ///增加一个本地玩家到大厅。
        /// </summary>
        protected virtual void AddLocalPlayerToLobby()
		{
			LobbyPlayer p = new LobbyPlayer {
				Position = Position,
				//大厅显示玩家的姓名和头像
				Name = PlayerNameText.text,
				PlayerImg = PlayerRawImage.texture,
				//VehicleName = VehicleText.text,
				VehicleSelectedIndex = _currentVehicleIndex,
				IsBot = true,
				WhitchTeam = 2
			};
			_localLobbyManager.AddPlayer(p);
		}

        /// <summary>
        ///取消玩家选择
        /// </summary>
        protected virtual void CancelPlayer()
		{
			// 玩家取消准备状态
			//LeftButton.gameObject.SetActive(true);
			//RightButton.gameObject.SetActive(true);
			//RemovePlayerButton.gameObject.SetActive(true);
			//ReadyButton.gameObject.SetActive(true);

			_ready = false;
			//string buttonText = "";

			//if (_isBot)
			//{
			//	buttonText = "Bot Ready?";
			//}
			//else 
			//{
			//	buttonText = "Ready?";
			//}
			//ReadyButton.transform.Find("Text").GetComponent<Text>().text = buttonText;

			//ReadyButton.image.sprite = ReadyButtonSprite;
			_localLobbyManager.RemovePlayer(Position);
		}

        //因为没有玩家的输入，是系统自动跑，所以不需要输入管理
        #region IPlayerInput implementation

        /// <summary>
        ///主要操作按钮
        /// </summary>
        public virtual void MainActionDown()
		{
			//if (AddPlayerZone.gameObject.activeSelf)
			//{
			//	// Player use its main action button to join game
			//	OnAddPlayerButton();
			//}
			//else
			//{
			//	// Player use its main action button to validate vehicle choosen
			//	OnReady(false);
			//}
		}

        /// <summary>
        /// Alt键:取消键。
        /// </summary>
        public virtual void AltActionDown()
		{
			//if (_ready)
			//{
			//	CancelPlayer();
			//}
			////else if (ChooseVehicleZone.gameObject.activeSelf)
			////{
			////	OnRemovePlayer();
			////}
		}

		/// <summary>
		///车辆选择器的布尔值
		/// </summary>
		protected bool okToMove;

        /// <summary>
        /// 从键盘/操纵杆进行输入管理。
        /// </summary>
        /// <param name="value">Value.</param>
        public virtual void HorizontalPosition(float value)
		{
            // 我们使用一个阻塞布尔值来避免滚动选择效果。
            // 每次玩家游戏对象被改变时，用户必须释放按钮来再次改变。
            if (Mathf.Abs(value) <= 0.1) 
			{
				okToMove = true;
			}

			if (okToMove) {
				if (Mathf.Abs(value) > 0.1) 
				{
					if (value < 0) 
					{
						OnLeft();
					} else 
					{
						OnRight();
					}
					okToMove = false;
				}
			}
		}

		public virtual void VerticalPosition(float value) { }
		public void RespawnActionDown() { }
		public virtual void AltActionReleased() { }
		public virtual void RespawnActionReleased() { }
		public void MobileJoystickPosition(Vector2 value) { }
		public virtual void MainActionReleased() { }
		public virtual void LeftPressed() { }
		public virtual void RightPressed() { }
		public virtual void UpPressed() { }
		public virtual void DownPressed() { }
		public void AltActionPressed() { }
		public void RespawnActionPressed() { }
		public virtual void MainActionPressed() { }
		public void LeftReleased() { }
		public void RightReleased() { }
		public void UpReleased() { }
		public void DownReleased() { }

		#endregion
	}
}