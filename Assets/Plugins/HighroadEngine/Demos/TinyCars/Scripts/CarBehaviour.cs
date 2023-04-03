using UnityEngine;
using System.Collections;
using MoreMountains.Tools;

namespace MoreMountains.HighroadEngine 
{
    /// <summary>
    /// 汽车行为课。管理赛车在比赛中的行为。
    /// 这个类负责汽车释放的粒子、烟雾、声音和运动部件。
    /// 它依赖于CarController类来获取速度或转向信息等数据
    /// </summary>
    [RequireComponent(typeof(CarController))]
	public class CarBehaviour : MonoBehaviour 
	{
		[Header("Animation Settings")]
		[Range(0f, 180f)]
		public float MaximumWheelSteeringAngle = 30f;
		[Range(0f, 90f)]
		public float MaximumCarBodyRollAngle = 12f;

		[Header("Particles Settings")]
		[Range(0f, 1f)]
		public float StartsLeavingSkidmarksAt = 0.8f;
		public bool EmitRocksOnlyOffRoad = false;
		[Range(0f, 1f)]
		public float StartsEmittingRocksAt = 0.6f;
		[Range(0f, 5f)]
		public float SmokesMultiplier = 2f;

		[Header("Particle Systems")]
		public ParticleSystem SmokeLeft;
		public ParticleSystem SmokeRight;
		public ParticleSystem RocksLeft;
		public ParticleSystem RocksRight;

		[Header("Car parts")]
		public GameObject FrontRightWheel;
		public GameObject FrontLeftWheel;
		public GameObject BackRightWheel;
		public GameObject BackLeftWheel;
		public GameObject CarBody;

		[Header("Sounds")]
		public AudioClip EngineSound;
		public AudioClip CrashSound;
		public int MinimalCrashSpeed = 2;

		protected float _smokeAngle;
		protected float _smokeStartSize;

		protected ParticleSystem.ShapeModule _leftSmokeShape;
		protected ParticleSystem.ShapeModule _rightSmokeShape;

		protected GameObject _skidmarksLeft;
		protected GameObject _skidmarksRight;

		protected ParticleSystem.EmissionModule _smokeLeftEmission;
		protected ParticleSystem.EmissionModule _smokeRightEmission;
		protected ParticleSystem.EmissionModule _rocksLeftEmission;
		protected ParticleSystem.EmissionModule _rocksRightEmission;

		protected SoundManager _soundManager;
		protected AudioSource _engineSound;
		protected float _engineSoundPitch;
		protected float _steeringBodyRollValue;

		protected CarController _carController;

        /// <summary>
        ///初始化组件
        /// </summary>
        protected virtual void Start() 
		{
			_carController = GetComponent<CarController>();
			_soundManager = FindObjectOfType<SoundManager>();

			_leftSmokeShape = SmokeLeft.shape;
			_rightSmokeShape = SmokeRight.shape;
			_smokeAngle = SmokeLeft.shape.angle;

			_smokeLeftEmission = SmokeLeft.emission;
			_smokeRightEmission = SmokeRight.emission;
			_rocksLeftEmission = RocksLeft.emission;
			_rocksRightEmission = RocksRight.emission;

			_smokeLeftEmission.enabled = true;
			_smokeRightEmission.enabled = true;
			_rocksLeftEmission.enabled = false;
			_rocksRightEmission.enabled = false;

			_skidmarksLeft = Instantiate(
				Resources.Load<GameObject>("Particles/Skidmarks"),
				SmokeLeft.transform.position,SmokeLeft.transform.rotation) as GameObject;
			
			_skidmarksRight = Instantiate(
				Resources.Load<GameObject>("Particles/Skidmarks"),
				SmokeRight.transform.position,SmokeLeft.transform.rotation) as GameObject;
			
			_skidmarksLeft.transform.parent = SmokeLeft.transform;
			_skidmarksRight.transform.parent = SmokeRight.transform;		

			if (EngineSound != null)
			{
				_soundManager = FindObjectOfType<SoundManager>();
				if (_soundManager != null)
				{
					_engineSound = _soundManager.PlayLoop(EngineSound, transform.position);

					if (_engineSound != null)
					{
						_engineSoundPitch = _engineSound.pitch;
						_engineSound.volume = 0;
					}
				}
			}
		}

        /// <summary>
        /// 在更新，我们使我们的汽车的身体滚动，转动它的车轮和处理效果
        /// </summary>
        protected virtual void Update()
		{
			CarBodyRoll();

			TurnWheels();

			ManageSounds();

			if (_carController.IsGrounded)
			{
				SmokeControl();
				SkidMarks();
				EmitRocks();
			}
		}

