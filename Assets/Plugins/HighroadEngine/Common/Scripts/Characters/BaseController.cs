using UnityEngine;
using System.Collections;
using System.Linq;
using MoreMountains.Tools;
using System.Collections.Generic;

namespace MoreMountains.HighroadEngine
{
    /// <summary>
    /// 基类控制器。
    /// 必须由车辆专用控制器使用。
    /// 管理分数和输入管理。
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
	public class BaseController : MonoBehaviour, IActorInput
	{
        
        [Header("Bonus")]
        [Tooltip("当飞行器处于升力区时所施加的力")]public float BoostForce = 1f;
		
        [Tooltip("刚体的临时值。当车辆在循环区域内时拖动。这允许在循环内更好的运动。")]public float RigidBodyDragInLoop = 1f;

		[Header("Engine")]
        [Tooltip("车速:汽车转向的速度")]public float SteeringSpeed = 100f;
        [Tooltip("如果你想让车辆永远加速，将此设置为true")] public bool AutoForward = false;

		
        protected enum Zones { SpeedBoost, JumpBoost, LoopZone };
		protected Rigidbody _rigidbody;
		protected Collider _collider;
        protected RaceManager _raceManager;
		public List<Transform> _wayPoints;

        [Header("全局设置")]
        [Tooltip("当前前进的路径点")]public int _currentWaypoint = 0;
        [Tooltip("上一个穿越的点")]public int _lastWaypointCrossed = -1;
        [Tooltip("初始推力")] public float _defaultDrag;
        protected int _controllerId = -1;
		/// 当大于0，车辆已完成比赛，这是最终成绩排名
		protected int _finisherPosition = 0;

        /// <summary>
        /// 返回当前圈数。
        /// </summary>
        public int CurrentLap {get; protected set;}

        /// <summary>
        /// 获取或设置当前转向量从-1(完全左)到1(完全右)。
        /// 0为none。
        /// </summary>
        /// <value>当前转向量。</value>
        public float CurrentSteeringAmount {get; set;}

		protected float _currentGasPedalAmount;

        /// <summary>
        /// 获取或设置当前油门踏板的数量。
        /// 全加速时为1。-1表示全速制动或倒车
        /// </summary>
        /// <value>The current gas pedal amount.</value>
        public float CurrentGasPedalAmount 
		{
			get 
			{ 
				if (AutoForward) 
				{
                    // 我们得查出这个玩家是不是机器人
                    VehicleAI ai = GetComponent<VehicleAI>();
					if (ai != null && ai.Active)
					{
						return _currentGasPedalAmount;
					}
					else 
					{
                        //人类玩家总是在加速
                        return IsPlaying ? 1 : 0;
					}
				}
				else
				{
					return _currentGasPedalAmount;
				}

			} 
			set { _currentGasPedalAmount = value; }
		}

        /// <summary>
        /// 获取或设置一个值，该值指示此用户是否正在游玩
        /// </summary>
        /// <value><c>true</c> if this instance is playing; otherwise, <c>false</c>.</value>
        public virtual bool IsPlaying {get; protected set;}

        /// <summary>
        /// 获取或设置一个值，该值指示此实例是否接地。
        /// </summary>
        /// <value><c>true</c> if this instance is grounded; otherwise, <c>false</c>.</value>
        public virtual bool IsGrounded {get; protected set;}

        /// <summary>
        /// 获取或设置一个值，该值指示此实例是否处于加速状态。
        /// </summary>
        /// <value><c>true</c> if this instance is on speed boost; otherwise, <c>false</c>.</value>
        public virtual bool IsOnSpeedBoost {get; protected set;}

        /// <summary>
        ///获取车辆速度。
        /// </summary>
        /// <value>The speed.</value>
        public virtual float Speed 
		{ 
			get { return _rigidbody.velocity.magnitude; } 
		}

		public virtual void CheckSpeed()
		{

		}

