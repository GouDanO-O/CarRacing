using UnityEngine;
using System.Collections;
using UnityEngine.UI;

namespace MoreMountains.HighroadEngine
{
    /// <summary>
    /// ������ڴ���/�˵���Ļ�Ϲ���һ����������
    /// ʵ������UI������IActorInput�ӿ�
    /// </summary>
    public class TeamLobbyPlayerUI : MonoBehaviour, IActorInput
    {
        [Header("Slot data")]
        /// ��Ҽ���ʱUIԪ�صĴ���---��ʡ�ԣ���Ϊ����޷����ƣ����Է��͵�Ļ�ӽ������ǻ�����
        public int Position;
        ///�ο��˵�����������
        public TeamLobbyGameUI MenuSceneManager;

        [Header("GUI Elements")]
        // UI player name zone
        public Text PlayerNameText;

        public RawImage PlayerRawImage;

        public Texture initialTexture;

        public bool whitchTeam=false; //false�����ӣ�Ĭ����false�������ӵ��˶����ӣ����������ѡ�����Ļ�����ᵽ�����

        public RawImage VehicleImage;

        protected TeamLobbyManager _teamLobbyManager;
        protected bool _ready; //����Ƿ�׼��������Ϸ������ѡ�������жϵ�ǰλ���Ƿ��ڿ���״̬��
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
        /// ��ʼ������״̬��
        /// </summary>
        protected virtual void InitStartState()
        {
            //*******�˴����շ������������û�����������Ҫ��ʾ�û���ͷ��**********/
            //��ҵ����������ƣ����14���ַ��������Ͳ���ʾ�ˣ�Ҫ�Ӹ���λ
            PlayerNameText.text = "UnSet" + (Position);

            _ready = false;

            // ����������Ѿ����ڣ��������췵�ص��˵�ʱ�����ǽ��������ݲ���ʾ��״̬
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
        /// ��ʾ�ӡ��������������ó�������ѡ���ĳ���
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
        /// ��������Add Bot��ťʱ�����������Ӧ������ҽ�����Ϸ����ӵ�UI�У�
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
        /// �ڴ�����������ʱ���ڲ��߼�
        /// </summary>
        /// <param name="isBot">If set to <c>true</c> player is a bot.</param>
        protected virtual void AddLobbyPlayer(bool isBot)
        {
            // ����Ѱ����һ�����õĳ���ģ��
            if (_currentVehicleIndex == -1)
            {
                int vehicle = _teamLobbyManager.FindFreeVehicle();
                _currentVehicleIndex = vehicle;
            }

            ShowSelectedVehicle();
        }

        /// <summary>
        /// ����ұ��Ƴ�ʱ�ᷢ��ʲô //�����շ���������������˳���Ϣ��ִ������
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
        ///�������������Ǹ���������ģ��
        /// </summary>
        public virtual void OnLeft()
        {

        }

        public virtual void OnRight()
        {
            
        }
        /// <summary>
        ///����һ��������ҵ�������
        /// </summary>
        protected virtual void AddPlayerToBuleTeam()
        {
            LobbyPlayer p = new LobbyPlayer
            {
                Position = Position+TeamLobbyManager.Instance.PlayersRed().Count,
                //������ʾ��ҵ�������ͷ��
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
                //������ʾ��ҵ�������ͷ��
                Name = PlayerNameText.text,
                PlayerImg = PlayerRawImage.texture,
                VehicleSelectedIndex = _currentVehicleIndex,
                IsBot = true,
                WhitchTeam = 1
            };
            _teamLobbyManager.AddRedPlayer(p);
        }

        /// <summary>
        ///����˳���Ϸ���ж�������ĸ����飬����˳���Ϸ������ҴӴ����������Ƴ���
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

        //��Ϊû����ҵ����룬��ϵͳ�Զ��ܣ����Բ���Ҫ�������
        #region IPlayerInput implementation

        /// <summary>
        ///��Ҫ������ť
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
        /// Alt��:ȡ������
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
        ///����ѡ�����Ĳ���ֵ
        /// </summary>
        protected bool okToMove;

        /// <summary>
        /// �Ӽ���/���ݸ˽����������
        /// </summary>
        /// <param name="value">Value.</param>
        public virtual void HorizontalPosition(float value)
        {
            // ����ʹ��һ����������ֵ���������ѡ��Ч����
            // ÿ�������Ϸ���󱻸ı�ʱ���û������ͷŰ�ť���ٴθı䡣
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