        /// <summary>
        /// 管理声音。
        /// </summary>
        protected virtual void ManageSounds()
		{
			if (_engineSound == null)
			{
				return;
			}

			_engineSound.volume = 0.1f 
				+ Mathf.Abs(_carController.NormalizedSpeed) 
				- Mathf.Abs(_carController.CurrentSteeringAmount / 2);
			
			_engineSound.pitch = _engineSoundPitch
				* (Mathf.Abs(_carController.NormalizedSpeed * 3) + Mathf.Abs(_carController.CurrentSteeringAmount * 2));
		}

        /// <summary>
        /// 管理汽车的车身滚动
        /// </summary>
        protected virtual void CarBodyRoll()
		{
			_steeringBodyRollValue = Mathf.Lerp(_steeringBodyRollValue, _carController.CurrentSteeringAmount, Time.deltaTime);
			CarBody.transform.localEulerAngles = _steeringBodyRollValue
				* MaximumCarBodyRollAngle 
				* Mathf.Abs(_carController.NormalizedSpeed) 
				* Vector3.forward;
		}

        /// <summary>
        /// 控制烟雾效果
        /// </summary>
        protected virtual void SmokeControl()
		{
			_leftSmokeShape.angle = Mathf.Abs(_carController.NormalizedSpeed) * _smokeAngle;
			_rightSmokeShape.angle = Mathf.Abs(_carController.NormalizedSpeed) * _smokeAngle;

			float startSizeMultiplier = Mathf.Abs(_carController.CurrentGasPedalAmount) * SmokesMultiplier;

			ParticleSystem.MainModule leftMain = SmokeLeft.main;
			ParticleSystem.MainModule rightMain = SmokeRight.main;

			leftMain.startSizeMultiplier = startSizeMultiplier;
			rightMain.startSizeMultiplier = startSizeMultiplier;
		}

        /// <summary>
        /// 转动轮子。
        /// </summary>
        protected virtual void TurnWheels()
		{
			FrontLeftWheel.transform.localEulerAngles = _carController.CurrentSteeringAmount 
				* MaximumWheelSteeringAngle
				* Vector3.up;
			
			FrontRightWheel.transform.localEulerAngles = _carController.CurrentSteeringAmount
				* MaximumWheelSteeringAngle 
				* Vector3.up;
		}

        /// <summary>
        ///处理刹车痕迹时，车辆转向足够
        /// </summary>
        protected virtual void SkidMarks()
		{	
			Vector3 leftPosition = SmokeLeft.transform.position;
			leftPosition.y -= 0.15f; 
			Vector3 rightPosition = SmokeRight.transform.position;
			rightPosition.y -= 0.15f;


            // 如果我们需要放滑痕，我们定位我们的滑痕发射器在地面水平
            if (Mathf.Abs(_carController.CurrentSteeringAmount) > StartsLeavingSkidmarksAt)
			{
				_skidmarksLeft.transform.position = leftPosition;
				_skidmarksRight.transform.position = rightPosition;
			}
			else
			{
                // 否则我们就把他们藏在地下。很难看，但这是TrailRenderer组件的一个限制。
                _skidmarksLeft.transform.position = leftPosition + Vector3.down * 3;
				_skidmarksRight.transform.position = rightPosition + Vector3.down * 3;
			}
		}

        /// <summary>
        /// 岩石发射
        /// </summary>
        protected virtual void EmitRocks()
		{
			_rocksLeftEmission.enabled=false;
			_rocksRightEmission.enabled=false;

            // 只有当汽车接地时
            if (!_carController.IsGrounded) 
			{
				return;
			}

            // 如果岩石仅限于越野，我们检查这个条件
            if (EmitRocksOnlyOffRoad && !_carController.IsOffRoad) 
			{
				return;
			}

            // 如果我们需要开始扔石头，我们打开发射器，否则关闭
            if (Mathf.Abs(_carController.CurrentSteeringAmount) > StartsEmittingRocksAt)
			{
				_rocksLeftEmission.enabled = true;
				_rocksRightEmission.enabled = true;
			}
		}

        /// <summary>
        ///用于碰撞时播放碰撞声
        /// </summary>
        /// <param name="other">Other.</param>
        protected virtual void OnCollisionEnter(Collision collision)
		{
			if (CrashSound != null)
			{
				if (collision.gameObject.layer != LayerMask.NameToLayer("Ground"))
				{
					if (_soundManager != null)
					{
						if (collision.relativeVelocity.magnitude >= MinimalCrashSpeed) 
						{
							_soundManager.PlaySound(CrashSound, transform.position, true);
						}
					}
				}
			}
	    }

        /// <summary>
        /// 我们在游戏结束时删除引擎声音。
        /// </summary>
        protected virtual void OnDestroy()
		{
			if (_engineSound != null)
			{
				_engineSound.Stop();
				Destroy(_engineSound);
			}
		}
	}
}
