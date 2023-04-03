using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Tools;

namespace MoreMountains.HighroadEngine
{
    /// <summary>
    /// 这个类处理物理行为(主要是悬挂)和基于当前车辆动力学的车轮显示
    /// </summary>
    public class SolidWheelBehaviour : MonoBehaviour 
	{
		[Header("Configuration")]
        /// 引用Solid Controller父节点
        public SolidController VehicleController;
        /// 车轮位置gameObject的引用
        public Transform WheelPosition;
        /// 引用车轮模型游戏对象
        public Transform WheelModel; 
		[MMInformation("如果这个轮子在转动汽车车轮时旋转，则将此设置为true\n", MMInformationAttribute.InformationType.Info, false)]
		public bool SteeringWheel;
        /// 车轮转动方向
        public enum RollingOrientationEnum { Normal, Inverse }
		[MMInformation("这个布尔值控制轮子的旋转方向\n", MMInformationAttribute.InformationType.Info, false)]
		public RollingOrientationEnum RollingOrientation;

		[Header("Wheel behaviour")]
        /// 车轮转动的乘数因子。这也取决于车辆的速度
        public int WheelRotationSpeed = 600;
		[Range(0.1f, 50f)]
        ///车轮方向树的最大旋转角度，基于汽车车轮的方向
        public float MaximumWheelSteeringAngle = 30;
        /// 如果这是真的，轮子就会接触到地面
        public bool IsGrounded { get; protected set;}
        /// 接触点之间的道路和车轮在物理更新
        public RaycastHit PhysicsHit { get { return _physicsHit; } }

		protected RaycastHit _physicsHit;
		protected Vector3 _wheelTargetPosition;
		protected Rigidbody _rigidbody;
		protected RaycastHit _updateHit;
		// 汽车悬挂值
		protected float _previousLength;
		protected float _currentLength;
		protected float _springVelocity;
		protected float _wheelDistance; //目标距离包括悬架高度和车轮高度 
        protected LayerMask _noLayerMask = ~0;

        /// <summary>
        /// 实例初始化
        /// </summary>
        protected virtual void Start() 
		{
			_rigidbody = VehicleController.GetComponent<Rigidbody>();
		}

        /// <summary>
        /// 为车辆的动态行为更新弹簧物理
        /// </summary>
        protected virtual void FixedUpdate() 
		{
			IsGrounded = false;
			//_wheelDistance = VehicleController.RestLength + VehicleController.RadiusWheel;

            // 我们向地面投射一条射线，以了解飞行器与地面之间的距离
            if (Physics.Raycast(transform.position, -transform.up, out _physicsHit, _wheelDistance, _noLayerMask, QueryTriggerInteraction.Ignore)) 
			{
                // 如果地面比期望的距离更近或在正确的距离
                IsGrounded = true;
				SpringPhysics();
				EnginePhysics();
			}
		}

        /// <summary>
        ///更新汽车的位置/独立于物理
        /// </summary>
        protected virtual void Update()
		{
			if (WheelPosition != null) 
			{
				//UpdateWheelHeight();			
				UpdateWheelAngle();
				UpdateWheelRolling();
			}
		}

        /// <summary>
        ///应用弹簧物理。
        /// </summary>
        protected virtual void SpringPhysics()
		{
			//我们存储之前的状态
			_previousLength = _currentLength;
			//我们计算新的长度
			_currentLength = _wheelDistance - PhysicsHit.distance;
			//我们计算两个长度的差
			_springVelocity = (_currentLength - _previousLength) / Time.fixedDeltaTime;
			//我们在此基础上更新弹簧力
			float SpringForce = /*VehicleController.SpringConstant **/ _currentLength;
			//我们更新阻尼力 
			//当前与之前的差值越小，修正值就越低
			float DamperForce = /*VehicleController.DamperConstant **/ _springVelocity;
			//我们把力施加在车的上方
			Vector3 springVector = transform.up * (SpringForce + DamperForce);
			_rigidbody.AddForceAtPosition(springVector, transform.position);
		}

