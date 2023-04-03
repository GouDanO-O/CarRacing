using UnityEngine;
using MoreMountains.Tools;

namespace MoreMountains.HighroadEngine 
{
    /// <summary>
    /// 摄像机控制器跟踪一个目标或一组目标
    /// 用正字法视图。
    /// </summary>
    public class IsometricCameraController : CameraController 
	{
		[Header("Targets")]
		[MMInformation("当人类玩家处于活动状态时，你是否希望摄像机也跟随机器人?", MMInformationAttribute.InformationType.Info,false)]
        /// 如果设置为true，摄像机将跟随人类和机器人玩家在赛道上。
        public bool FollowBotPlayers;

		[Header("Camera Controls")]
        /// 相机开始聚焦于目标之前的时间(以秒为单位)。允许平滑的跟随效果
        public float DampTime = 0.2f; 
		[MMInformation("For a single fast velocity object, a low damp time value will make the screen moving too fast. Use this different value to have a smooth movement.", MMInformationAttribute.InformationType.Info, false)]
        /// 当目标是一辆车时，相机对焦所需的时间(以秒为单位)
        public float SingleDampTime = 1f;
        /// 距离最远的两辆车形成的矩形周围的空间
        public float ScreenEdgeSize = 4f;
        /// 最大缩放值
        public float CameraMinimalSize = 6.5f;
    
		[Header("Camera Single Human Player Controls")]
        /// 最小缩放尺寸
        public float CameraMaximalSingleSize = 6.5f;
        ///加速度补偿。数值越大，赛车时相机越超前
        public float OffsetSingleCar = 1f;
        ///乘以车辆速度来改变相机的变焦
        public float ZoomSingleCar = 2f;

		protected float _zoomDampSpeed;
		protected Vector3 _moveVelocityReference;
		protected Vector3 _cameraTargetPosition;
		public GameObject _singleTarget;

        public RaceManager raceManager;

        /// <summary>
        /// 确定此相机是否可用于多个目标或单个目标
        /// </summary>
        /// <value><c>true</c> 如果此实例是单人游戏摄像机;否则, <c>false</c>.</value>
        public override bool IsSinglePlayerCamera 
		{
			get { return false; }
		}

        private void OnEnable()
        {
            _singleTarget = raceManager.exitCars[0].gameObject;
        }

        /// <summary>
        /// 返回阻尼时间取决于单个或多个目标
        /// </summary>
        /// <returns>The damp time value.</returns>
        protected virtual float CorrectDampTime()
		{
			if (_singleTarget != null)
			{
				return SingleDampTime;
			}

			return DampTime;
		}

		/// <summary>
		/// 重新刷新目标
		/// </summary>
		public override void RefreshTargets()
		{
			// Nothing
		}

        /// <summary>
        ///摄像头位置更新
        /// </summary>
        protected override void CameraUpdate() 
		{
            //我们需要一份目标名单
            if (HumanPlayers == null && BotPlayers == null)
			{
				return;
			}
            //改变相机位置
            EvaluatePosition();
            //改变相机正字法尺寸
            EvaluateSize();
        }

        /// <summary>
        ///移动摄像机
        /// </summary>
        protected virtual void EvaluatePosition()  
		{
            // 计算车辆的平均位置。
            EvaluateAveragePosition();

            //用平滑的阻尼器改变相机位置
            transform.position = Vector3.SmoothDamp(transform.position, _cameraTargetPosition, ref _moveVelocityReference, CorrectDampTime());
        }

        /// <summary>
        ///求平均位置。
        /// </summary>
        protected virtual void EvaluateAveragePosition()
        {
			Vector3 averagePosition = new Vector3();

            if(_singleTarget != null) 
            {
                averagePosition = _singleTarget.transform.position;
            }
          

            // 位置有一个偏移，取决于偏移值和车辆的速度
            CarAIControl controller = _singleTarget.GetComponent<CarAIControl>();
			Vector3 vehicle = _singleTarget.transform.forward * controller.Speed * OffsetSingleCar;
			averagePosition += vehicle;
            // 新相机所需位置已设置
            _cameraTargetPosition = averagePosition;
        }

        /// <summary>
        /// 放大相机
        /// </summary>
        protected virtual void EvaluateSize() 
		{
			float newSize;

            //在单目标模式，大小取决于车辆的速度
            if (_singleTarget != null)
			{
                // 当只有一个目标时，变焦与汽车的速度成线性
                CarAIControl controller = _singleTarget.GetComponent<CarAIControl>();
				newSize = Mathf.Max(CameraMinimalSize, controller.Speed * ZoomSingleCar);
				newSize = Mathf.Min(CameraMaximalSingleSize, newSize );
            }
			else
			{
				newSize = EvaluateNewSize();
			}

            //设置相机正确大小
            _camera.orthographicSize = Mathf.SmoothDamp(_camera.orthographicSize, newSize, ref _zoomDampSpeed, CorrectDampTime());
        }

        /// <summary>
        /// 查找所需的缩放大小。
        /// </summary>
        /// <returns>The required size.</returns>
        protected virtual float EvaluateNewSize() 
		{
			Vector3 localPosition = transform.InverseTransformPoint(_cameraTargetPosition);

            // 此浮动将存储找到的最佳大小
            float newSize = 0f;

			if (FollowBotPlayers)
			{
				 if (_singleTarget != null)
				 {
				 	Vector3 vehicleLocalPosition = transform.InverseTransformPoint(_singleTarget.transform.position);
				 	Vector3 cameraToVehicleVector = vehicleLocalPosition - localPosition;
                 
                     //新大小是到y或x的距离的最大值
                     newSize = Mathf.Max(newSize, Mathf.Abs(cameraToVehicleVector.y));
				 	newSize = Mathf.Max(newSize, Mathf.Abs(cameraToVehicleVector.x) / _camera.aspect);
				 }
			}

            //我们将大小缓冲区添加到新大小
            newSize += ScreenEdgeSize;
            //新站点不能低于相机的最小尺寸值
            newSize = Mathf.Max(newSize, CameraMinimalSize);
            return newSize;
		}
    }
}