        /// <summary>
        ///获取玩家分数。
        /// </summary>
        /// <value>The score.</value>
        public virtual int Score 
		{
			get 
			{
				if (_wayPoints != null)
				{
					return (CurrentLap * (_wayPoints.Count-1)) + _currentWaypoint;
				}
				else
				{
					return 0;
				}
			}
		}

        /// <summary>
        /// 获取到下一个路径点的距离。
        /// </summary>
        /// <value>The distance to next waypoint.</value>
        public virtual float DistanceToNextWaypoint 
		{
			get 
			{
				if (_wayPoints.Count == 0)
				{
					return 0;
				}

				Vector3 checkpoint = _wayPoints[_currentWaypoint].position;
				return Vector3.Distance(transform.position, checkpoint);
			}
		}

        /// <summary>
        ///获取最终排名。
        /// 0表示车辆未结束。
        /// </summary>
        /// <value>The final rank.</value>
        public int FinalRank
		{ 
			get { return _finisherPosition; }
		}

        /// <summary>
        /// 初始化各种引用
        /// </summary>
        protected virtual void Awake()
		{
			// Init managers
			_collider = GetComponent<Collider>();
			_raceManager = FindObjectOfType<RaceManager>();
			_rigidbody = GetComponent<Rigidbody>();

			IsOnSpeedBoost = false;
			_defaultDrag = _rigidbody.drag;
		}

        /// <summary>
        /// 初始化的检查点
        /// </summary>
        protected virtual void Start() 
		{
			for (int i = 0; i < _raceManager.AIWaypoints.transform.childCount; i++)
			{
				_wayPoints.Add(_raceManager.AIWaypoints.transform.GetChild(i));
            }		
		}

        /// <summary>
        /// 获取一个值，该值指示此车辆是否刚刚完成比赛。
        /// 这个属性只会返回true一次。
        /// </summary>
        /// <param name="finalRankPosition">车辆通过终点线时的等级。这个值将
        /// 可以由FinalRank属性返回。</param>
        /// <value><c>true</c> 如果这辆车已经完成;否则, <c>false</c>.</value>
        public virtual bool HasJustFinished(int finalRankPosition) 
		{
			if (_finisherPosition > 0)
			{
				return false;
			}

			bool raceEndedForPlayer;
			raceEndedForPlayer = _raceManager.ClosedLoopTrack ? 
				(Score >= (_raceManager.Laps * _wayPoints.Count)) 
				: (Score >= _wayPoints.Count);
			
			if (raceEndedForPlayer)
			{
				_finisherPosition = finalRankPosition;
			}

			return raceEndedForPlayer;
		}

        #region IActorInput implementation

        // 管理用户交互键盘，操纵杆，触摸手柄

        public virtual void MainActionPressed() 
		{
			CurrentGasPedalAmount = 1;
		}

		public virtual void MainActionDown() 
		{
			CurrentGasPedalAmount = 1;
		}

		public virtual void MainActionReleased() 
		{
			CurrentGasPedalAmount = 0;
		}

		public virtual void AltActionPressed()
		{
			CurrentGasPedalAmount = -1;
		}

		public virtual void AltActionDown()
		{
			CurrentGasPedalAmount = -1;
		}

		public virtual void AltActionReleased()
		{
			CurrentGasPedalAmount = 0;
		}

		public virtual void RespawnActionPressed()
		{
			Respawn();
		}

		public virtual void RespawnActionDown()
		{
			// nothing
		}

		public virtual void RespawnActionReleased()
		{
			// nothing
		}

		public virtual void LeftPressed() 
		{ 
			CurrentSteeringAmount = -1;
		}

		public virtual void RightPressed() 
		{ 
			CurrentSteeringAmount = 1;
		}

		public virtual void UpPressed() 
		{ 
			CurrentGasPedalAmount = 1;
		}

		public virtual void DownPressed() 
		{ 
			CurrentGasPedalAmount = -1;
		}

