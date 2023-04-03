using UnityEngine;
using MoreMountains.Tools;

namespace MoreMountains.HighroadEngine 
{
    /// <summary>
    /// 相机控制器基类
    /// </summary>
    public abstract class CameraController : MonoBehaviour 
	{
        /// 如果设置为true，意味着摄像机只能跟踪一辆车。
        /// 这个值必须被子类覆盖
        public abstract bool IsSinglePlayerCamera {get;}
		public enum UpdateType { FixedUpdate, LateUpdate, Update }
		public UpdateType UpdateMode;
		/// List of human players
		public Transform[] HumanPlayers;
		/// List of bot players.
		public Transform[] BotPlayers;

		protected Camera _camera;

        /// <summary>
        /// 获取或设置一个值，该值指示此游戏是否已开始。
        /// </summary>
        /// <value><c>true</c> if game has started; otherwise, <c>false</c>.</value>
        public bool GameHasStarted {get; set;}

        /// <summary>
        /// 初始化相机游戏对象
        /// </summary>
        protected virtual void Awake() 
		{
			_camera = GetComponentInChildren<Camera>();
			GameHasStarted = false;
		}

        /// <summary>
        /// 重写此方法以刷新目标列表。
        /// </summary>
        public abstract void RefreshTargets();

        /// <summary>
        /// 重写此方法来实现摄像机移动。
        /// </summary>
        protected abstract void CameraUpdate();

		/// <summary>
		/// Unity Update
		/// </summary>
		protected virtual void Update()
		{
			if (UpdateMode == UpdateType.Update)
			{
				CameraUpdate();
			}
		}

		/// <summary>
		/// Unity LateUpdate
		/// </summary>
		protected virtual void LateUpdate()
		{
			if (UpdateMode == UpdateType.LateUpdate)
			{
				CameraUpdate();
			}
		}

		/// <summary>
		/// Unity FixedUpdate
		/// </summary>
		protected virtual void FixedUpdate()
		{
			if (UpdateMode == UpdateType.FixedUpdate)
			{
				CameraUpdate();
			}
		}
	}
}