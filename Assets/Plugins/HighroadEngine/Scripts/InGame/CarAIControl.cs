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
        [MMInformation("�����������õ����һ������֮ǰ�����������ֵΪ�㣬����������ԭ����", MMInformationAttribute.InformationType.Info, false)]
        [Range(0, 10)]
        /// �����������õ����һ������֮ǰ�����������ֵΪ�㣬����������ԭ����
        [Tooltip("���õ����һ������֮ǰ������")] public float TimeBeforeStuck = 5f;
        [Tooltip("��һ����¼�ĵ㵽��ǰ��������ס���������������")][SerializeField] private float _maximalDistanceStuck = 0.5f;

        protected float _stuckTime = 0f;
        public Vector3 _lastPosition;

        //�ƶ�ģʽ
        public MovementMode movementMode;
        //�ŵؼ��ģʽ
        public groundCheck GroundCheck;
        public enum groundCheck { rayCast, sphereCaste };
        public enum MovementMode { Velocity, AngularVelocity };
        //�ر�㼶
        public LayerMask drivableSurface; 
        //���壬�����������
        public Rigidbody rb, carBody;
        //����ٶȡ����ٶȡ�ת���ٶ�
        public float MaxSpeed, accelaration, turn;

        [HideInInspector]
        //���߼��
        public RaycastHit hit;
        //Ħ��������
        public AnimationCurve frictionCurve;
        //ת������
        public AnimationCurve turnCurve;
        //�������
        public PhysicMaterial frictionMaterial;

        /// <summary>
        /// ���֣������ƶ������֣�
        /// </summary>
        [Header("Visuals")]
        public Transform BodyMesh;
        public Transform[] FrontWheels = new Transform[2];
        public Transform[] RearWheels = new Transform[2];
        //���ӵ��ٶ�
        public Vector3 carVelocity;

        public UnityAction OnRespawn;

        //�������б��
        [Range(0, 10)]
        public float BodyTilt;
        //ɲ���Ŀ��
        public float skidWidth;
        //�뾶
        public float radius;
        //���ĵ�
        public Vector3 origin;

        //Ŀ���
        public Transform target;

        private WaypointCircuit waypointCircuit;

        public List<Vector3> wayPoints=new List<Vector3>();

        public List<Vector3> currentWayPoint=new List<Vector3>();

        public int currentLap { get; protected set; }

        public int currentWaypointCount { get; protected set; }

        private int finisherPosition = 0;

        //AI������
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
                //�������������ٶȸı�Ħ����
                frictionMaterial.dynamicFriction = frictionCurve.Evaluate(Mathf.Abs(carVelocity.x / 100));
            }
            if (grounded())
            {
                //ת����߼�
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
                //ɲ���߼�
                if (brakeAI > 0.1f)
                {
                    rb.constraints = RigidbodyConstraints.FreezeRotationX;
                }
                else
                {
                    rb.constraints = RigidbodyConstraints.None;
                }
                //�����߼�
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
                //�������ת
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
            //����ת����
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
        /// ���������ͬһ���ط�ͣ��̫�ã����Ǿͻ����������һ������
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

        //�����Ӿ�Ч���ı��
        public void Visuals()
        {
            //����
            foreach (Transform FW in FrontWheels)
            {
                FW.localRotation = Quaternion.Slerp(FW.localRotation, Quaternion.Euler(FW.localRotation.eulerAngles.x,
                                   30 * TurnAI, FW.localRotation.eulerAngles.z), 0.1f);
                FW.GetChild(0).localRotation = rb.transform.localRotation;
            }
            RearWheels[0].localRotation = rb.transform.localRotation;
            RearWheels[1].localRotation = rb.transform.localRotation;

            //����
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
        /// ��鳵���Ƿ��ŵ�
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
