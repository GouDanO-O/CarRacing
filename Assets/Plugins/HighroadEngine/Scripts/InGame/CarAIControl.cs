using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using MoreMountains.Tools;
using static ArcadeAiVehicleController;
using static UnityEngine.UI.Image;
using UnityEngine.SocialPlatforms.Impl;
using UnityEngine.Events;

namespace MoreMountains.HighroadEngine
{
    public class CarAIControl : MonoBehaviour
    {
        [MMInformation("在汽车被重置到最后一个检查点之前的秒数。如果值为零，汽车将保持原样。", MMInformationAttribute.InformationType.Info, false)]
        [Range(0, 10)]
        /// 在汽车被重置到最后一个检查点之前的秒数。如果值为零，汽车将保持原样。
        [Tooltip("重置到最后一个检查点之前的秒数")] public float TimeBeforeStuck = 5f;
        [Tooltip("上一个记录的点到当前车辆被卡住的最大重生检测距离")][SerializeField] private float _maximalDistanceStuck = 0.5f;

        protected float _stuckTime = 0f;
        public Vector3 _lastPosition;

        //移动模式
        public MovementMode movementMode;
        //着地检测模式
        public groundCheck GroundCheck;
        public enum groundCheck { rayCast, sphereCaste };
        public enum MovementMode { Velocity, AngularVelocity };
        //地表层级
        public LayerMask drivableSurface; 
        //刚体，车的身体刚体
        public Rigidbody rb, carBody;
        //最大速度、加速度、转向速度
        public float MaxSpeed, accelaration, turn;

        [HideInInspector]
        //射线检测
        public RaycastHit hit;
        //摩擦力曲线
        public AnimationCurve frictionCurve;
        //转向曲线
        public AnimationCurve turnCurve;
        //物理材质
        public PhysicMaterial frictionMaterial;

        /// <summary>
        /// 表现（车身移动、车轮）
        /// </summary>
        [Header("Visuals")]
        public Transform BodyMesh;
        public Transform[] FrontWheels = new Transform[2];
        public Transform[] RearWheels = new Transform[2];
        //车子的速度
        public Vector3 carVelocity;

        public UnityAction OnRespawn;

        //车身的倾斜度
        [Range(0, 10)]
        public float BodyTilt;
        //刹车的宽度
        public float skidWidth;
        //半径
        public float radius;
        //中心点
        public Vector3 origin;

        //目标点
        public Transform target;

        private WaypointCircuit waypointCircuit;

        public List<Vector3> wayPoints=new List<Vector3>();

        public List<Vector3> currentWayPoint=new List<Vector3>();

        public int currentLap { get; protected set; }

        public int currentWaypointCount { get; protected set; }

        private int finisherPosition = 0;

        //AI的设置
        private float TurnAI = 1f;
        private float SpeedAI = 1f;
        private float brakeAI = 0f;
        public float brakeAngle = 30f;

        public float desiredTurning;

        private RaceManager raceManager;

