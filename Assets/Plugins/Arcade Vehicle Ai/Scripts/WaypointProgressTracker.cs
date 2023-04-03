using System;
using UnityEngine;

namespace MoreMountains.HighroadEngine
{
    public class WaypointProgressTracker : MonoBehaviour
    {
        //这个脚本可以用于任何应该遵循由路径点标出的路线的对象
        //这个脚本管理沿着路线向前看的量，并跟踪进度和圈数。

        //对我们应该遵循的基于路标的路线的引用
        public WaypointCircuit circuit;

        [SerializeField] private float lookAheadForTargetOffset = 5;
        //沿着我们将要瞄准的路线前进的偏移量

        [SerializeField] private float lookAheadForTargetFactor = .1f;
        //一个乘数，在当前速度的基础上，增加前方要瞄准的距离

        private float lookAheadForSpeedOffset = 50;
        //前方偏移量仅用于航路速度调整(应用于航路点目标的旋转变换)

        private float lookAheadForSpeedFactor = .2f;
        //一个乘数增加的距离，沿着路线的速度调整

        [SerializeField] private ProgressStyle progressStyle = ProgressStyle.SmoothAlongRoute;
        //是沿着路线平滑地更新位置(适合弯曲的路径)，还是在到达每个航路点时更新位置

        private float pointToPointThreshold = 4;
        //在切换目标到下一个路径点时必须到达的路径点的接近度:仅在点对点模式下使用

        public enum ProgressStyle
        {
            SmoothAlongRoute,
            PointToPoint,
        }

        //这些是公共的，其他对象可读-即人工智能知道去哪里!
        public WaypointCircuit.RoutePoint targetPoint { get; private set; }
        public WaypointCircuit.RoutePoint speedPoint { get; private set; }
        public WaypointCircuit.RoutePoint progressPoint { get; private set; }

        public Transform target;

        [HideInInspector]
        public float progressDistance; //在平滑模式下使用的绕路线的进度。
        private int progressNum; //当前路径点编号，用于点对点模式
        private Vector3 lastPosition; //用于计算当前速度(因为我们可能没有刚体组件)
        private float speed; //该对象的当前速度(从上一帧开始的增量计算)

        // setup script properties
        private void Start()
        {

            //我们使用一个变换来表示要瞄准的点，以及即将发生速度变化时考虑的点。
            //这允许该组件将此信息传递给AI，而无需进一步依赖。
            //你可以手动创建一个转换，并将其分配给这个组件*和AI,
            //然后这个组件将更新它，AI可以读取它
            if (target == null)
            {
                target = new GameObject(name + " Waypoint Target").transform;
            }

            Reset();
            circuit = FindObjectOfType<WaypointCircuit>();
        }


        //将对象重置为合理值
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
                //确定我们当前的目标位置(这与当前的进度位置不同，它是沿着路线前进的一定数量)，
                //我们使用lerp作为一种简单的方法来平滑速度随时间的变化
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
                //了解我们目前的进展情况
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
                //点对点模式。只要增加路径点，如果我们足够接近:
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


