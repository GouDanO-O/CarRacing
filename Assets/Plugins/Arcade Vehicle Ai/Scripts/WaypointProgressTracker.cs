using System;
using UnityEngine;

namespace MoreMountains.HighroadEngine
{
    public class WaypointProgressTracker : MonoBehaviour
    {
        //����ű����������κ�Ӧ����ѭ��·��������·�ߵĶ���
        //����ű���������·����ǰ�������������ٽ��Ⱥ�Ȧ����

        //������Ӧ����ѭ�Ļ���·���·�ߵ�����
        public WaypointCircuit circuit;

        [SerializeField] private float lookAheadForTargetOffset = 5;
        //�������ǽ�Ҫ��׼��·��ǰ����ƫ����

        [SerializeField] private float lookAheadForTargetFactor = .1f;
        //һ���������ڵ�ǰ�ٶȵĻ����ϣ�����ǰ��Ҫ��׼�ľ���

        private float lookAheadForSpeedOffset = 50;
        //ǰ��ƫ���������ں�·�ٶȵ���(Ӧ���ں�·��Ŀ�����ת�任)

        private float lookAheadForSpeedFactor = .2f;
        //һ���������ӵľ��룬����·�ߵ��ٶȵ���

        [SerializeField] private ProgressStyle progressStyle = ProgressStyle.SmoothAlongRoute;
        //������·��ƽ���ظ���λ��(�ʺ�������·��)�������ڵ���ÿ����·��ʱ����λ��

        private float pointToPointThreshold = 4;
        //���л�Ŀ�굽��һ��·����ʱ���뵽���·����Ľӽ���:���ڵ�Ե�ģʽ��ʹ��

        public enum ProgressStyle
        {
            SmoothAlongRoute,
            PointToPoint,
        }

        //��Щ�ǹ����ģ���������ɶ�-���˹�����֪��ȥ����!
        public WaypointCircuit.RoutePoint targetPoint { get; private set; }
        public WaypointCircuit.RoutePoint speedPoint { get; private set; }
        public WaypointCircuit.RoutePoint progressPoint { get; private set; }

        public Transform target;

        [HideInInspector]
        public float progressDistance; //��ƽ��ģʽ��ʹ�õ���·�ߵĽ��ȡ�
        private int progressNum; //��ǰ·�����ţ����ڵ�Ե�ģʽ
        private Vector3 lastPosition; //���ڼ��㵱ǰ�ٶ�(��Ϊ���ǿ���û�и������)
        private float speed; //�ö���ĵ�ǰ�ٶ�(����һ֡��ʼ����������)

        // setup script properties
        private void Start()
        {

            //����ʹ��һ���任����ʾҪ��׼�ĵ㣬�Լ����������ٶȱ仯ʱ���ǵĵ㡣
            //����������������Ϣ���ݸ�AI���������һ��������
            //������ֶ�����һ��ת��������������������*��AI,
            //Ȼ������������������AI���Զ�ȡ��
            if (target == null)
            {
                target = new GameObject(name + " Waypoint Target").transform;
            }

            Reset();
            circuit = FindObjectOfType<WaypointCircuit>();
        }


        //����������Ϊ����ֵ
        public void Reset()
        {
            progressDistance = 0;
            progressNum = 0;
            if (progressStyle == ProgressStyle.PointToPoint)
            {
                target.position = circuit.Waypoints[progressNum].position;
                target.rotation = circuit.Waypoints[progressNum].rotation;
            }
        }

        private void Update()
        {
            if (progressStyle == ProgressStyle.SmoothAlongRoute)
            {
                //ȷ�����ǵ�ǰ��Ŀ��λ��(���뵱ǰ�Ľ���λ�ò�ͬ����������·��ǰ����һ������)��
                //����ʹ��lerp��Ϊһ�ּ򵥵ķ�����ƽ���ٶ���ʱ��ı仯
                if (Time.deltaTime > 0)
                {
                    speed = GetComponent<CarAIControl>().carVelocity.z;
                }
                target.position =
                    circuit.GetRoutePoint(progressDistance + lookAheadForTargetOffset + lookAheadForTargetFactor * speed)
                           .position;
                target.rotation =
                    Quaternion.LookRotation(
                        circuit.GetRoutePoint(progressDistance + lookAheadForSpeedOffset + lookAheadForSpeedFactor * speed)
                               .direction);
                //�˽�����Ŀǰ�Ľ�չ���
                progressPoint = circuit.GetRoutePoint(progressDistance);
                Vector3 progressDelta = progressPoint.position - transform.position;
                if (Vector3.Dot(progressDelta, progressPoint.direction) < 0)
                {
                    progressDistance += progressDelta.magnitude * 0.5f;
                }

                lastPosition = transform.position;
            }
            else
            {
                //��Ե�ģʽ��ֻҪ����·���㣬��������㹻�ӽ�:
                Vector3 targetDelta = target.position - transform.position;
                if (targetDelta.magnitude < pointToPointThreshold)
                {
                    progressNum = (progressNum + 1) % circuit.Waypoints.Length;
                }

                target.position = circuit.Waypoints[progressNum].position;
                target.rotation = circuit.Waypoints[progressNum].rotation;

                // get our current progress along the route
                progressPoint = circuit.GetRoutePoint(progressDistance);
                Vector3 progressDelta = progressPoint.position - transform.position;
                if (Vector3.Dot(progressDelta, progressPoint.direction) < 0)
                {
                    progressDistance += progressDelta.magnitude;
                }
                lastPosition = transform.position;
            }
        }

        private void OnDrawGizmos()
        {
            if (Application.isPlaying)
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawLine(transform.position, target.position);
                Gizmos.color = Color.blue;
                Gizmos.DrawWireSphere(circuit.GetRoutePosition(progressDistance), 0.2f);
                Gizmos.DrawLine(transform.position, circuit.GetRoutePosition(progressDistance));
                Gizmos.DrawLine(target.position, target.position + target.forward);
                Gizmos.color = Color.magenta;
                Gizmos.DrawWireSphere(target.position, 1);
            }
        }
    }
}


