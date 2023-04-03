using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.HighroadEngine
{
    /// <summary>
    ///这个类处理附加到SolidWheel上的轮子的滑痕生产
    /// </summary>
    [RequireComponent(typeof(SolidWheelBehaviour))]
	public class SolidWheelSkidmarks : MonoBehaviour
	{
        ///在这个速度以下车轮不会留下滑痕
        public float MinimalSkidMarksSlideSpeed = 5f;
        /// 产生滑痕的最大数量
        public float MaxSkidIntensity = 10f;

		public GameObject[] fxs;

		protected SkidmarksManager SkidMarksObject;
		protected SolidWheelBehaviour _wheel;
		protected int lastSkid = 0;

        /// <summary>
        ///我们寻找必须在场景中才能发挥作用的SkidmarksManager
        ///对于所有车辆，一个Skidmarks管理器就足够了
        /// </summary>
        public virtual void Start()
		{
			SkidMarksObject = Object.FindObjectOfType<SkidmarksManager>();

			if (SkidMarksObject == null)
			{
				Debug.LogWarning("这个类在场景中需要一个SkidmarksManager。请加一个。");
			}
				
			_wheel = GetComponent<SolidWheelBehaviour>();
		}
        /// <summary>
        ///更新滑动标记的属性
        /// </summary>
        public virtual void Update()
		{
			if (SkidMarksObject != null)
			{
				float slideSpeed = _wheel.VehicleController.SlideSpeed;

                // 如果车轮触地并以适当的速度
                if (_wheel.IsGrounded && (Mathf.Abs(slideSpeed) > MinimalSkidMarksSlideSpeed))
				{
					Vector3 velocity = _wheel.VehicleController.GetComponent<Rigidbody>().velocity;
					float intensity = Mathf.Clamp01(Mathf.Abs(slideSpeed) / MaxSkidIntensity);
					Vector3 skidPoint = _wheel.PhysicsHit.point;
					lastSkid = SkidMarksObject.AddSkidMark(skidPoint, _wheel.PhysicsHit.normal, intensity, lastSkid);
				} 
				else
				{
                    
                    // 我们将最后一个滑段的ID重置为开始一个新的滑段标记
                    lastSkid = -1;
				}
			}
		}
    }
}
