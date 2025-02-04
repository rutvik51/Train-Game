namespace Dreamteck.Splines.Examples
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using Dreamteck.Splines;
    using System;
    using System.Linq;
    using DG.Tweening;

    public class TrainEngine : MonoBehaviour
    {
        public SplineTracer _tracer = null;
        private double _lastPercent = 0.0;
        private double lastPercentOfPath = 0.0;
        private Wagon _wagon;
        public LastWagon lastWagon;
        public List<TravelPoint> path;
        public List<Wagon> wagons;
        public SplineComputer splinePath;
        public SplineComputer linePath;
        public SplineComputer lineSmoothPath;
        public SplineProjector splineProjector;
        public SplineProjector mainProjector;

        public SplineProjector frontProjector;
        public SplineProjector backProjector;
        public List<SplineProjector> onLineCheckProjectors;
        private bool isInLastSpline;
        private bool moving;
        public GameObject startPoint;
        public GameObject checkPoint;
        public MeshRenderer meshLineRenderer;

        public GameObject frontPoint;
        public GameObject backPoint;

        public DrawLine drawLine;
        TrainView trainView;

        bool isComplete;
        bool isReverse;


        public void OnCollisionAction(Transform point)
        {
            if (isComplete) return;
            isComplete = true;

            List<SplinePoint> splineNodes = new List<SplinePoint>();
            SplinePoint emptyPoint = new SplinePoint();
            SplinePoint emptyPoint2 = new SplinePoint();

            emptyPoint.SetPosition(Vector3.forward * -50);
            emptyPoint2.SetPosition(Vector3.one * -49);

            splineNodes.Add(emptyPoint);
            splineNodes.Add(emptyPoint2);
            linePath.SetPoints(splineNodes.ToArray());
            lineSmoothPath.SetPoints(splineNodes.ToArray());

            List<SplinePoint> pointsCurrent = splinePath.GetPoints().ToList();
            SplinePoint cavePoint = new SplinePoint();
            cavePoint.SetPosition(point.position);
            cavePoint.size = 1;
            if (!isReverse)
                pointsCurrent.Add(cavePoint);
            else
                pointsCurrent.Insert(0, cavePoint);
            splinePath.SetPoints(pointsCurrent.ToArray());

            DOVirtual.DelayedCall(1.3f, () =>
        {
            transform.parent.gameObject.SetActive(false);
        });
        }
        private void Awake()
        {
            _wagon = GetComponent<Wagon>();
        }

        void Start()
        {
            trainView = transform.parent.gameObject.transform.GetComponent<TrainView>();
            _tracer.onMotionApplied += OnMotionApplied;

            if (_tracer is SplineFollower)
            {
                SplineFollower follower = (SplineFollower)_tracer;
                follower.onBeginningReached += FollowerOnBeginningReached;
                follower.onEndReached += FollowerOnEndReached;
            }
        }
        public void ResetPath()
        {
            GetOnLineSplineComputer();
            GetComponent<SplineFollower>().followSpeed = 0;
            Debug.LogError("Stop");
            trainView.StopTrainAnimation();
            moving = false;
            isInLastSpline = false;
            path = new List<TravelPoint>();
            List<SplinePoint> splineNodes = new List<SplinePoint>();
            SplinePoint emptyPoint = new SplinePoint();
            SplinePoint emptyPoint2 = new SplinePoint();

            emptyPoint.SetPosition(Vector3.forward * -50);
            emptyPoint2.SetPosition(Vector3.one * -49);

            splineNodes.Add(emptyPoint);
            splineNodes.Add(emptyPoint2);
            linePath.SetPoints(splineNodes.ToArray());
            lineSmoothPath.SetPoints(splineNodes.ToArray());
        }
        public void Reverse()
        {
            lastWagon.gameObject.layer = LayerMask.NameToLayer("Wagon");
            gameObject.layer = LayerMask.NameToLayer("Default");
            frontProjector.projectTarget = lastWagon.frontPoint.transform;
            backProjector.projectTarget = lastWagon.backPoint.transform;
            foreach (var item in onLineCheckProjectors)
            {
                item.projectTarget = lastWagon.checkPoint.transform;
            }
        }
        public void Normal()
        {
            GameController.instance.CompleteTutorial();
            UIController.instance.gamePlayPanel.ReverseBtnEnableState(true);
            gameObject.layer = LayerMask.NameToLayer("Wagon");
            lastWagon.gameObject.layer = LayerMask.NameToLayer("Default");
            frontProjector.projectTarget = frontPoint.transform;
            backProjector.projectTarget = backPoint.transform;
            foreach (var item in onLineCheckProjectors)
            {
                item.projectTarget = checkPoint.transform;
            }

        }
        public void GetOnLineSplineComputer()
        {
            Transform check = checkPoint.transform;
            if (TrainsManager.Instance.isReverse)
                check = lastWagon.checkPoint.transform;
            float dis = Vector3.Distance(onLineCheckProjectors[0].EvaluatePosition(onLineCheckProjectors[0].GetPercent()), check.position);
            int currentLineIndex = 0;

            for (int i = 1; i < onLineCheckProjectors.Count; i++)
            {
                if (Vector3.Distance(onLineCheckProjectors[i].EvaluatePosition(onLineCheckProjectors[i].GetPercent()), check.position) < dis)
                {
                    currentLineIndex = i;
                    dis = Vector3.Distance(onLineCheckProjectors[i].EvaluatePosition(onLineCheckProjectors[i].GetPercent()), check.position);
                }
            }
            frontProjector.spline = onLineCheckProjectors[currentLineIndex].spline;
            backProjector.spline = onLineCheckProjectors[currentLineIndex].spline;
        }
        private bool CheckLastTwoPoints(PointHolder pointOne, PointHolder pointTwo)
        {
            if (pointOne.boundedPoint != null && pointTwo.boundedPoint != null && pointTwo.boundedPoint.spline != null && pointOne.boundedPoint.spline != null)
            {
                if (pointOne.boundedPoint.spline.GetNodes().TryGetValue(pointOne.boundedPoint.pointNo, out Node node1) && pointTwo.boundedPoint.spline.GetNodes().TryGetValue(pointTwo.boundedPoint.pointNo, out Node node2))
                {
                    return true;
                }

            }
            return false;

        }
        public IEnumerator SetPath(List<PointHolder> pointsAndProjector, List<TravelPoint> travelPath, double lastPercent)
        {
            moving = true;
            trainView.StartTrainAnimation();
            Debug.LogError("Start");
            //splineLinePath.SetPoints(pointsAndProjector.Select(x => x.point).ToArray());

            if (TrainsManager.Instance.isReverse)
            {
                isReverse = true;
                pointsAndProjector.Reverse();
                if (CheckLastTwoPoints(pointsAndProjector[pointsAndProjector.Count - 2], pointsAndProjector[pointsAndProjector.Count - 1]))
                    pointsAndProjector.Remove(pointsAndProjector[pointsAndProjector.Count - 1]);

                List<PointHolder> samePoints = new List<PointHolder>();
                for (int i = 0; i < pointsAndProjector.Count; i++)
                {
                    if (i != pointsAndProjector.Count - 1 && pointsAndProjector[i].point.position == pointsAndProjector[i + 1].point.position)
                    {
                        samePoints.Add(pointsAndProjector[i]);
                    }
                }
                foreach (var item in samePoints)
                    pointsAndProjector.Remove(item);
                List<SplinePoint> points = pointsAndProjector.Select(x => x.point).ToList();
                SplinePoint wagonPoint = new SplinePoint();

                for (int i = wagons.Count - 2; i >= 0; i--)
                {
                    wagonPoint = new SplinePoint();
                    wagonPoint.SetPosition(wagons[i].transform.position);
                    wagonPoint.size = 1;
                    wagonPoint.normal = Vector3.up;
                    wagonPoint.color = Color.white;
                    points.Add(wagonPoint);
                }
                wagonPoint = new SplinePoint();

                wagonPoint.SetPosition(transform.position);
                wagonPoint.size = 1;
                wagonPoint.normal = Vector3.up;
                wagonPoint.color = Color.white;
                points.Add(wagonPoint);
                startPoint.transform.position = lastWagon.transform.position;
                yield return new WaitForSeconds(0.075f);
                _tracer.enabled = false;

                foreach (var item in wagons)
                {
                    item.tracer.enabled = false;
                }

                _tracer.GetComponent<SplineFollower>().applyDirectionRotation = false;
                splinePath.SetPoints(points.ToArray());
                _tracer.spline = splinePath;
                yield return new WaitForSeconds(0.075f);

                _tracer.SetPercent(1);
                _tracer.enabled = true;
                foreach (var item in wagons)
                {
                    item.tracer.enabled = true;
                }
                GetComponent<SplineFollower>().followSpeed = -7;

                SwitchDirection(Spline.Direction.Backward);

                _wagon.UpdateWagon(_wagon, splinePath, Spline.Direction.Forward);

                lastWagon.Set();
            }
            else
            {
                isReverse = false;
                if (CheckLastTwoPoints(pointsAndProjector[pointsAndProjector.Count - 2], pointsAndProjector[pointsAndProjector.Count - 1]))
                    pointsAndProjector.Remove(pointsAndProjector[pointsAndProjector.Count - 1]);

                List<PointHolder> samePoints = new List<PointHolder>();
                for (int i = 0; i < pointsAndProjector.Count; i++)
                {
                    if (i != pointsAndProjector.Count - 1 && pointsAndProjector[i].point.position == pointsAndProjector[i + 1].point.position)
                    {
                        samePoints.Add(pointsAndProjector[i]);
                    }
                }
                foreach (var item in samePoints)
                    pointsAndProjector.Remove(item);
                List<SplinePoint> points = pointsAndProjector.Select(x => x.point).ToList();

                for (int i = 0; i < wagons.Count; i++)
                {
                    SplinePoint wagonPoint = new SplinePoint();

                    wagonPoint.SetPosition(wagons[i].transform.position);
                    wagonPoint.size = 1;
                    wagonPoint.normal = Vector3.up;
                    wagonPoint.color = Color.white;
                    points.Insert(0, wagonPoint);

                }
                startPoint.transform.position = transform.position;
                yield return new WaitForSeconds(0.075f);
                _tracer.enabled = false;

                foreach (var item in wagons)
                {
                    item.tracer.enabled = false;
                }

                _tracer.GetComponent<SplineFollower>().applyDirectionRotation = false;
                splinePath.SetPoints(points.ToArray());
                _tracer.spline = splinePath;

                yield return new WaitForSeconds(0.075f);


                _tracer.SetPercent(splineProjector.GetPercent());
                _tracer.enabled = true;
                foreach (var item in wagons)
                {
                    item.tracer.enabled = true;
                }
                path = travelPath.ToList();


                moving = true;
                GetComponent<SplineFollower>().followSpeed = 7;
                SwitchDirection(Spline.Direction.Forward);
                _wagon.UpdateWagon(_wagon, splinePath, Spline.Direction.Forward);

            }

            TrainsManager.Instance.GoBackNormalDirection();
            //CheckAnyNode();
            yield return new WaitForSeconds(1);
        }

        private void RemovePassPoints()
        {
            List<SplinePoint> pointsAndProjector = splinePath.GetPoints().ToList();
            List<SplinePoint> outs = new List<SplinePoint>();

            for (int i = 0; i < pointsAndProjector.Count; i++)
            {
                if (splinePath.GetPointPercent(i) < splineProjector.GetPercent())
                {
                    outs.Add(splinePath.GetPoint(i));
                }
            }
            List<SplinePoint> outOfBounds = pointsAndProjector.Where(x => !outs.Exists(y => y == x)).ToList();
            splinePath.SetPoints(outOfBounds.ToArray());

        }
        private void OnMotionApplied()
        {
            //Apply the wagon's offset (this will recursively apply the offsets to the rest of the wagons in the chain)

            _lastPercent = _tracer.result.percent;
            _wagon.UpdateOffset();
        }

        /// <summary>
        /// Gets the last follower percent before reaching the beginning and looping / ping-ponging
        /// </summary>
        private void FollowerOnBeginningReached(double lastPercent)
        {
            _lastPercent = lastPercent;
        }

        /// <summary>
        /// Gets the last follower percent before reaching the end and looping / ping-ponging
        /// </summary>
        private void FollowerOnEndReached(double lastPercent)
        {
            if (moving)
            {
                ResetPath();
            }
            _lastPercent = lastPercent;
        }
        private void CheckAnyNode()
        {
            for (int i = 0; i < path.Count; i++)
            {
                TravelPoint checkPoint = path[i];
                if (checkPoint.spline.GetNodes().TryGetValue(checkPoint.pointNo, out Node node))
                {
                    isInLastSpline = false;
                    return;
                }
            }
            isInLastSpline = true;
            return;
        }
        //Called when the tracer has passed a junction (a Node)
        private void OnJunction(List<SplineTracer.NodeConnection> passed)
        {
            if (!TrainsManager.Instance.isReverse)
            {
                for (int i = 0; i < path.Count; i++)
                {
                    TravelPoint travelPoint = path[i];
                    if (travelPoint.spline.GetNodes().TryGetValue(travelPoint.pointNo, out Node node) && passed.Exists(x => x.node == node))
                    {
                        if (i + 1 >= path.Count)
                        {
                            path.Remove(travelPoint);

                            CheckAnyNode();
                            return;
                        }
                        TravelPoint nextPoint = path[i + 1];

                        if (travelPoint.spline == nextPoint.spline)
                        {
                            path.Remove(travelPoint);
                            CheckAnyNode();
                            return;
                        }
                        else if (nextPoint.spline.GetNodes().TryGetValue(nextPoint.pointNo, out Node node2))
                        {
                            Spline.Direction dir = Spline.Direction.Forward;
                            if (i + 2 < path.Count)
                            {
                                dir = path[i + 2].pointNo > nextPoint.pointNo ? Spline.Direction.Forward : Spline.Direction.Backward;

                            }
                            else
                            {
                                dir = lastPercentOfPath > nextPoint.percent ? Spline.Direction.Forward : Spline.Direction.Backward;
                            }
                            Node.Connection[] connections = node.GetConnections();

                            SwitchSpline(connections.First(x => x.spline == travelPoint.spline), connections.First(x => x.spline == nextPoint.spline), dir);
                            path.Remove(travelPoint);
                            path.Remove(nextPoint);
                            CheckAnyNode();
                            return;
                        }
                    }
                }
            }

        }
        void SwitchSpline(Node.Connection from, Node.Connection to, Spline.Direction direction)
        {
            //See how much units we have travelled past that Node in the last frame
            float excessDistance = from.spline.CalculateLength(from.spline.GetPointPercent(from.pointIndex), _tracer.UnclipPercent(_lastPercent));
            //Set the spline to the tracer
            _tracer.spline = to.spline;
            _tracer.RebuildImmediate();
            //Get the location of the junction point in percent along the new spline
            double startpercent = _tracer.ClipPercent(to.spline.GetPointPercent(to.pointIndex));
            _tracer.direction = direction;
            //Position the tracer at the new location and travel excessDistance along the new spline
            _tracer.SetPercent(_tracer.Travel(startpercent, excessDistance, _tracer.direction));
            //Notify the wagon that we have entered a new spline segment
            _wagon.EnterSplineSegment(from.pointIndex, _tracer.spline, to.pointIndex, _tracer.direction);
            _wagon.UpdateOffset();
        }
        void SwitchDirection(Spline.Direction direction)
        {
            _tracer.RebuildImmediate();
            _tracer.direction = direction;
            _wagon.UpdateOffset();
        }


    }


}
