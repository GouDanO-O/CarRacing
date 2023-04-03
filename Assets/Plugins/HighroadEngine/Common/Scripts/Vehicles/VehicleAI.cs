using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using MoreMountains.Tools;

namespace MoreMountains.HighroadEngine
{
    /// <summary>
    /// 汽车的AI逻辑
    /// 使用简单的AI管理驾驶:AI遵循每个AIWaypoint的顺序。
    /// 这个引擎是通用的，可以用在不同类型的车辆，只要他们实现
    /// BaseController与Steer和GasPedal。
    /// </summary>
    [RequireComponent(typeof(BaseController))]
	public class VehicleAI : MonoBehaviour 
	{
        /// 如果激活，AI控制车辆
        public bool Active;

		[Header("AI configuration")]
		[MMInformation("在汽车被重置到最后一个检查点之前的秒数。如果值为零，汽车将保持原样。", MMInformationAttribute.InformationType.Info, false)]
		[Range(0,10)]
        /// 在汽车被重置到最后一个检查点之前的秒数。如果值为零，汽车将保持原样。
        [Tooltip("重置到最后一个检查点之前的秒数")]public float TimeBeforeStuck = 5f;
		[MMInformation("考虑路点到达的距离", MMInformationAttribute.InformationType.Info, false)]
		[Range(5,30)]
        ///当到达这个距离时，AI会前往下一个航路点
        [Tooltip("离点的最低距离（当到达这个距离时，会前往下一个点）")]public int MinimalDistanceToNextWaypoint = 10; 

		[MMInformation("当航路点在前方时节流", MMInformationAttribute.InformationType.Info, false)]
		[Range(0f, 1f)]
        [Tooltip("最大油门踏板量")] public float FullThrottle = 1f; 

		[MMInformation("当车辆必须转弯到达航路点时节流阀。", MMInformationAttribute.InformationType.Info, false)]
		[Range(0f, 1f)]
		[Tooltip("油门踏板的最小量")] public float SmallThrottle = 0.3f; 

		[MMInformation("为了帮助人工智能，车辆可以拥有比平时更好的转向速度。", MMInformationAttribute.InformationType.Info, false)]
        [Tooltip("是否超过转弯速度")] public bool OverrideSteringSpeed = false;
        [Tooltip("转向速度")] public int SteeringSpeed = 300;

        // AI引擎使用的常量
        // 请随意编辑这些值。一定要彻底测试一下新的人工智能汽车的驾驶行为
        [Tooltip("车辆前方与目标航路点之间的最大夹角距离")][SerializeField] protected const float _largeAngleDistance = 90f;
        [Tooltip("车辆前方与目标航路点之间的最小夹角距离")][SerializeField] protected const float _smallAngleDistance = 5f;
        [Tooltip("当车辆至少以这个速度行驶时，AI可以使用刹车")][SerializeField] protected const float _minimalSpeedForBrakes = 0.5f;
        [Tooltip("车辆卡住的距离")][SerializeField] protected const float _maximalDistanceStuck = 0.5f;

        public List<Vector3> _AIWaypoints;
		protected BaseController _controller;
		[SerializeField]protected int _currentWaypoint;
        [Tooltip("方向***仅供参考***")][SerializeField] protected float _direction = 0f;
        [Tooltip("加速度***仅供参考***")][SerializeField] protected float _acceleration = 0f;
        [Tooltip("目标点***仅供参考***")][SerializeField] protected Vector3 _targetWaypoint;
		protected RaceManager _raceManager;
		protected SolidController _solidController;
        [SerializeField] protected float _targetAngleAbsolute;
        [SerializeField] protected int _newDirection;
		protected float _stuckTime = 0f;
        [SerializeField] protected Vector3 _lastPosition;

        public Transform followTarget;
        public float followDistance;

        public WaypointCircuit.RoutePoint targetPoint { get; private set; }
        public WaypointCircuit.RoutePoint speedPoint { get; private set; }
        public WaypointCircuit.RoutePoint progressPoint { get; private set; }