        public virtual void Start()
        {          
            radius = rb.GetComponent<SphereCollider>().radius;
            raceManager=GameObject.FindWithTag("RaceManager").GetComponent<RaceManager>();
            waypointCircuit = GameObject.FindWithTag("WayPoints").GetComponent<WaypointCircuit>();
            for (int i = 0; i < waypointCircuit.points.Length-1; i++)
            {
                wayPoints.Add(waypointCircuit.points[i]);
            }
        }
        void FixedUpdate()
        {
            AILogic();
        }
        private void AILogic()
        {
            carVelocity = carBody.transform.InverseTransformDirection(carBody.velocity);

            if (Mathf.Abs(carVelocity.x) > 0)
            {
                //根据汽车横向速度改变摩擦力
                frictionMaterial.dynamicFriction = frictionCurve.Evaluate(Mathf.Abs(carVelocity.x / 100));
            }
            if (grounded())
            {
                //转向的逻辑
                float sign = Mathf.Sign(carVelocity.z);
                float TurnMultiplyer = turnCurve.Evaluate(carVelocity.magnitude / MaxSpeed);
                if (SpeedAI > 0.1f || carVelocity.z > 1)
                {
                    carBody.AddTorque(Vector3.up * TurnAI * sign * turn * 100 * TurnMultiplyer);
                }
                else if (SpeedAI < -0.1f || carVelocity.z < -1)
                {
                    carBody.AddTorque(Vector3.up * TurnAI * sign * turn * 100 * TurnMultiplyer);
                }
                //刹车逻辑
                if (brakeAI > 0.1f)
                {
                    rb.constraints = RigidbodyConstraints.FreezeRotationX;
                }
                else
                {
                    rb.constraints = RigidbodyConstraints.None;
                }
                //加速逻辑
                if (movementMode == MovementMode.AngularVelocity)
                {
                    if (Mathf.Abs(SpeedAI) > 0.1f)
                    {
                        rb.angularVelocity = Vector3.Lerp(rb.angularVelocity, carBody.transform.right * SpeedAI * MaxSpeed / radius, accelaration * Time.deltaTime);
                    }
                }
                else if (movementMode == MovementMode.Velocity)
                {
                    if (Mathf.Abs(SpeedAI) > 0.1f && brakeAI < 0.1f)
                    {
                        rb.velocity = Vector3.Lerp(rb.velocity, carBody.transform.forward * SpeedAI * MaxSpeed, accelaration / 10 * Time.deltaTime);
                    }
                }
                //身体的旋转
                carBody.MoveRotation(Quaternion.Slerp(carBody.rotation, Quaternion.FromToRotation(carBody.transform.up, hit.normal) * carBody.transform.rotation, 0.12f));
            }
            else
            {
                carBody.MoveRotation(Quaternion.Slerp(carBody.rotation, Quaternion.FromToRotation(carBody.transform.up, Vector3.up) * carBody.transform.rotation, 0.02f));
            }
        }
        private void Update()
        {
            PassByWayPoint();
            Visuals();
            CaculateTurnAngle();     
            if(IsStuck())
            {
                Respawn();
            }
        }

