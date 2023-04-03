using System.Collections;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

namespace MoreMountains.HighroadEngine 
{
    /// <summary>
    /// 三维相机控制器跟随一个单一的目标在透视
    /// </summary>
    public class ThirdPersonCameraController : CameraController 
	{
		[Header("Camera Controls")]
        /// 到目标的距离
        public float Distance = 5.5f;
        /// 目标与摄像机之间的高度
        public float Height = 2.5f;
        /// 相机的阻尼平移
        public float DampingPosition = 0.02f;
        /// 阻尼转向横向平移
        public float DampingSteering = 0.5f;
        ///转向平移对相机的影响
        public float SteeringOffset = 5f;
        /// camera LookAt目标偏移量
        public float TargetLookUpOffset = 2f;

		public RaceManager raceManager;

		public int counter;
        ///这种相机只能跟踪一个目标
        public override bool IsSinglePlayerCamera 
		{
			get { return true; }
		}

		[HideInInspector]public Transform _target;
		protected BaseController _baseController;
		protected Vector3 currentLateralOffset = Vector3.zero;
		protected Vector3 _moveVelocityReference;
		protected Vector3 _targetPosition;
		protected Vector3 _targetLateralTranslation;

        /// <summary>
        /// 起动时横向速度为零
        /// </summary>
        public virtual void Start()
		{
			currentLateralOffset = Vector3.zero;
		}

        /// <summary>
        ///刷新目标
        /// </summary>
        public override void RefreshTargets()
		{
			_target = null;
		}

		/// <summary>
		/// Updates the camera position
		/// </summary>
		protected override void CameraUpdate() 
		{
            // 我们确定我们想要遵循的目标
            if (!_target)
			{
				if (HumanPlayers.Length > 0)
				{
					_target = HumanPlayers[0];
				}
				else if (BotPlayers.Length > 0)
				{
					_target = BotPlayers[0];
				}

				if (_target != null)
				{
					_baseController = _target.GetComponent<BaseController>();
                }
			}

            //如果我们没有找到任何目标，我们什么都不做，然后退出
            if (_target == null)
			{
				return;
			}
            //目标位置的计算取决于车辆的位置和摄像机参数
            _targetPosition = _target.transform.position                  
				- (_target.transform.forward * Distance) 
				+ (_target.transform.up * Height);

            //我们使用平滑阻尼来改变相机位置
            _camera.transform.position = Vector3.SmoothDamp(_camera.transform.position, _targetPosition, ref _moveVelocityReference, DampingPosition);

            //我们计算新的横向平移值来平滑车辆的旋转
            _targetLateralTranslation = _target.transform.right * (_baseController.CurrentSteeringAmount * SteeringOffset);

            //我们将当前值保存在相机GameObject中，以备下一次更新
            currentLateralOffset = Vector3.Lerp(currentLateralOffset, _targetLateralTranslation, Time.deltaTime * DampingSteering);

            //我们使相机看车辆，修改了横向和向上偏移
            _camera.transform.LookAt(currentLateralOffset + _target.transform.position + (_target.up * TargetLookUpOffset));
			return;
		}
	}
}