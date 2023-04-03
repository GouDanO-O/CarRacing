using MoreMountains.HighroadEngine;
using MoreMountains.Tools;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Services.Analytics;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MoreMountains.HighroadEngine
{
    public class TeamLobbyManager : MMPersistentSingleton<TeamLobbyManager>, IGenericLobbyManager
    {
        /// ��Ϸ����ҵ����������
        ///�������ı����ֵ������Ҫ�չ˴���GUI��
        ///�����Ϊʲô���ڼ�����в��ǹ������ԡ�
        private const int MAX_PLAYERS = 16;

        [Header("Lobby parameters")]
        ///���ó��������ơ�
        public string LobbyScene = "TeamLobby";

        [Header("Vehicles configuration")]
        ///��ҿ���ѡ��ĳ���Ԥ�Ƽ��б�
        public GameObject[] AvailableVehiclesPrefabs;

        [Header("Tracks configuration")]
        ///������������б�������UI�м��س�������ʾ��������
        public string[] AvailableTracksSceneName;
        ///���������б�������UI����ʾ��ѡ�����ͼ��
        public Sprite[] AvailableTracksSprite;

        protected Dictionary<int, IPlayerInfo> _players = new Dictionary<int, IPlayerInfo>();

        protected Dictionary<int, IPlayerInfo> _bluePlayers = new Dictionary<int, IPlayerInfo>();

        protected Dictionary<int, IPlayerInfo> _redPlayers = new Dictionary<int, IPlayerInfo>();
        /// <summary>
        ///�洢��ǰѡ���ĸ���������
        /// </summary>
        /// <value>��ǰѡ��Ĺ��.</value>
        public virtual int TrackSelected { get; set; }

        #region interface IGenericLobbyManager

        /// <summary>
        ///������ҵ����������
        /// </summary>
        /// <value>��ҵ����������</value>
        public virtual int MaxPlayers
        {
            get { return MAX_PLAYERS; }
        }

        /// <summary>
        /// ����ǰ��������Ϊ����������
        /// </summary>
        public void ReturnToLobby()
        {
            SceneManager.LoadScene(LobbyScene);
        }

        /// <summary>
        ///����ǰ��������Ϊ��ʼ��Ļ��
        /// </summary>
        public virtual void ReturnToStartScreen()
        {
            LoadingSceneManager.LoadScene("StartScene");
            //������������־ö���
            Destroy(gameObject);
        }

        #endregion

        /// <summary>
        ///Ĭ������£�����ѡ���һ�����
        /// </summary>
        public virtual void Start()
        {
            TrackSelected = 0;
        }
        /// <summary>
        ///���ػ�Ծ����б�
        ///����������ֵ��������ݡ�
        /// </summary>
        public virtual Dictionary<int,IPlayerInfo> Players()
        {
            CombineDictional();
            return _players;
        }

        public virtual Dictionary<int, IPlayerInfo> PlayersBlue()
        {
            return _bluePlayers;
        }

        public virtual Dictionary<int, IPlayerInfo> PlayersRed()
        {
            return _redPlayers;
        }

        public virtual void CombineDictional()
        {
            int i = 0, j =0;
            while (i < _bluePlayers.Count)
            {
                _players.Add(i, _bluePlayers[i]);
                i++;
            }
            i = _bluePlayers.Count;
            while(j<_redPlayers.Count)
            {
                _players.Add(i, _redPlayers[j]);
                i++;
                j++;
            }
        }

        /// <summary>
        ///����Ҷ�����ӵ������б���
        /// </summary>
        /// <param name="p">LocalLobbyPlayer player</param>
        public virtual void AddBluePlayer(LobbyPlayer p)
        {
            _bluePlayers[p.Position-_redPlayers.Count] = p;
        }
        public virtual void AddRedPlayer(LobbyPlayer p)
        {
            _redPlayers[p.Position-_bluePlayers.Count] = p;
        }


        /// <summary>
        /// ����Ҵӻ�Ծ����б����Ƴ�
        /// </summary>
        /// <param name="p">Position</param>
        public virtual void RemovePlayer(int p)
        {
            _players.Remove(p);
        }
        public virtual void RemoveBluePlayer(int p)
        {
            _bluePlayers.Remove(p);
        }
        public virtual void RemoveRedPlayer(int p)
        {
            _redPlayers.Remove(p);
        }

        /// <summary>
        ///ͨ��λ�û�ȡ���
        /// </summary>
        /// <returns>������Ա</returns>
        /// <param name="position">Position index</param>
        public virtual IPlayerInfo GetPlayer(int p)
        {
            return _players[p];
        }

        public virtual IPlayerInfo GetPlayerBlue(int p)
        {
            return _bluePlayers[p];
        }

        public virtual IPlayerInfo GetPlayerRed(int p)
        {
            return _redPlayers[p];
        }

        /// <summary>
        /// �������Ѿ�ע���˸�λ�ã��򷵻�true��
        /// ����Ϸ����������ص���������ʱʹ�á�
        /// </summary>
        /// <returns><c>true</c>, if player exists, <c>false</c> otherwise.</returns>
        /// <param name="position">Position index.</param>
        public virtual bool ContainsPlayerBlue(int position)
        {
            return _bluePlayers.ContainsKey(position);
        }
        public virtual bool ContainsPlayerRed(int position)
        {
            return _redPlayers.ContainsKey(position);
        }

        /// <summary>
        /// ���س���������
        /// </summary>
        /// <returns>The free vehicle index.</returns>
        public virtual int FindFreeVehicle()
        {
            return Random.Range(0, AvailableVehiclesPrefabs.Length);
        }

        /// <summary>
        ///�������Ƿ�׼������
        /// </summary>
        /// <returns><c>true</c> ��������������һ������ڳ���������Ҷ�׼�����ˡ�</returns>
        public virtual bool IsReadyToPlay()
        {
            return _bluePlayers.Count>0&& _redPlayers.Count > 0;
        }
    }
}
