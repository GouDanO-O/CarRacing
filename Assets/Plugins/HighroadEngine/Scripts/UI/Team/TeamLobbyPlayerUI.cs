using UnityEngine;
using System.Collections;
using UnityEngine.UI;

namespace MoreMountains.HighroadEngine
{
    /// <summary>
    /// 这个类在大厅/菜单屏幕上管理一个播放器。
    /// 实现用于UI操作的IActorInput接口
    /// </summary>
    public class TeamLobbyPlayerUI : MonoBehaviour, IActorInput
    {
        [Header("Slot data")]
        /// 玩家加入时UI元素的次序---可省略，因为玩家无法控制，所以发送弹幕加进来就是机器人
        public int Position;
        ///参考菜单场景管理器
        public TeamLobbyGameUI MenuSceneManager;

        [Header("GUI Elements")]
        // UI player name zone
        public Text PlayerNameText;

        public RawImage PlayerRawImage;

        public Texture initialTexture;

        public bool whitchTeam=false; //false是蓝队，默认是false，当蓝队的人多余红队，如果不主动选择队伍的话，则会到红队中

        public RawImage VehicleImage;

        protected TeamLobbyManager _teamLobbyManager;
        protected bool _ready; //玩家是否准备好玩游戏或仍在选择车辆（判断当前位序是否处于空闲状态）
        protected int _currentVehicleIndex = -1;

        /// <summary>
        /// Initialisation
        /// </summary>
        public virtual void Start()
        {
            InitManagers();

            InitStartState();
        }

        /// <summary>
        /// Initializes managers
        /// </summary>
        protected virtual void InitManagers()
        {
            // Get reference to Local Lobby Manager
            _teamLobbyManager = TeamLobbyManager.Instance;
        }

        /// <summary>
        /// 初始化启动状态。
        /// </summary>
        protected virtual void InitStartState()
        {
            //*******此处接收服务器发来的用户姓名，还需要显示用户的头像**********/
            //玩家的姓名有限制，最多14个字符，超过就不显示了，要加个限位
            PlayerNameText.text = "UnSet" + (Position);

            _ready = false;

            // 如果播放器已经存在，当从音轨返回到菜单时，我们将加载数据并显示其状态
            if (_teamLobbyManager.ContainsPlayerBlue(Position))
            {
                IPlayerInfo player = _teamLobbyManager.GetPlayerBlue(Position);
                _currentVehicleIndex = player.VehicleSelectedIndex;
                OnAddBlueButton();
            }

            if(_teamLobbyManager.ContainsPlayerRed(Position))
            {
                IPlayerInfo player = _teamLobbyManager.GetPlayerRed(Position);
                _currentVehicleIndex = player.VehicleSelectedIndex;
                OnAddRedButton();
            }
            ShowSelectedVehicle();
        }

        /// <summary>
        /// 显示从“大厅管理器可用车辆”中选定的车辆
        /// </summary>
        protected virtual void ShowSelectedVehicle()
        {
            if (_currentVehicleIndex != -1)
            {
                VehicleInformation info = _teamLobbyManager.AvailableVehiclesPrefabs[_currentVehicleIndex].GetComponent<VehicleInformation>();

                VehicleImage.texture = info.lobbyTexture;
            }
        }
        /// <summary>
        /// 描述按下Add Bot按钮时发生的情况（应该是玩家进入游戏，添加到UI中）
        /// </summary>
        public virtual void OnAddBlueButton()
        {
            AddLobbyPlayer(true);
            _ready = true;
            AddPlayerToBuleTeam();
        }
        public virtual void OnAddRedButton()
        {
            AddLobbyPlayer(true);
            _ready = true;
            AddPlayerToRedTeam();
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
            // 我们寻找下一辆可用的车辆模型
            if (_currentVehicleIndex == -1)
            {
                int vehicle = _teamLobbyManager.FindFreeVehicle();
                _currentVehicleIndex = vehicle;
            }

            ShowSelectedVehicle();
        }

        /// <summary>
        /// 当玩家被移除时会发生什么 //当接收服务器发来的玩家退出消息，执行下面
        /// </summary>
        public virtual void OnRemovePlayerFromBlue()
        {
            _teamLobbyManager.RemoveBluePlayer(Position);
            _currentVehicleIndex = -1;
            PlayerNameText.text = "Player #" + (Position + 1);
            PlayerRawImage.texture = initialTexture;
            _ready = false;
        }
        public virtual void OnRemovePlayerFromRed()
        {
            _teamLobbyManager.RemoveRedPlayer(Position);
            _currentVehicleIndex = -1;
            PlayerNameText.text = "Player #" + (Position + 1);
            PlayerRawImage.texture = initialTexture;
            _ready = false;
        }

        /// <summary>
        ///下面两个方法是更换车辆的模型
        /// </summary>
        public virtual void OnLeft()
        {

        }

        public virtual void OnRight()
        {
            
        }
        /// <summary>
        ///增加一个本地玩家到大厅。
        /// </summary>
        protected virtual void AddPlayerToBuleTeam()
        {
            LobbyPlayer p = new LobbyPlayer
            {
                Position = Position+TeamLobbyManager.Instance.PlayersRed().Count,
                //大厅显示玩家的姓名和头像
                Name = PlayerNameText.text,
                PlayerImg = PlayerRawImage.texture,
                VehicleSelectedIndex = _currentVehicleIndex,
                IsBot = true,
                WhitchTeam = 0
            };
            _teamLobbyManager.AddBluePlayer(p);
        }

        protected void AddPlayerToRedTeam()
        {
            LobbyPlayer p = new LobbyPlayer
            {
                Position = Position + TeamLobbyManager.Instance.PlayersBlue().Count,
                //大厅显示玩家的姓名和头像
                Name = PlayerNameText.text,
                PlayerImg = PlayerRawImage.texture,
                VehicleSelectedIndex = _currentVehicleIndex,
                IsBot = true,
                WhitchTeam = 1
            };
            _teamLobbyManager.AddRedPlayer(p);
        }

        /// <summary>
        ///玩家退出游戏（判断玩家是哪个队伍，如果退出游戏，则将玩家从大厅队列中移除）
        /// </summary>
        protected virtual void CancelBluePlayer()
        {
            _ready = false;

            _teamLobbyManager.RemoveBluePlayer(Position);
        }
        protected virtual void CancelRedPlayer()
        {
            _ready = false;

            _teamLobbyManager.RemoveRedPlayer(Position);
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

            if (okToMove)
            {
                if (Mathf.Abs(value) > 0.1)
                {
                    if (value < 0)
                    {
                        OnLeft();
                    }
                    else
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