        private void CaculateTurnAngle()
        {
            //计算转向量
            Vector3 aimedPoint = target.position;
            aimedPoint.y = transform.position.y;
            Vector3 aimedDir = (aimedPoint - transform.position).normalized;
            Vector3 myDir = transform.forward;
            myDir.Normalize();
            desiredTurning = Mathf.Abs(Vector3.Angle(myDir, Vector3.ProjectOnPlane(aimedDir, transform.up)));

            float reachedTargetDistance = 1f;
            float distanceToTarget = Vector3.Distance(transform.position, target.position);
            Vector3 dirToMovePosition = (target.position - transform.position).normalized;
            float dot = Vector3.Dot(transform.forward, dirToMovePosition);
            float angleToMove = Vector3.Angle(transform.forward, dirToMovePosition);
            if (angleToMove > brakeAngle)
            {
                if (carVelocity.z > 15)
                {
                    brakeAI = 1;
                }
                else
                {
                    brakeAI = 0;
                }

            }
            else { brakeAI = 0; }

            if (distanceToTarget > reachedTargetDistance)
            {

                if (dot > 0)
                {
                    SpeedAI = 1f;

                    float stoppingDistance = 5f;
                    if (distanceToTarget < stoppingDistance)
                    {
                        brakeAI = 1f;
                    }
                    else
                    {
                        brakeAI = 0f;
                    }
                }
                else
                {
                    float reverseDistance = 5f;
                    if (distanceToTarget > reverseDistance)
                    {
                        SpeedAI = 1f;
                    }
                    else
                    {
                        brakeAI = -1f;
                    }
                }

                float angleToDir = Vector3.SignedAngle(transform.forward, dirToMovePosition, Vector3.up);

                if (angleToDir > 0)
                {
                    TurnAI = 1f * turnCurve.Evaluate(desiredTurning / 90);
                }
                else
                {
                    TurnAI = -1f * turnCurve.Evaluate(desiredTurning / 90);
                }

            }
            else
            {
                if (carVelocity.z > 1f)
                {
                    brakeAI = -1f;
                }
                else
                {
                    brakeAI = 0f;
                }
                TurnAI = 0f;
            }
        }
        /// <summary>
        /// 如果汽车在同一个地方停留太久，我们就会重生到最后一个检查点
        /// </summary>
        protected virtual bool IsStuck()
        {
            if (TimeBeforeStuck > 0)
            {
                if (Vector3.Distance(_lastPosition, transform.position) < _maximalDistanceStuck)
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

        private void Respawn()
        {
            Vector3 resetPosition;

            resetPosition = currentWaypointCount == 0 ? wayPoints[wayPoints.Count - 1] : wayPoints[currentWaypointCount - 1];

            rb.velocity = Vector3.zero;
            transform.position = resetPosition;
        }

        public float DistanceToNextWaypoint
        {
            get
            {
                if(wayPoints.Count==0)
                    return 0f;

                Vector3 wayPoint = wayPoints[currentWaypointCount];
                Debug.Log("DistanceToNextWayPoint" + Vector3.Distance(transform.position, wayPoint));
                return Vector3.Distance(transform.position, wayPoint);
            }
        }

        public float Speed
        {
            get
            {
                return rb.velocity.magnitude;
            }
        }

        public int Score
        {
            get
            {
                if(currentWayPoint!=null)
                {
                    Debug.Log("Score:" + (currentLap * currentWayPoint.Count) + currentWaypointCount);
                    return (currentLap * currentWayPoint.Count) + currentWaypointCount;
                }
                else
                { 
                    return 0; 
                }
            }
        }

        public int FinalRank
        {
            get 
            {
                Debug.Log("FinalRank"+finisherPosition);
                return finisherPosition; 
            }
        }

        public bool HasJustFinished(int finalRankPos)
        {
            if(finalRankPos>0)
                return false;

            bool raceEndedForPlayer;
            raceEndedForPlayer = raceManager.ClosedLoopTrack ? (Score >= (raceManager.Laps * currentWayPoint.Count)) : (Score >= currentWayPoint.Count);

            if (raceEndedForPlayer)
                finisherPosition = finalRankPos;

            Debug.Log(raceEndedForPlayer);
            return raceEndedForPlayer;
        }

        private void PassByWayPoint()
        {
            if (Vector3.Cross(wayPoints[currentWaypointCount], transform.position).magnitude>=0)
            {
                currentWayPoint.Add(wayPoints[currentWaypointCount]);
                currentWaypointCount++;
            }
        }

        //车子视觉效果的变更
        public void Visuals()
        {
            //轮子
            foreach (Transform FW in FrontWheels)
            {
                FW.localRotation = Quaternion.Slerp(FW.localRotation, Quaternion.Euler(FW.localRotation.eulerAngles.x,
                                   30 * TurnAI, FW.localRotation.eulerAngles.z), 0.1f);
                FW.GetChild(0).localRotation = rb.transform.localRotation;
            }
            RearWheels[0].localRotation = rb.transform.localRotation;
            RearWheels[1].localRotation = rb.transform.localRotation;

            //车身
            if (carVelocity.z > 1)
            {
                BodyMesh.localRotation = Quaternion.Slerp(BodyMesh.localRotation, Quaternion.Euler(Mathf.Lerp(0, -5, carVelocity.z / MaxSpeed),
                                   BodyMesh.localRotation.eulerAngles.y, Mathf.Clamp(desiredTurning * TurnAI, -BodyTilt, BodyTilt)), 0.05f);
            }
            else
            {
                BodyMesh.localRotation = Quaternion.Slerp(BodyMesh.localRotation, Quaternion.Euler(0, 0, 0), 0.05f);
            }
        }

        /// <summary>
        /// 检查车子是否着地
        /// </summary>
        /// <returns></returns>
        public bool grounded()
        {
            origin = rb.position + rb.GetComponent<SphereCollider>().radius * Vector3.up;
            var direction = -transform.up;
            var maxdistance = rb.GetComponent<SphereCollider>().radius + 0.2f;

            if (GroundCheck == groundCheck.rayCast)
            {
                if (Physics.Raycast(rb.position, Vector3.down, out hit, maxdistance, drivableSurface))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }

            else if (GroundCheck == groundCheck.sphereCaste)
            {
                if (Physics.SphereCast(origin, radius + 0.1f, direction, out hit, maxdistance, drivableSurface))
                {
                    return true;

                }
                else
                {
                    return false;
                }
            }
            else { return false; }
        }

        private void OnDrawGizmos()
        {
            //debug gizmos
            radius = rb.GetComponent<SphereCollider>().radius;
            float width = 0.02f;
            if (!Application.isPlaying)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireCube(rb.transform.position + ((radius + width) * Vector3.down), new Vector3(2 * radius, 2 * width, 4 * radius));
                if (GetComponent<BoxCollider>())
                {
                    Gizmos.color = Color.green;
                    Gizmos.DrawWireCube(transform.position, GetComponent<BoxCollider>().size);
                }

            }
        }
    }   
}
