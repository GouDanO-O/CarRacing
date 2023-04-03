using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.HighroadEngine
{
    /// <summary>
    /// 一个实用工具类，允许您扩展Solid车辆的行为。
    /// 例如添加车灯、滑痕管理等。
    /// 如果需要，您只需实现更新和初始化。
    /// </summary>
    [RequireComponent(typeof(SolidController))]
	public abstract class SolidBehaviourExtender : MonoBehaviour 
	{
		protected SolidController _controller;

        /// <summary>
        ///控制器的初始化
        /// </summary>
        public virtual void Start()
		{
			_controller = GetComponent<SolidController>();
			//_controller.OnCollisionEnterWithOther += OnVehicleCollisionEnter;
			Initialization();
		}

        /// <summary>
        ///使用此方法初始化子类中的对象
        /// </summary>
        public virtual void Initialization()
		{
			// Nothing here
		}

        /// <summary>
        ///在子类中更新此方法
        /// </summary>
        public abstract void Update();

        /// <summary>
        ///使用这个方法来描述当车辆与某物相撞时发生的情况
        /// </summary>
        /// <param name="tag">Objet en collision</param>
        public virtual void OnVehicleCollisionEnter(Collision other)
		{
			// Nothing here
		}
	}
}