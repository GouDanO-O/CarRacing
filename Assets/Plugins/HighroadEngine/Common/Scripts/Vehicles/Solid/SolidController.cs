using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Tools;
using MoreMountains.HighroadEngine;
using UnityEngine.Events;

namespace MoreMountains.HighroadEngine
{
    /// <summary>
    /// 固体控制器类，处理车辆与悬挂
    ///这允许车辆有更动态的行为，并允许颠簸的道路，斜坡，甚至循环
    ///由于SolidBehaviourExtender，该控制器还提供了更容易的可扩展性。
    ///如果你想使用这个控制器创建一个车辆，你需要正确地设置它的悬挂，并注意重量重新分配。
    /// 为此，您可以简单地复制一个演示车辆，或者查看解释如何一步一步设置车辆的文档。
    /// </summary>
    public class SolidController : BaseController 
	{
        [Tooltip("发动机的功率")] public float EngineForce = 1000;
		[Header("Vehicule Physics")]
        ///汽车的重心设置在下面。这有助于统一物理与汽车的稳定性
        [Tooltip("汽车的重心设置")] public Vector3 CenterOfMass = new Vector3(0, -1, 0); 
		[Range(0.0f,5f)]
        [Tooltip("到地面的距离")] public float GroundDistance = 1f;
		
        //[Tooltip("超过路面的速度惩罚")][Range(1, 10)] public float OffroadPenaltyFactor = 2f;

        [Tooltip("车轮的抓地力。数值越高，汽车转弯时滑动越少")] public float CarGrip = 10f;

        [Tooltip("全速:超过该速度的车辆被认为是全速行驶车辆的速度可以比这更高")] public float FullThrottleVelocity = 30;

        [Tooltip("车辆转弯所需的最低速度")][Range(0.0f, 5f)] public float MinimalTurningSpeed = 1f;

        //[Tooltip("前进力量被施加的高度")][Range(-5.0f, 5f)] public float ForwardForceHeight = 1f;

        //[Tooltip("附加扭矩力基于速度")] public AnimationCurve TorqueCurve;

        //[Tooltip("后退时的扭矩离")] public AnimationCurve BackwardForceCurve;

        public AnimationCurve frictionCurve;

        public AnimationCurve turnCurve;

        public PhysicMaterial frictionMaterial;

		[Range(0.0f,1f)]
        [Tooltip("握力系数乘数。该值越高，车辆就越容易粘在路上，即使在高速行驶时也是如此")] public float GripSpeedFactor = 0.02f;
		
        [Range(0,200)]
        [Tooltip("车辆的最大抓地力值。")] public int MaxGripValue = 100;

		//[Header("汽车的悬挂")]

  //      [Tooltip("车轮的大小")] public float RadiusWheel = 0.5f;

  //      [Tooltip("弹力")] public float SpringConstant = 20000f;

  //      [Tooltip("减震器")] public float DamperConstant = 2000f;

  //      [Tooltip("悬吊弹簧放松时的长度")] public float RestLength = 0.5f;

  //      [Tooltip("水平旋转力，将角度汽车向左或向右模拟弹簧压缩时转弯")] public float SpringTorqueForce = 1000f;

        public Transform followTarget;
        ///// <summary>
        ///// 当车辆与某物相撞时触发的事件
        ///// </summary>
        //public UnityAction<Collision> OnCollisionEnterWithOther;
        /// 当车辆重生时触发的事件
        public UnityAction OnRespawn;

        protected RaycastHit _hit;
	    protected Vector3 _startPosition;
	    protected Quaternion _startRotation;
		protected GameObject _groundGameObject;
		protected LayerMask _noLayerMask = ~0;

        //AI设置
        public float turnAI, speedAI, breakeAI;

