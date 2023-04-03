using UnityEngine;
using System.Collections;
using MoreMountains.Tools;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace MoreMountains.HighroadEngine
{
    /// <summary>
    ///汽车控制器类。
    /// 管理来自InputManager和驱动评估的输入
    /// 您可以为不同类型的汽车定制汽车属性
    /// </summary>
    public class CarController : BaseController
	{
		[Header("Engine")]
		[Range(1,30)]
        /// 汽车的最高速度
        public float MaximumVelocity = 20f; 
		[Range(20,100)]
        ///发动机的动力
        public float Acceleration = 100f; 
		[Range(1,10)]
        ///刹车的力度
        public float BrakeForce = 10f; 
		[Range(1,10)]
        ///处罚适用于外出时
        public float OffroadFactor = 2f; //违例行驶

        [Header("Parameters")]
        // 汽车的重心设置在下面。这有助于统一物理与汽车的稳定性
        public Vector3 CenterOfMass = new Vector3(0,-1,0);

        // IsGrounded评估到地面的最大距离
        public float CarGroundDistance = 0.1f;

        ///齿轮枚举。汽车可向前行驶或向后行驶(倒车)
        public enum Gears {forward, reverse}

        ///当前车速
        protected Vector3 _currentVelocity;

        /// 当前齿轮值
        public Gears CurrentGear {get; protected set;}

        //汽车发动机使用的常量
        // 请随意编辑这些值。一定要彻底测试一下新车的驾驶性能
        protected const float _smallValue = 0.01f; // 汽车从刹车到倒车的最小速度值
        protected const float _minimalGasPedalValue = 20f; // 最小速度值。少走意味着汽车不能在公路上移动和其他故障
        protected float _distanceToTheGroundRaycastLength = 50f; // 用于地面评估的光线投射长度

        protected Vector3 _gravity = new Vector3(0,-30,0); // 重力是这个世界在y轴上被设为-30


        /// <summary>
        ///获取或设置到地面的距离。
        /// </summary>
        /// <value>到地面的距离。</value>
        protected virtual float DistanceToTheGround {get; set;}

        /// <summary>
        ///获取或设置地面游戏对象。
        /// </summary>
        /// <value>地面游戏对象。</value>
        protected virtual GameObject GroundGameObject {get; set;}

        /// <summary>
        /// 获取一个值，该值指示该车是否偏离道路。
        /// </summary>
        /// <value><c>true</c>如果这个实例不在公路上;否则, <c>false</c>.</value>
        public virtual bool IsOffRoad 
		{
			get 
			{
				return (GroundGameObject != null && GroundGameObject.tag == "OffRoad");
			}
		}

        /// <summary>
        /// 获取标准化的速度。
        /// </summary>
        /// <value>标准化速度。</value>
        public virtual float NormalizedSpeed {
			get 
			{
				return Mathf.InverseLerp(0f, MaximumVelocity, Mathf.Abs(Speed));
			}
		}

        /// <summary>
        /// 物理初始化
        /// </summary>
        protected override void Awake() 
		{
			base.Awake();

            // 我们设置了物理重力来适应这款游戏
            Physics.gravity = _gravity;

            //我们改变了车辆下方的质心，以帮助统一物理稳定性
            _rigidbody.centerOfMass += CenterOfMass;
		}

		/// <summary>
		/// Update
		/// </summary>
		protected virtual void Update() 
		{
			_currentVelocity = _rigidbody.velocity;

			UpdateGroundSituation();

			//如果您想查看调试信息，请启用该行
			//ShowDebugInformation();
		}

        /// <summary>
        /// 显示调试信息。Optionnal。
        /// 能在测试中显示车辆价值吗
        /// </summary>
        protected virtual void ShowDebugInformation() 
		{
			MMDebug.DebugOnScreen("Speed", (int)Speed);
			MMDebug.DebugOnScreen("IsGrounded", IsGrounded);
			MMDebug.DebugOnScreen("IsOffRoad", IsOffRoad);
			MMDebug.DebugOnScreen("Score", Score);
		}

        /// <summary>
        /// 更新这辆车的地面情况。
        /// </summary>
        protected virtual void UpdateGroundSituation() 
		{
            // 我们在汽车下方投射光线，以检查我们是否在地面上，并确定距离
            RaycastHit raycast3D = MMDebug.Raycast3D(
				_collider.bounds.center, 
				Vector3.down, 
				_distanceToTheGroundRaycastLength, 
				1<<LayerMask.NameToLayer("Ground"), 
				Color.green, 
				true);

			if (raycast3D.transform != null) 
			{
                //我们有一个地面物体
                DistanceToTheGround = raycast3D.distance;

				if (raycast3D.transform.gameObject != null) 
				{
                    //我们存储地面物体。(将用于越野检查)
                    GroundGameObject = raycast3D.transform.gameObject;
				}
			}

            //与地面的距离应该最小
            if ((DistanceToTheGround - _collider.bounds.extents.y) < CarGroundDistance) 
			{
				IsGrounded = true;
			} 
			else 
			{
				IsGrounded = false;
			}
		}

        /// <summary>
        ///固定的更新。
        /// 我们应用物理和输入评估。
        /// </summary>
        protected virtual void FixedUpdate() 
		{
            // 如果玩家在加速
            if (CurrentGasPedalAmount > 0)
			{
				CurrentGear = Gears.forward;
				Accelerate();
			}

            // 如果玩家在操纵
            if (CurrentSteeringAmount != 0)
			{
				Steer();
			}

            // 如果玩家在刹车
            if (CurrentGasPedalAmount < 0)
			{
                // 如果足够快，汽车就会开始刹车
                if ((Speed > _smallValue) && (CurrentGear == Gears.forward))
				{
					Brake();
				} else
				{
                    // 否则，车慢到可以倒车
                    CurrentGear = Gears.reverse;
                    //汽车以加速度值的一半倒车
                    CurrentGasPedalAmount /= 2;
					Accelerate();
				}
			}
		}

        /// <summary>
        /// 管理汽车的加速度(向前或反向)
        /// </summary>
        protected virtual void Accelerate() 
		{
			if (_rigidbody.velocity.magnitude > MaximumVelocity) 
			{
                // 如果车开得足够快，我们什么也不做，直接退出
                return;
			}

            // 如果我们在越野，我们会惩罚加速度和最大速度
            if ( GroundGameObject != null && GroundGameObject.tag == "OffRoad") 
			{
				if (_rigidbody.velocity.magnitude > ( MaximumVelocity / OffroadFactor))
				{
                    // 如果汽车在越野场地上跑得足够快，我们什么也不做
                    return;
				}

                // 加速度除以越野惩罚因子
                CurrentGasPedalAmount /= OffroadFactor;
			}

            //加速度具有最小值，以避免因阻力/摩擦和材料物理相互作用而阻塞汽车
            float force = Mathf.Max(_minimalGasPedalValue, Mathf.Abs(CurrentGasPedalAmount * Acceleration));

            //如果加速度是负的，我们改变力值符号
            if (CurrentGasPedalAmount < 0) 
			{
				force = -force;
			}

            // 我们应用新的速度
            _rigidbody.velocity = _rigidbody.velocity + (transform.forward * force * Time.fixedDeltaTime);

		}

        /// <summary>
        ///制动力
        /// </summary>
        protected virtual void Brake() 
		{
            // 只有当车在移动的时候
            if ((Speed > 0) && (CurrentGear == Gears.forward)) 
			{
                //我们应用新的速度
                _rigidbody.velocity = _rigidbody.velocity - (transform.forward * BrakeForce * Time.fixedDeltaTime);	        
			}
	    }

        /// <summary>
        /// 引导评估。
        /// 我们得到转向值并相应地应用旋转
        /// </summary>
        protected virtual void Steer() 
		{
			float steeringAmount = CurrentSteeringAmount * Time.fixedDeltaTime * SteeringSpeed;

            //向后行驶时，我们倒转方向盘
            if (CurrentGear == Gears.reverse) 
			{
				steeringAmount = -steeringAmount;
			}

			transform.Rotate(steeringAmount * NormalizedSpeed * Vector3.up);
		}
	}
}