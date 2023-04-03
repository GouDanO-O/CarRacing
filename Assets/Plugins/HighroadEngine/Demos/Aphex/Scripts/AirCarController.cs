using UnityEngine;
using System.Collections;
using MoreMountains.Tools;

namespace MoreMountains.HighroadEngine
{
    /// <summary>
    /// 空中汽车控制器类。
    /// 管理来自InputManager的输入和悬停汽车的驾驶
    /// 您可以为不同类型的车辆定制属性。
    /// </summary>
    public class AirCarController : BaseController	
	{
		[MMInformation("Engine Power force.\n", MMInformationAttribute.InformationType.Info, false)]
        ///发动机的功率
        public int EnginePower = 100;

        ///转向时施加的侧向力
        public float LateralSteeringForce = 1f;
        ///最大速度
        public int MaxSpeed = 100;

		[Header("Hover management")]
		[MMInformation("Hover height.\n", MMInformationAttribute.InformationType.Info, false)]
        ///飞行距离:飞行器悬停时离地面的距离
        public float HoverHeight = 1f;

		[MMInformation("Gravity force applied to the vehicle when ground is too far.\n", MMInformationAttribute.InformationType.Info, false)]
        ///将车辆推向地面的力
        public float HoverGravityForce = 1f;

		[MMInformation("Hover force applied.\n", MMInformationAttribute.InformationType.Info, false)]
        ///推力:推动飞行器在空中的力
        public float HoverForce = 1f;

		public float OrientationGroundSpeed = 10f;

		protected RaycastHit _groundRaycastHit;


        /// <summary>
        /// 固定更新:物理控件
        /// </summary>
        public virtual void FixedUpdate()
		{
			// Input management
			if (CurrentGasPedalAmount > 0)
			{
				Accelerate();
			}
				
			Rotation();

			// Physics
			Hover();

			OrientationToGround();
		}

        /// <summary>
        ///管理车辆的加速度
        /// </summary>
        protected virtual void Accelerate() 
		{
			if (Speed < MaxSpeed)
			{
				_rigidbody.AddForce(CurrentGasPedalAmount * transform.forward * EnginePower * Time.fixedDeltaTime);
			}
		}

        /// <summary>
        /// 车辆的旋转使用转向输入
        /// </summary>
        protected virtual void Rotation()
		{
			if (CurrentSteeringAmount != 0)
			{
				transform.Rotate(CurrentSteeringAmount * Vector3.up * SteeringSpeed * Time.fixedDeltaTime);

				Vector3 horizontalVector = transform.right;

				// When rotating, we also apply an opposite tangent force to counter slipping 
				_rigidbody.AddForce(horizontalVector * CurrentSteeringAmount * Time.fixedDeltaTime * LateralSteeringForce * Speed);
			}
		}


		/// <summary>
		/// Management of the hover and gravity of the vehicle
		/// </summary>
		protected virtual void Hover()
		{
			// we enforce isgrounded to false before calculations
			IsGrounded = false;

			// Raycast origin is positionned on the center front of the car
			Vector3 rayOrigin = transform.position + (transform.forward * _collider.bounds.size.z / 2);

			// Raycast to the ground layer
			if (Physics.Raycast(
				rayOrigin, 
				-Vector3.up, 
				out _groundRaycastHit, 
				Mathf.Infinity,
				1 << LayerMask.NameToLayer("Ground")))
			{
				// Raycast hit the ground

				// If distance between vehicle and ground is higher than target height, we apply a force from up to
				// bottom (gravity) to push the vehicle down.
				if (_groundRaycastHit.distance > HoverHeight)
				{
					// Vehicle is too high, We apply gravity force
					_rigidbody.AddForce(-Vector3.up * HoverGravityForce * Time.fixedDeltaTime, ForceMode.Acceleration);
				} 
				else
				{
					// if the vehicle is low enough, it is considered grounded
					IsGrounded = true;
				
					// we determine the distance between current vehicle height and wanted height
					float distanceVehicleToHoverPosition = HoverHeight - _groundRaycastHit.distance;

					float force = distanceVehicleToHoverPosition * HoverForce;

					// we add the hoverforce to the rigidbody
					_rigidbody.AddForce(Vector3.up * force * Time.fixedDeltaTime, ForceMode.Acceleration);
				}
			}
		}

		/// <summary>
		/// Manages orientation of the vehicle depending ground surface normale 
		/// </summary>
		protected virtual void OrientationToGround()
		{
			var rotationTarget = Quaternion.FromToRotation(transform.up, _groundRaycastHit.normal) * transform.rotation;

			transform.rotation = Quaternion.Slerp(transform.rotation, rotationTarget, Time.fixedDeltaTime * OrientationGroundSpeed);
		}
	
		/// <summary>
		/// Draws controller gizmos
		/// </summary>
		public virtual void OnDrawGizmos()
		{
			var collider = GetComponent<BoxCollider>();

			var hoverposition = transform.position + (transform.forward * collider.size.z / 2);

			Gizmos.color = Color.blue;
			Gizmos.DrawSphere(hoverposition, 0.1f);

			if (IsGrounded) 
			{
				Gizmos.color = Color.green;
			} 
			else 
			{
				Gizmos.color = Color.red;
			}

			Gizmos.DrawLine(hoverposition, _groundRaycastHit.point);
		}
	}
}

