using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using MoreMountains.Tools;
using UnityEngine.SceneManagement;

namespace MoreMountains.HighroadEngine 
{

    /// <summary>
    /// 设置大厅中汽车的数量和加载
    /// 这个单例类在种族和大厅操作之间管理玩家列表
    /// </summary>
    public class LocalLobbyManager : MMPersistentSingleton<LocalLobbyManager>, IGenericLobbyManager 
	{
        /// 游戏中玩家的最大数量。
        ///如果你想改变这个值，你需要照顾大厅GUI。
        ///这就是为什么它在检查器中不是公共属性。
        private const int MAX_PLAYERS = 16;

		[Header("Lobby parameters")]
        ///大堂场景的名称。
        public string LobbyScene = "SingleLobby";

		[Header("Vehicles configuration")]
        ///玩家可以选择的车辆预制件列表。
        public GameObject[] AvailableVehiclesPrefabs;

		[Header("Tracks configuration")]
        ///轨道场景名称列表。用于在UI中加载场景和显示场景名称
        public string[] AvailableTracksSceneName;
        ///轨道精灵的列表。用于在UI中显示所选轨道的图像
        public Sprite[] AvailableTracksSprite; 

		protected Dictionary<int, IPlayerInfo> _players = new Dictionary<int, IPlayerInfo>(); //关键是球员位置号。
        protected Dictionary<int, int> _registeredLobbyPlayers = new Dictionary<int, int>(); //已注册的大厅玩家尚未准备好。

        /// <summary>
        ///存储当前选定的跟踪索引。
        /// </summary>
        /// <value>当前选择的轨道.</value>
        public virtual int TrackSelected {get; set;}

        #region interface IGenericLobbyManager

        /// <summary>
        ///返回玩家的最大数量。
        /// </summary>
        /// <value>玩家的最大数量。</value>
        public virtual int MaxPlayers 
		{ 
			get { return MAX_PLAYERS; }
		}

        /// <summary>
        /// 将当前场景更改为大厅场景。
        /// </summary>
        public void ReturnToLobby()
		{
			SceneManager.LoadScene(LobbyScene);
		}

        /// <summary>
        ///将当前场景更改为开始屏幕。
        /// </summary>
        public virtual void ReturnToStartScreen()
		{
			LoadingSceneManager.LoadScene("StartScene");
            //我们销毁这个持久对象
            Destroy(gameObject);
		}

        #endregion

        /// <summary>
        ///默认情况下，我们选择第一个轨道
        /// </summary>
        public virtual void Start() 
		{
			TrackSelected = 0;
		}


        /// <summary>
        ///返回活跃玩家列表
        ///键是索引，值是玩家数据。
        /// </summary>
        public virtual Dictionary<int, IPlayerInfo> Players() 
		{
			return _players;
		}

        /// <summary>
        ///将玩家对象添加到活动玩家列表中
        /// </summary>
        /// <param name="p">LocalLobbyPlayer player</param>
        public virtual void AddPlayer(LobbyPlayer p) 
		{
			_players[p.Position] = p;
		}

        /// <summary>
        /// 将玩家从活跃玩家列表中移除
        /// </summary>
        /// <param name="position">Position</param>
        public virtual void RemovePlayer(int position) 
		{ 
			_players.Remove(position);
		}

        /// <summary>
        ///通过位置获取玩家
        /// </summary>
        /// <returns>这名球员</returns>
        /// <param name="position">Position index</param>
        public virtual IPlayerInfo GetPlayer(int position)
		{
			return _players[position];
		}

        /// <summary>
        /// 如果玩家已经注册了该位置，则返回true。
        /// 在游戏场景结束后回到大厅场景时使用。
        /// </summary>
        /// <returns><c>true</c>, if player exists, <c>false</c> otherwise.</returns>
        /// <param name="position">Position index.</param>
        public virtual bool ContainsPlayer(int position) 
		{
			return _players.ContainsKey(position);
		}

        /// <summary>
        /// 返回车辆的索引
        /// </summary>
        /// <returns>The free vehicle index.</returns>
        public virtual int FindFreeVehicle()
		{
            return Random.Range(0, AvailableVehiclesPrefabs.Length);
		}

        /// <summary>
        ///检查玩家是否准备好了
        /// </summary>
        /// <returns><c>true</c> 至少有一名玩家在场，所有玩家都准备好了。</returns>
        public virtual bool IsReadyToPlay() 
		{
			if (_players.Count == 0)
			{
				return false;
			}

			return (_players.Count == _registeredLobbyPlayers.Count);
		}

        /// <summary>
        /// 返回大厅中尚未准备好的玩家数量
        /// </summary>
        /// <returns>玩家计数.</returns>
        public virtual int PlayersNotReadyCount()
		{
			return _registeredLobbyPlayers.Count;
		}

        /// <summary>
        /// 注册大厅播放器。
        /// </summary>
        /// <param name="position">Position.</param>
        /// /// <param name="vehicleIndex">Vehicle Index.</param>
        public virtual void registerLobbyPlayer(int position, int vehicleIndex)
		{
			_registeredLobbyPlayers[position] = vehicleIndex;
		}

        /// <summary>
        /// 取消大厅播放器的注册。
        /// </summary>
        /// <param name="position">Position.</param>
        public virtual void unregisterLobbyPlayer(int position)
		{
			_registeredLobbyPlayers.Remove(position);
		}
	}
}