        //一个乘数，在当前速度的基础上，增加前方要瞄准的距离
        [SerializeField] private float lookAheadForTargetFactor = .1f;
        
        //前方偏移量仅用于航路速度调整(应用于航路点目标的旋转变换)
        private float lookAheadForSpeedOffset = 50;

        //一个乘数增加的距离，沿着路线的速度调整
        private float lookAheadForSpeedFactor = .2f;

        private WaypointCircuit waypointCircuit;

        /// <summary>
        /// 初始化
        /// </summary>
        public virtual void Start() 
		{
			_controller = GetComponent<BaseController>();
			_solidController = GetComponent<SolidController>();
			_raceManager = FindObjectOfType<RaceManager>();
            waypointCircuit = _raceManager.AIWaypoints.GetComponent<WaypointCircuit>();

            if (followTarget == null)
            {
                followTarget = new GameObject(name + " Waypoint Target").transform;
            }

            //我们得到了AI路点的列表
            if (waypointCircuit != null)
			{
                _AIWaypoints = waypointCircuit.points.ToList();
                // AI会在列表中寻找第一个路径点
                _currentWaypoint = 0;
				_targetWaypoint = _AIWaypoints[_currentWaypoint];
			}

            followDistance= 0f;

            followTarget.position = waypointCircuit.Waypoints[_currentWaypoint].position;
            followTarget.rotation = waypointCircuit.Waypoints[_currentWaypoint].rotation;

            if (_solidController != null)
            {
                _solidController.OnRespawn += ResetAIWaypointToClosest;
            }
		}

        /// <summary>
        /// 在LateUpdate，我们应用人工智能逻辑
        /// </summary>
        public virtual void LateUpdate()
		{
   //         //如果需要，我们会控制人工智能的转向速度
   //         if (OverrideSteringSpeed)
			//{
			//	_controller.SteeringSpeed = SteeringSpeed;
			//}

			if (IsStuck())
            {
                _controller.Respawn();
                return;
            }

            EvaluateNextWaypoint();

            EvaluateDirection();

			//CalculateValues();

            //我们更新控制器输入
            _controller.VerticalPosition(_acceleration);
			_controller.HorizontalPosition(_direction);
		}

        /// <summary>
        ///重置下一个AI航路点到最近
        /// </summary>
        protected virtual void ResetAIWaypointToClosest()
        {
            int indexChoosen = -1;
            float localMinimumDistance = float.MaxValue;
            for (int i = 0; i < _AIWaypoints.Count; i++)
            {
                Vector3 heading = _AIWaypoints[i] - _controller.transform.position;
                float facing = Vector3.Dot(heading, _controller.transform.forward);

                if (facing > 0)                    
                {
                    float distance = Vector3.Distance(_AIWaypoints[i], _controller.transform.position);
                    if (distance < localMinimumDistance)
                    {
                        localMinimumDistance = distance;
                        indexChoosen = i;
                    }
                }
            }
            _currentWaypoint = indexChoosen;
            _targetWaypoint = _AIWaypoints[indexChoosen];
        }
        /// <summary>
        /// 如果汽车在同一个地方停留太久，我们就会重生到最后一个检查点
        /// </summary>
        protected virtual bool IsStuck()
		{
			if (TimeBeforeStuck > 0) 
			{
				if (Vector3.Distance(_lastPosition,transform.position) < _maximalDistanceStuck)
				{
					if (_stuckTime == 0f)
					{
						_stuckTime = Time.time;
					}
				}
				else 
				{
					_lastPosition = transform.position;
					_stuckTime = 0;
				}

				if ((_stuckTime > 0f) && (Time.time - _stuckTime) > TimeBeforeStuck) 
				{
					_stuckTime = 0;
					return true;
				}

				return false;
			}

			return false;
		}