        ///齿轮枚举。汽车可向前行驶或向后行驶(倒车)
        public enum Gears {forward, reverse}
        /// 当前齿轮值
        public Gears CurrentGear {get; protected set;}
        /// 车轮使用的当前发动机力值
        public Vector3 CurrentEngineForceValue { get; protected set;}
        /// 获取一个值，该值指示该车是否在越野。
        public virtual bool IsOffRoad 
		{ 
			get { return (_groundGameObject != null && _groundGameObject.tag == "OffRoad"); }           
		}
        /// <summary>
        ///获取标准化的速度。
        /// </summary>
        /// <value>The normalized speed.</value>
        public virtual float NormalizedSpeed 
		{
			get { return Mathf.InverseLerp(0f, FullThrottleVelocity, Mathf.Abs(Speed)); }
		}
        /// <summary>
        /// 如果车辆正在前进，则返回true
        /// </summary>
        /// <value><c>true</c> if forward; otherwise, <c>false</c>.</value>
        public virtual bool Forward 
		{
			get { return transform.InverseTransformDirection(_rigidbody.velocity).z > 0; }
		}
        /// <summary>
        /// 如果车辆正在制动，则返回true
        /// </summary>
        /// <value><c>true</c> if braking; otherwise, <c>false</c>.</value>
        public virtual bool Braking 
		{
			get { return Forward && (CurrentGasPedalAmount < 0); }
		}
        /// <summary>
        /// 返回汽车与地平线的当前角度。
        /// 例如，当车辆超过某个角度时，用于禁用AI的方向。
        /// 允许更容易的循环处理
        /// </summary>
        /// <value>The horizontal angle.</value>
        public virtual float HorizontalAngle
		{
			get { return Vector3.Dot(Vector3.up, transform.up); }
		}

        /// <summary>
        /// 获取正向归一化速度。
        /// 用于评估发动机的功率
        /// </summary>
        /// <value>The forward normalized speed.</value>
        public virtual float ForwardNormalizedSpeed
		{
			get
			{
				float forwardSpeed = Vector3.Dot(transform.forward, _rigidbody.velocity);
				return Mathf.InverseLerp(0f, FullThrottleVelocity, Mathf.Abs(forwardSpeed));
			}
		}

        /// 车辆当前的横向速度值
        public virtual float SlideSpeed {get; protected set;}

        /// <summary>
        /// 物理初始化
        /// </summary>
        protected override void Awake() 
		{
			base.Awake();

            // 我们改变了车辆下方的质心，以帮助统一物理稳定性
            _rigidbody.centerOfMass += CenterOfMass;

			CurrentGear = Gears.forward;
		}

        /// <summary>
        /// Unity启动函数
        /// </summary>
        protected override void Start()
        {
            base.Start();
            _startPosition = transform.position;
            _startRotation = transform.rotation;
        }

        /// <summary>
        ///更新主功能
        /// </summary>
        protected virtual void Update() 
		{
			UpdateGroundSituation();

			/*	MMDebug.DebugOnScreen("Steering", CurrentSteeringAmount);
			MMDebug.DebugOnScreen("acceleration", CurrentGasPedalAmount);
			MMDebug.DebugOnScreen("Speed", Speed);
			MMDebug.DebugOnScreen("SlideSpeed", SlideSpeed);
			MMDebug.DebugOnScreen("IsGrounded", IsGrounded);
			MMDebug.DebugOnScreen("Forward", Forward);
			MMDebug.DebugOnScreen("Braking", Braking);
			MMDebug.DebugOnScreen("_engineForce", CurrentEngineForceValue);
			MMDebug.DebugOnScreen("_rotationForce", CurrentRotationForceValue);
			MMDebug.DebugOnScreen("ForwardNormalizedSpeed", ForwardNormalizedSpeed);
			MMDebug.DebugOnScreen("HorizontalAngle", HorizontalAngle);*/
		}

        /// <summary>
        /// 更新这辆车的地面情况
        /// </summary>
        protected virtual void UpdateGroundSituation() 
		{
            IsGrounded = Physics.Raycast(transform.position, -transform.up, out _hit, GroundDistance, _noLayerMask, QueryTriggerInteraction.Ignore) ? true : false;
            _groundGameObject = _hit.transform != null ? _hit.transform.gameObject : null;
		}

        private void CarControl()
        {

        }