		public virtual void MobileJoystickPosition(Vector2 value)
		{
			CurrentSteeringAmount = value.x;
		}

		public virtual void HorizontalPosition(float value) 
		{
			CurrentSteeringAmount = value;
		}

		public virtual void VerticalPosition(float value) 
		{
			CurrentGasPedalAmount = value;
		}

		public virtual void LeftReleased()
		{ 
			CurrentSteeringAmount = 0;
		}

		public virtual void RightReleased()
		{ 
			CurrentSteeringAmount = 0;
		}

		public virtual void UpReleased()
		{
			CurrentGasPedalAmount = 0;
		}

		public virtual void DownReleased()
		{ 
			CurrentGasPedalAmount = 0;
		}

        #endregion

        /// <summary>
        /// 该方法触发车辆重生到其最后一个检查点
        /// </summary>
        public virtual void Respawn()
		{
			// Must be overriden in child classes
		}

		public virtual void CheckCarIsWhichDirToWayPoint()
		{
			//bool isCloseNewPoint=false;
			//float dis = Vector3.Distance(transform.position, _wayPoints[_currentWaypoint].position);
			//float dir = Vector3.Dot(transform.position, _wayPoints[_currentWaypoint].position);
			//if(dis <)
			//{
			//	isCloseNewPoint = true;
			//}
		}
        /// <summary>
        ///描述物体开始与物体碰撞时发生的情况
        ///用于检查点交互
        /// </summary>
        /// <param name="other">Other.</param>
        public virtual void OnTriggerEnter(Collider other) 
		{

		}

        /// <summary>
        /// 描述物体与物体碰撞时发生的情况
        /// 当飞行器停留在助推区时，用于对其施加助推力。
        /// </summary>
        /// <param name="other">Other.</param>
        public virtual void OnTriggerStay(Collider other) 
		{
			if (other.tag == Zones.SpeedBoost.ToString())
			{
                // 在加速时，我们加速车辆
                _rigidbody.AddForce(transform.forward * BoostForce, ForceMode.Impulse);
				IsOnSpeedBoost = true;

			} 

			if (other.tag == Zones.JumpBoost.ToString())
			{
				_rigidbody.AddForce(transform.up * BoostForce, ForceMode.Impulse);
				IsOnSpeedBoost = true;
			}

			if (other.tag == Zones.LoopZone.ToString())
			{
				_rigidbody.drag = RigidBodyDragInLoop;
			}
		}

        /// <summary>
        ///描述碰撞结束时发生的情况
        ///当车辆离开助推区时，移除“助推”状态
        /// </summary>
        /// <param name="other">Other.</param>
        public virtual void OnTriggerExit(Collider other)
		{
			if (other.tag == Zones.SpeedBoost.ToString() || other.tag == Zones.JumpBoost.ToString())
			{
				IsOnSpeedBoost = false;
			}
			if (other.tag == Zones.LoopZone.ToString())
			{
				// We reset physics
				_rigidbody.drag = _defaultDrag;
			}
		}

        /// <summary>
        ///启用控件。
        /// </summary>
        /// <param name="controllerId">Controller identifier.</param>
        public virtual void EnableControls(int controllerId) 
		{
			IsPlaying = true;
			CurrentSteeringAmount = 0;
			CurrentGasPedalAmount = 0;
			_controllerId = controllerId;

			// If player is not a bot
			if (_controllerId != -1) 
			{
				InputManager.Instance.SetPlayer(_controllerId, this);
			}
		}

        /// <summary>
        ///禁用控件。
        /// </summary>
        public virtual void DisableControls() 
		{
			IsPlaying = false;
			CurrentSteeringAmount = 0;
			CurrentGasPedalAmount = 0;

			// If player is not a bot
			if (_controllerId != -1) 
			{
				InputManager.Instance.DisablePlayer(_controllerId);
			}
		}
	}
}