        /// <summary>
        ///我们确定当前路径点是否仍然正确
        /// </summary>
        protected virtual void EvaluateNextWaypoint()
		{
			var distanceToWaypoint = PlaneDistanceToWaypoints();
            // 如果我们足够接近当前路径点，我们就切换到下一个路径点
            if (distanceToWaypoint < MinimalDistanceToNextWaypoint)
			{
				_currentWaypoint++;
                //一圈后，我们回到1号检查点
                if (_currentWaypoint == _AIWaypoints.Count)
				{
					_currentWaypoint = 0;
				}
                //我们设置了新的目标航路点
                _targetWaypoint = _AIWaypoints[_currentWaypoint];
			}
		}
        /// <summary>
        ///确定朝向航路点的方向
        /// </summary>
        protected virtual void EvaluateDirection()
		{
            followTarget.position = waypointCircuit.GetRoutePoint(followDistance + _smallAngleDistance + lookAheadForTargetFactor * _solidController.SlideSpeed).position;
            followTarget.rotation = Quaternion.LookRotation(waypointCircuit.GetRoutePoint(followDistance + lookAheadForSpeedOffset + lookAheadForSpeedFactor * _solidController.NormalizedSpeed).direction);

            progressPoint = waypointCircuit.GetRoutePoint(followDistance);
            Vector3 progressDelta=progressPoint.position-transform.position;
            if (Vector3.Dot(progressDelta, progressPoint.direction) < 0)
                followDistance += progressDelta.magnitude * 0.5f;

   //         //我们计算车辆与平面上下一个路径点之间的目标向量(不含Y轴)
   //         Vector3 targetVector = _targetWaypoint - transform.position;
			//targetVector.y = 0;
			//Vector3 transformForwardPlane = transform.forward;
			//transformForwardPlane.y = 0;
   //         // 然后我们测量车辆向前到目标向量的角度
   //         _targetAngleAbsolute = Vector3.Angle(transformForwardPlane, targetVector);
   //         //我们还计算叉乘来确定角度是否为正
   //         Vector3 cross = Vector3.Cross(transformForwardPlane, targetVector);
   //         // 这个值表示车辆是向左还是向右移动
   //         _newDirection = cross.y >= 0 ? 1 : -1;
		}

        /// <summary>
        ///应用控制将车辆移动到路径点
        /// </summary>
        protected virtual void CalculateValues()
		{
            // 现在，我们应用_direction和_acceleration值
            // 如果车辆看向相反的方向?
            if (_targetAngleAbsolute > _largeAngleDistance)
			{
                //我们朝着正确的方向前进
                _direction = -_newDirection;
                // 如果我们有足够的速度，我们刹车旋转得更快
                if (_controller.Speed > _minimalSpeedForBrakes)
				{
					_acceleration = -FullThrottle;
                }
				else
				{
                    //否则我们会缓慢加速
                    _acceleration = SmallThrottle;
				}
                //否则，如果车辆没有指向路点，但也不太远?
            }
            else if (_targetAngleAbsolute > _smallAngleDistance)
			{
                //我们朝着正确的方向前进
                _direction = _newDirection;
                // 我们慢慢加速
                _acceleration = SmallThrottle;
			}
			else
			{
                //如果车辆正对着路点，我们就切换到全速
                _direction = 0f;
				_acceleration = FullThrottle;
			}
		}

        /// <summary>
        /// 返回下一个路径点和车辆之间的平面距离
        /// </summary>
        /// <returns>到下一个航路点的距离。</returns>
        protected virtual float PlaneDistanceToWaypoints()
		{
			Vector2 target = new Vector2(_targetWaypoint.x, _targetWaypoint.z);
			Vector2 position = new Vector2(transform.position.x, transform.position.z);

			return Vector2.Distance(target, position);
		}

        private void OnDrawGizmos()
        {
            if (Application.isPlaying)
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawLine(transform.position, followTarget.position);
                Gizmos.color = Color.blue;
                Gizmos.DrawWireSphere(waypointCircuit.GetRoutePosition(followDistance), 0.2f);
                Gizmos.DrawLine(transform.position, waypointCircuit.GetRoutePosition(followDistance));
                Gizmos.DrawLine(followTarget.position, followTarget.position + followTarget.forward);
                Gizmos.color = Color.magenta;
                Gizmos.DrawWireSphere(followTarget.position, 1);
            }
        }

    }

}