        /// <summary>
        ///施加加速度
        /// </summary>
        protected virtual void EnginePhysics()
		{
			_rigidbody.AddForceAtPosition(VehicleController.CurrentEngineForceValue * Time.fixedDeltaTime, 
				PhysicsHit.point + (transform.up /** VehicleController.ForwardForceHeight*/), 
				ForceMode.Acceleration);
		}

        /// <summary>
        ///更新轮子的高度
        /// </summary>
        protected virtual void UpdateWheelHeight()
		{
			//if (Physics.Raycast(transform.position, -transform.up, out _updateHit, _wheelDistance))
			//{
			//	_wheelTargetPosition = transform.position - transform.up * (_updateHit.distance - VehicleController.RadiusWheel);
   //             //如果车轮埋在地下，我们将其置于地面水平
   //             //否则我们就会向想要的位置倾斜
   //             if (WheelPosition.position.y >= _wheelTargetPosition.y)
			//	{
			//		_wheelTargetPosition.y = Mathf.Lerp(WheelPosition.position.y, _wheelTargetPosition.y, Time.deltaTime * 4);
			//	}
			//}
			//else
			//{
   //             //如果地面离地面太远，则将车轮置于最低位置
   //             _wheelTargetPosition = transform.position - transform.up * (VehicleController.RestLength);
			//	_wheelTargetPosition.y = Mathf.Lerp(WheelPosition.position.y, _wheelTargetPosition.y, Time.deltaTime * 2);
			//}
			//WheelPosition.position = _wheelTargetPosition;
		}

		/// <summary>
		///更新车轮角度
		/// </summary>
		protected virtual void UpdateWheelAngle()
		{
			if (SteeringWheel)
			{
				if (VehicleController.CurrentSteeringAmount != 0)
				{
                    WheelPosition.transform.localEulerAngles = (VehicleController.CurrentSteeringAmount * MaximumWheelSteeringAngle * Vector3.up);
				}
				else
				{
					WheelPosition.transform.localEulerAngles = Vector3.zero;
				}
			}
		}

        /// <summary>
        ///使车轮滚动基于当前速度
        /// </summary>
        void UpdateWheelRolling()
		{
			Vector3 rotationAmount = Vector3.zero;

			if (VehicleController.CurrentGasPedalAmount != 0)
			{
				rotationAmount = Vector3.right * WheelRotationSpeed * Time.deltaTime * VehicleController.NormalizedSpeed * Mathf.Sign(VehicleController.CurrentGasPedalAmount);
				rotationAmount *= RollingOrientation == RollingOrientationEnum.Normal ? -1f : 1f;
			}
			else if (VehicleController.IsGrounded)
			{
                //我们只是跟随速度的方向
                rotationAmount = Vector3.right * WheelRotationSpeed * Time.deltaTime * VehicleController.NormalizedSpeed;
                //查找滚动方向
                rotationAmount *= VehicleController.Forward == (RollingOrientation == RollingOrientationEnum.Normal) ? -1f : 1f;
			}
		
			WheelModel.Rotate(rotationAmount);
		}
			
		///// <summary>
		///// Gizmos draws
		///// </summary>
		//public virtual void OnDrawGizmos() 
		//{
		//	Gizmos.color = Color.cyan;
		//	Gizmos.DrawLine(transform.position, PhysicsHit.point);

		//	Gizmos.color = Color.red;
		
		//	Gizmos.color = Color.green;
		//	Gizmos.DrawLine(transform.position, transform.position -transform.up * (VehicleController.RestLength + VehicleController.RadiusWheel));

		//	Gizmos.color = Color.magenta;
		//	Gizmos.DrawLine (PhysicsHit.point+ transform.up * VehicleController.ForwardForceHeight, (PhysicsHit.point + transform.up * VehicleController.ForwardForceHeight)  + VehicleController.CurrentEngineForceValue / 100);

		//	Gizmos.DrawWireSphere (_updateHit.point, 0.3f);
		//}
	}
}