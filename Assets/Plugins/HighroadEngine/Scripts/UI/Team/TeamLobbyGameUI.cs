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

        ///UI����ѡ������
        public RectTransform SceneSelectZone;
        ///��ѡ����һ����������ť
        public Button LeftButton;
        /// ��ѡ����һ����������ť
        public Button RightButton;
        ///// the text object used to display the scene name
        //public Text SceneName;
        ///������ʾĿ�곡����ͼƬ��ͼ�����
        public Image SceneImage;
        ///// the start game button
        //public Button StartGameButton;
        ///������ʾ���ӵȴ��ı����ı�����
        public Text BlueWaitPlayersText;
        ///������ʾ��ӵȴ��ı����ı�����
        public Text RedWaitPlayersText;
        //����ʱ�ı�
        public Text CountDownTimeText;
        //���ذ�ť
        public Button BackButton;

        public List<LobbyPlayer> buleTeamPlayers;

        public List<LobbyPlayer> redTeamPlayers;

        protected TeamLobbyManager _teamLobbyManager;
        protected int _currentSceneSelected;
        protected int maxTime = 120;
        protected int currentTime;

        /// <summary>
        ///��ʼ��״̬��
        /// </summary>
        protected virtual void Start()
        {
            InitManagers();

            InitUI();

            InitStartState();
        }

        /// <summary>
        /// ��ʼ������
        /// </summary>
        protected virtual void InitManagers()
        {
            //����ȫ�ֲ˵�������
            _teamLobbyManager = TeamLobbyManager.Instance;
        }

        /// <summary>
        ///��ʼ����UIԪ�ص����ӡ�
        /// </summary>
        protected virtual void InitUI()
        {
            //��ʼ����ť����
            LeftButton.onClick.AddListener(OnLeft);
            RightButton.onClick.AddListener(OnRight);
            //StartGameButton.onClick.AddListener(OnStartGame);
            BackButton.onClick.AddListener(_teamLobbyManager.ReturnToStartScreen);
            BlueWaitPlayersText.text = "";
            RedWaitPlayersText.text = "";
            currentTime = maxTime;
        }

        /// <summary>
        /// ��ʼ������״̬��
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
        /// �ڸ���ʱ�����Ǽ���Ƿ�������Ҷ�׼������
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
                newText += "���� ";
                newText += _teamLobbyManager.PlayersBlue().Count.ToString() + "/" + "8";
            }
            BlueWaitPlayersText.text = newText;
        }

        protected virtual void ShowPlayerCountToRedPanel()
        {
            string newText = "";          
            if (_teamLobbyManager.PlayersRed().Count > 0)
            {
               newText += "��� ";
               newText += _teamLobbyManager.PlayersRed().Count.ToString() + "/" + "8";                        
            }           
            RedWaitPlayersText.text = newText;
        }

        /// <summary>
        /// ��ʾ��ѡ������
        /// </summary>
        protected virtual void ShowSelectedScene()
        {
            //SceneName.text = _localLobbyManager.AvailableTracksSceneName[_currentSceneSelected];
            SceneImage.sprite = _teamLobbyManager.AvailableTracksSprite[_currentSceneSelected];
        }

        /// <summary>
        ///����л���ͼ
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
        /// �Ҽ��л���ͼ
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
        /// ������Ϸ��ʼʱ����������
        /// </summary>
        public void OnStartGame()
        {
            if (_teamLobbyManager.IsReadyToPlay())
            {
                LoadingSceneManager.LoadScene(_teamLobbyManager.AvailableTracksSceneName[_currentSceneSelected]);
            }
        }
        /// <summary>
        /// ����������Ϊ0������ʱ�����򷵻�����������ѡ�񣬷���ʼ��Ϸ
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
                CountDownTimeText.text = "��ʼ����ʱ..." + currentTime.ToString() + "S";
            }
        }
    }
}