        /// <summary>
        /// 固定的更新。
        /// 我们应用物理和输入评估。
        /// </summary>
        protected virtual void FixedUpdate() 
		{
			UpdateEngineForceValue();

			UpdateSlideForce();

			//UpdateTorqueRotation();

			UpdateAirRotation();
		}

        /// <summary>
        /// 计算引擎的功率。如果条件满足，车轮可以使用此值来施加力
        /// </summary>
        protected virtual void UpdateEngineForceValue()
		{
            // 我们使用这个中间变量来解释向后模式
            float gasPedalForce = CurrentGasPedalAmount;

			CurrentEngineForceValue = (Quaternion.AngleAxis(90, transform.right) * _hit.normal * (EngineForce * gasPedalForce));
		}

        /// <summary>
        /// 当用户想转弯时，向车辆施加扭矩
        /// </summary>
  //      protected virtual void UpdateTorqueRotation()
		//{
  //          if (IsGrounded)
  //          {
  //              Vector3 torque = transform.up * Time.fixedDeltaTime * _rigidbody.mass * TorqueCurve.Evaluate(NormalizedSpeed) * SteeringSpeed;
  //              // 向后行驶时，我们倒转方向盘
  //              if (CurrentGear == Gears.reverse)
  //              {
  //                  torque = -torque;
  //              }
  //              _rigidbody.AddTorque(torque * CurrentSteeringAmount);
  //              // 水平扭矩。模拟弹簧压缩。
  //              _rigidbody.AddTorque(transform.forward * SpringTorqueForce * Time.fixedDeltaTime * _rigidbody.mass * CurrentSteeringAmount * ForwardNormalizedSpeed);
  //          }
  //      }

        /// <summary>
        /// 对车辆施加滑动力
        /// </summary>
        protected virtual void UpdateSlideForce()
		{
            if (IsGrounded)
            {
                // 我们存储水平速度
                Vector3 flatVelocity = new Vector3(_rigidbody.velocity.x, 0, _rigidbody.velocity.z);

                //我们计算横向速度值
                // 我们存储它，以便skidmarks可以使用它
                SlideSpeed = Vector3.Dot(transform.right, flatVelocity);

                // 我们根据速度和设置计算车辆的抓地力值
                float grip = Mathf.Lerp(MaxGripValue, CarGrip, Speed * GripSpeedFactor);

				Vector3 slideForce = transform.right * (-SlideSpeed * grip);
				_rigidbody.AddForce(slideForce * Time.fixedDeltaTime * _rigidbody.mass);
            }
        }

        /// <summary>
        ///控制车辆在空中的旋转
        /// </summary>
        protected virtual void UpdateAirRotation()
		{
            if (!IsGrounded)
            {
                //在空中缓慢转弯
                if (Speed > MinimalTurningSpeed)
                {
                    Vector3 airRotationForce = transform.up * CurrentSteeringAmount * SteeringSpeed * Time.fixedDeltaTime * _rigidbody.mass;
                    _rigidbody.AddTorque(airRotationForce);
                }
            }
        }

        /// <summary>
        ///重置车辆的位置。
        /// </summary>
        public override void Respawn()
		{
			Vector3 resetPosition;
			Quaternion resetRotation;

			Transform resetTransform = _currentWaypoint == 0 ? _wayPoints[_wayPoints.Count - 1] : _wayPoints[_currentWaypoint - 1];
			resetPosition = resetTransform.position;
			resetRotation = resetTransform.rotation;

			_rigidbody.velocity = Vector3.zero;
			transform.position = resetPosition;
			transform.rotation = resetRotation;

            OnRespawn();
        }
        /// <summary>
        /// 引发碰撞进入事件。
        /// </summary>
        /// <param name="other">Other object.</param>
  //      protected virtual void OnCollisionEnter(Collision other)
		//{
		//	if (OnCollisionEnterWithOther != null) 
		//	{
		//		OnCollisionEnterWithOther(other);
		//	}
		//}

        /// <summary>
        ///绘制调试信息
        /// </summary>
        protected virtual void OnDrawGizmos() 
		{
			// distance to ground
			Gizmos.color = Color.green;
			Gizmos.DrawLine (transform.position, transform.position - (transform.up * (GroundDistance)));
		}
	}
}