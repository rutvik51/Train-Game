using System.Collections.Generic;
using System.Linq;
using Dreamteck.Splines;
using Dreamteck.Splines.Examples;
using UnityEngine;

public class PointConroller : MonoBehaviour
{
    public static PointConroller Instance;

    private List<TravelPoint> travelPoints = new List<TravelPoint>();

    public List<PointHolder> points = new List<PointHolder>();

    [HideInInspector] public int currentIndex = 0;
    SplineComputer targetSpline;
    SplineComputer previousSpline;
    private TrainEngine trainEngine;
    public List<SplineComputer> splines;
    public List<SplineProjector> projector;

    public List<GameObject> trackers;
    public GameObject selectionPoint;
    public GameObject rayPoint;
    public LayerMask layerGround;
    public LayerMask layerWagon;
    List<TravelPath> travelPaths = new List<TravelPath>();
    TravelPath bestPath;
    double currentShortestPath;

    private void OnEnable()
    {
        if (Instance == null) Instance = this;
    }

    private void Update()
    {

        if (Input.GetKey(KeyCode.LeftShift))
        {
            Time.timeScale = 10;
        }

        if (Input.GetKeyUp(KeyCode.LeftShift))
        {
            Time.timeScale = 1;
        }
    }
    private void LateUpdate()
    {
        DrawRayAndGetClosestPoint();

        if (Input.GetMouseButtonDown(0))
        {
            if (UIController.instance)
            {
                UIController.instance.gamePlayPanel.TurnOffTutorial();

                if (GameController.instance.fakeLevelIndex == 6)
                {
                    UIController.instance.gamePlayPanel.TurnOffTutorialOfReverseLevel();
                }
            }
            RaycastHit raycastHit;
            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out raycastHit, Mathf.Infinity, layerWagon))
            {
                if (raycastHit.collider.gameObject.CompareTag("Wagon"))
                {
                    if (raycastHit.collider.gameObject.TryGetComponent<TrainEngine>(out TrainEngine te))
                        trainEngine = te;
                    else if (raycastHit.collider.gameObject.TryGetComponent<LastWagon>(out LastWagon lw))
                        trainEngine = lw.engine;

                }
            }
            if (trainEngine == null) return;
            DrawRayAndGetClosestPoint();

            currentIndex = splines.IndexOf(trainEngine.backProjector.spline);
            targetSpline = splines[currentIndex];
            previousSpline = splines[currentIndex];
            trainEngine.ResetPath();
            AddFirst();

        }
        else if (Input.GetMouseButton(0))
        {
            if (trainEngine == null) return;
            UpdatePoints();
            previousSpline = targetSpline;


        }
        else if (Input.GetMouseButtonUp(0))
        {
            if (trainEngine == null) return;
            AfterDraw();
            trainEngine = null;
        }

        // if (points.Count > 0)
        // {
        //     drawline.pointsHolders = points;
        // }
        //previousPercent = projector[currentIndex].GetPercent();

    }
    private void AfterDraw()
    {
        SplinePoint projectorPoint = new SplinePoint();
        SplineComputer lastSpline;

        if (points.Count > 2)
        {
            if (splines[currentIndex] == travelPoints[travelPoints.Count - 1].spline)
            {
                if ((points[points.Count - 1].spline == points[points.Count - 2].spline &&
                     ((points[points.Count - 1].percent > points[points.Count - 2].percent &&
                       projector[currentIndex].GetPercent() > points[points.Count - 1].percent) ||
                      (points[points.Count - 1].percent < points[points.Count - 2].percent &&
                       projector[currentIndex].GetPercent() < points[points.Count - 1].percent))) ||
                     points[points.Count - 1].spline != points[points.Count - 2].spline)
                {
                    projectorPoint = new SplinePoint();
                    projectorPoint.SetPosition(targetSpline.EvaluatePosition(projector[currentIndex].GetPercent()));
                    lastSpline = points[points.Count - 1].spline;
                    projectorPoint.size = 1;
                    projectorPoint.normal = Vector3.up;
                    projectorPoint.color = Color.white;

                    points.Add(new PointHolder(projectorPoint, null, points.Count, points.Count,
                                                projector[currentIndex].GetPercent(), lastSpline));
                }
            }

            if (TrainsManager.Instance.isReverse)
            {
                StartCoroutine(trainEngine.SetPath(points, travelPoints, points[points.Count - 1].percent));
            }
            else if (!TrainsManager.Instance.isReverse)
            {
                StartCoroutine(trainEngine.SetPath(points, travelPoints, points[points.Count - 1].percent));
            }
        }

        points = new List<PointHolder>();
        travelPaths = new List<TravelPath>();
        bestPath = null;
    }

    // private void AfterDraw()
    // {

    //     #region  OLD LOGIC
    //     if (points.Count > 2)
    //     {
    //         if (splines[currentIndex] == travelPoints[travelPoints.Count - 1].spline && (
    //         (projector[currentIndex].GetPercent() < points[points.Count - 1].percent + 0.06 && projector[currentIndex].GetPercent() >= points[points.Count - 1].percent + 0.02) ||
    //         (projector[currentIndex].GetPercent() > points[points.Count - 1].percent - 0.06 && projector[currentIndex].GetPercent() <= points[points.Count - 1].percent - 0.02)))
    //         {
    //             SplinePoint projectorPoint = new SplinePoint();
    //             projectorPoint.SetPosition(targetSpline.EvaluatePosition(projector[currentIndex].GetPercent()));
    //             SplineComputer lastSpline = points[points.Count - 1].spline;
    //             projectorPoint.size = 1;
    //             projectorPoint.normal = Vector3.up;
    //             projectorPoint.color = Color.white;
    //             points.Add(new PointHolder(projectorPoint, null, points.Count, points.Count, projector[currentIndex].GetPercent(), lastSpline));


    //         }
    //         if (TrainsManager.Instance.isReverse)
    //             StartCoroutine(trainEngine.SetPath(points, travelPoints, points[points.Count - 1].percent));
    //         else if (!TrainsManager.Instance.isReverse)
    //             StartCoroutine(trainEngine.SetPath(points, travelPoints, points[points.Count - 1].percent));
    //     }
    //     points = new List<PointHolder>();
    //     travelPaths = new List<TravelPath>();
    //     bestPath = null;
    //     #endregion

    //     #region  NEWLOGIC
    //     // SplinePoint projectorPoint = new SplinePoint();
    //     // SplineComputer lastSpline;
    //     // if (points.Count > 2)
    //     // {

    //     //     if (splines[currentIndex] == travelPoints[travelPoints.Count - 1].spline)
    //     //     {
    //     //         projectorPoint = new SplinePoint();
    //     //         projectorPoint.SetPosition(targetSpline.EvaluatePosition(projector[currentIndex].GetPercent()));
    //     //         lastSpline = points[points.Count - 1].spline;
    //     //         projectorPoint.size = 1;
    //     //         projectorPoint.normal = Vector3.up;
    //     //         projectorPoint.color = Color.white;
    //     //         points.Add(new PointHolder(projectorPoint, null, points.Count, points.Count, projector[currentIndex].GetPercent(), lastSpline));

    //     //     }

    //     //     if (TrainsManager.Instance.isReverse)
    //     //         StartCoroutine(trainEngine.SetPath(points, travelPoints, points[points.Count - 1].percent));
    //     //     else if (!TrainsManager.Instance.isReverse)
    //     //         StartCoroutine(trainEngine.SetPath(points, travelPoints, points[points.Count - 1].percent));
    //     // }

    //     // points = new List<PointHolder>();
    //     // travelPaths = new List<TravelPath>();
    //     // bestPath = null;
    //     #endregion
    // }

    private void DrawRayAndGetClosestPoint()
    {
        RaycastHit raycastHit;
        // Debug.LogError(".." + currentIndex);
        // Debug.LogError(".." + projector[currentIndex].name);
        // Debug.LogError(".." + projector[currentIndex].spline);
        Vector3 closeVec = projector[currentIndex].spline.EvaluatePosition(projector[currentIndex].GetPercent());
        float dis = Vector3.Distance(closeVec, rayPoint.transform.position);

        for (int i = 0; i < trackers.Count; i++)
        {
            if (Vector3.Distance(projector[i].spline.EvaluatePosition(projector[i].GetPercent()), rayPoint.transform.position) < dis)
            {
                currentIndex = i;

                closeVec = projector[currentIndex].spline.EvaluatePosition(projector[currentIndex].GetPercent());

                dis = Vector3.Distance(closeVec, rayPoint.transform.position);
                targetSpline = splines[currentIndex];
            }
        }

        selectionPoint.transform.position = closeVec;
        if (trainEngine == null) return;
        if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out raycastHit))
        {
            rayPoint.transform.position = raycastHit.point;
            trainEngine.mainProjector.projectTarget.transform.position = rayPoint.transform.position;
            foreach (var item in trackers)
            {
                item.transform.position = rayPoint.transform.position;
            }
        }
    }

    private void DrawNewLines()
    {
        if (bestPath == null) return;
        points = new List<PointHolder>();
        SplinePoint newPoint = new SplinePoint();
        newPoint.SetPosition(bestPath.points[0].spline.EvaluatePosition(bestPath.points[0].percent));
        newPoint.size = 1;
        newPoint.normal = Vector3.up;
        newPoint.color = Color.white;
        points.Add(new PointHolder(newPoint, bestPath.points[0], -1, 0, bestPath.points[0].percent, bestPath.points[0].spline));
        for (int i = 1; i < bestPath.points.Count - 1; i++)
        {
            newPoint.SetPosition(bestPath.points[i].spline.EvaluatePosition(bestPath.points[i].percent));
            newPoint.size = 1;
            newPoint.normal = Vector3.up;
            newPoint.color = Color.white;
            points.Add(new PointHolder(newPoint, bestPath.points[i], points.Count, points.Count, bestPath.points[i].percent, bestPath.points[i].spline));

            if (bestPath.points[i].spline == bestPath.points[i + 1].spline)
            {

                if (bestPath.points[i].percent > bestPath.points[i + 1].percent)
                {
                    for (int j = (int)(bestPath.points[i].percent * 50); j >= (int)(bestPath.points[i + 1].percent * 50); j--)
                    {
                        if (j * 0.02 > points[points.Count - 1].percent || j * 0.02 < bestPath.points[i + 1].percent) continue;
                        newPoint = new SplinePoint();
                        newPoint.SetPosition(bestPath.points[i].spline.EvaluatePosition(j * 0.02));
                        newPoint.size = 1;
                        newPoint.normal = Vector3.up;
                        newPoint.color = Color.white;
                        points.Add(new PointHolder(newPoint, null, points.Count, points.Count, j * 0.02, bestPath.points[i].spline));

                    }


                }
                else
                {
                    for (int j = (int)(bestPath.points[i].percent * 50); j < (int)(bestPath.points[i + 1].percent * 50); j++)
                    {
                        if (j * 0.02 < points[points.Count - 1].percent || j * 0.02 > bestPath.points[i + 1].percent) continue;
                        newPoint = new SplinePoint();
                        newPoint.SetPosition(bestPath.points[i].spline.EvaluatePosition(j * 0.02));
                        newPoint.size = 1;
                        newPoint.normal = Vector3.up;
                        newPoint.color = Color.white;
                        points.Add(new PointHolder(newPoint, null, points.Count, points.Count, j * 0.02, bestPath.points[i].spline));

                    }
                }
            }
        }
        if (bestPath.points.Count > 2)
        {
            int index = points.Count;
            newPoint = new SplinePoint();
            newPoint.SetPosition(bestPath.points[bestPath.points.Count - 1].spline.EvaluatePosition(bestPath.points[bestPath.points.Count - 1].percent));
            newPoint.size = 1;
            newPoint.normal = Vector3.up;
            newPoint.color = Color.white;
            points.Add(new PointHolder(newPoint, bestPath.points[bestPath.points.Count - 1], index, points.Count, bestPath.points[bestPath.points.Count - 1].percent, bestPath.points[bestPath.points.Count - 1].spline));


        }
        else
        {
            int index = points.Count;
            newPoint = new SplinePoint();
            newPoint.SetPosition(bestPath.points[1].spline.EvaluatePosition(bestPath.points[1].percent));
            newPoint.size = 1;
            newPoint.normal = Vector3.up;
            newPoint.color = Color.white;
            points.Add(new PointHolder(newPoint, bestPath.points[1], index, points.Count, bestPath.points[1].percent, bestPath.points[1].spline));


        }
        if (bestPath.points[bestPath.points.Count - 1].percent < projector[currentIndex].GetPercent() && bestPath.points[bestPath.points.Count - 1].spline && splines[currentIndex])
        {
            for (int j = (int)(bestPath.points[bestPath.points.Count - 1].percent * 50); j < (int)(projector[currentIndex].GetPercent() * 50); j++)
            {
                if (j * 0.02 < points[points.Count - 1].percent || j * 0.02 > projector[currentIndex].GetPercent()) continue;
                newPoint = new SplinePoint();
                newPoint.SetPosition(bestPath.points[bestPath.points.Count - 1].spline.EvaluatePosition(j * 0.02));
                newPoint.size = 1;
                newPoint.normal = Vector3.up;
                newPoint.color = Color.white;
                points.Add(new PointHolder(newPoint, null, points.Count, points.Count, j * 0.02, splines[currentIndex]));
            }
        }
        else if (bestPath.points[bestPath.points.Count - 1].spline && splines[currentIndex])
        {
            for (int j = (int)(bestPath.points[bestPath.points.Count - 1].percent * 50) - 1; j >= (int)(projector[currentIndex].GetPercent() * 50); j--)
            {
                if (j * 0.02 > points[points.Count - 1].percent || j * 0.02 < projector[currentIndex].GetPercent()) continue;
                newPoint = new SplinePoint();
                newPoint.SetPosition(bestPath.points[bestPath.points.Count - 1].spline.EvaluatePosition(j * 0.02));
                newPoint.size = 1;
                newPoint.normal = Vector3.up;
                newPoint.color = Color.white;
                points.Add(new PointHolder(newPoint, null, points.Count, points.Count, j * 0.02, splines[currentIndex]));

            }

        }
        points[0].pointNo = -1;
        points[1].pointNo = -2;

    }

    private void FindingBest(List<TravelPoint> path, double currentDist, double pointPercent, double tracerPercent, int index, SplineComputer startSearchingSpline, SplineComputer targetPathSpline, double targetPointPathPercent, TravelPoint lastPointBeforeStart)
    {
        if (currentDist > currentShortestPath) return;
        if (pointPercent == tracerPercent)
        {
            if (pointPercent < 1)
            {
                List<TravelPoint> branchPath = CreateNewBranch(path);



                if (branchPath[branchPath.Count - 1].node.isNode)
                {
                    if (startSearchingSpline.GetNodes().TryGetValue(index, out Node node) && node.GetComponent<NodeCheck>().hasOnlyOneWay && node.GetComponent<NodeCheck>().isLoopConnection)
                        branchPath[branchPath.Count - 1].node = new SplineNode(true, true, true);
                    else
                        branchPath[branchPath.Count - 1].node = new SplineNode(true, true, false);
                }
                FindingBest(branchPath, currentDist, pointPercent, tracerPercent + 0.00001, index + 1, startSearchingSpline, targetPathSpline, targetPointPathPercent, lastPointBeforeStart);



            }
            if (pointPercent > 0)
            {
                List<TravelPoint> branchPath = CreateNewBranch(path);
                if (branchPath[branchPath.Count - 1].node.isNode)
                {
                    if (startSearchingSpline.GetNodes().TryGetValue(index, out Node node) && node.GetComponent<NodeCheck>().hasOnlyOneWay && node.GetComponent<NodeCheck>().isLoopConnection)
                        branchPath[branchPath.Count - 1].node = new SplineNode(true, true, true);
                    else
                        branchPath[branchPath.Count - 1].node = new SplineNode(true, false, true);
                }
                FindingBest(branchPath, currentDist, pointPercent, tracerPercent - 0.00001, index - 1, startSearchingSpline, targetPathSpline, targetPointPathPercent, lastPointBeforeStart);

            }

            return;
        }
        Spline.Direction direction = Spline.Direction.Forward;
        if (tracerPercent > pointPercent) { direction = Spline.Direction.Forward; }
        else if (tracerPercent < pointPercent) { direction = Spline.Direction.Backward; }
        if (pointPercent != tracerPercent && direction == Spline.Direction.Forward)
        {

            for (int i = index; i < startSearchingSpline.pointCount; i++)
            {
                if (startSearchingSpline.GetPointPercent(i) > targetPointPathPercent && targetPathSpline == startSearchingSpline && path[path.Count - 1].percent < targetPointPathPercent && path[path.Count - 1].spline == targetPathSpline)
                {

                    List<TravelPoint> branchPath = CreateNewBranch(path);
                    currentDist += startSearchingSpline.CalculateLength(pointPercent, targetPointPathPercent);
                    if (currentDist > currentShortestPath) return;
                    currentShortestPath = currentDist;
                    travelPaths.Add(new TravelPath(branchPath, currentDist, lastPointBeforeStart));
                    return;
                }
                else if ((path[0].spline == path[path.Count - 1].spline && path[0].spline == startSearchingSpline) && ((path[path.Count - 1].percent <= path[0].percent && path[0].percent <= startSearchingSpline.GetPointPercent(i)))) return;
                else if (path.Exists(x => startSearchingSpline.GetNodes().TryGetValue(i, out Node node) && !node.GetComponent<NodeCheck>().hasOnlyOneWay && x.spline == startSearchingSpline && x.point == startSearchingSpline.GetPoint(i) && !node.GetComponent<NodeCheck>().isLoopConnection && x.node.hasBackward) &&
                    path.FindIndex(x => startSearchingSpline.GetNodes().TryGetValue(i, out Node node) && x.spline == startSearchingSpline && !node.GetComponent<NodeCheck>().hasOnlyOneWay && x.point == startSearchingSpline.GetPoint(i) && !node.GetComponent<NodeCheck>().isLoopConnection && x.node.hasBackward) != path.Count - 1) return;
                else if (path.Exists(x => (!startSearchingSpline.GetNodes().TryGetValue(i, out Node node)) && x.point == startSearchingSpline.GetPoint(i))) return;
                else if (path.Exists(x => startSearchingSpline.GetNodes().TryGetValue(i, out Node node) && node != null && node.GetComponent<NodeCheck>().hasOnlyOneWay && x.point == startSearchingSpline.GetPoint(i))) return;
                //else if (path[path.Count - 1].point == startSearchingSpline.GetPoint(i)) return;
                if (startSearchingSpline.GetPointPercent(i) > pointPercent)
                {
                    startSearchingSpline.GetNodes().TryGetValue(i, out Node node);
                    currentDist += startSearchingSpline.CalculateLength(pointPercent, startSearchingSpline.GetPointPercent(i));
                    if (currentDist > currentShortestPath) return;
                    if (node != null && (path[path.Count - 1].point == startSearchingSpline.GetPoint(i) || path[path.Count - 2].point == startSearchingSpline.GetPoint(i))) return;

                    pointPercent = startSearchingSpline.GetPointPercent(i);
                    SplineNode connectedNode = node != null ? new SplineNode(true, false, true) : new SplineNode();
                    path.Add(new TravelPoint(startSearchingSpline.GetPoint(i), i, startSearchingSpline.GetPointPercent(i), startSearchingSpline, connectedNode));

                    if (node != null)
                    {
                        if (node.GetComponent<NodeCheck>().hasOnlyOneWay)
                            connectedNode = new SplineNode(true);
                        List<TravelPoint> branchPath = CreateNewBranch(path);
                        Node.Connection targetCon = node.GetConnections().First(x => x.spline != startSearchingSpline);
                        double otherTrackPointPercent = targetCon.spline.GetPointPercent(targetCon.pointIndex);
                        SplineNode targetNode = new SplineNode(true, node.GetComponent<NodeCheck>().hasOnlyOneWay, node.GetComponent<NodeCheck>().hasOnlyOneWay);
                        branchPath.Add(new TravelPoint(targetCon.spline.GetPoint(targetCon.pointIndex), targetCon.pointIndex, otherTrackPointPercent, targetCon.spline, targetNode));
                        FindingBest(branchPath, currentDist, otherTrackPointPercent, otherTrackPointPercent, targetCon.pointIndex, targetCon.spline, targetPathSpline, targetPointPathPercent, lastPointBeforeStart);

                        if (path.Exists(x => startSearchingSpline.GetNodes().TryGetValue(i, out Node node) && !node.GetComponent<NodeCheck>().hasOnlyOneWay && !node.GetComponent<NodeCheck>().isLoopConnection && x.point == startSearchingSpline.GetPoint(i) && x.node.hasForward)) return;

                    }
                }
            }
        }
        else if (pointPercent != tracerPercent && direction == Spline.Direction.Backward)
        {
            for (int i = index; i >= 0; i--)
            {
                if (startSearchingSpline.GetPointPercent(i) < targetPointPathPercent && targetPathSpline == startSearchingSpline && path[path.Count - 1].percent > targetPointPathPercent && path[path.Count - 1].spline == targetPathSpline)
                {
                    List<TravelPoint> branchPath = CreateNewBranch(path);
                    currentDist += startSearchingSpline.CalculateLength(targetPointPathPercent, pointPercent);
                    if (currentDist > currentShortestPath) return;
                    currentShortestPath = currentDist;
                    travelPaths.Add(new TravelPath(branchPath, currentDist, lastPointBeforeStart));
                    return;
                }
                else if ((path[0].spline == path[path.Count - 1].spline && path[0].spline == startSearchingSpline) && ((path[path.Count - 1].percent >= path[0].percent && path[0].percent >= startSearchingSpline.GetPointPercent(i)))) return;
                else if (path.Exists(x => startSearchingSpline.GetNodes().TryGetValue(i, out Node node) && x.spline == startSearchingSpline && !node.GetComponent<NodeCheck>().hasOnlyOneWay && !node.GetComponent<NodeCheck>().isLoopConnection && x.point == startSearchingSpline.GetPoint(i) && x.node.hasForward) &&
                    path.FindIndex(x => startSearchingSpline.GetNodes().TryGetValue(i, out Node node) && x.spline == startSearchingSpline && !node.GetComponent<NodeCheck>().hasOnlyOneWay && x.point == startSearchingSpline.GetPoint(i) && !node.GetComponent<NodeCheck>().isLoopConnection && x.node.hasForward) != path.Count - 1) return;
                else if (path.Exists(x => (!startSearchingSpline.GetNodes().TryGetValue(i, out Node node) && x.point == startSearchingSpline.GetPoint(i)))) return;
                else if (path.Exists(x => (startSearchingSpline.GetNodes().TryGetValue(i, out Node node)) && node.GetComponent<NodeCheck>().hasOnlyOneWay && x.point == startSearchingSpline.GetPoint(i))) return;
                //else if (path[path.Count - 1].point == startSearchingSpline.GetPoint(i)) return;
                if (startSearchingSpline.GetPointPercent(i) < pointPercent)
                {
                    startSearchingSpline.GetNodes().TryGetValue(i, out Node node);
                    currentDist += startSearchingSpline.CalculateLength(startSearchingSpline.GetPointPercent(i), pointPercent);
                    if (currentDist > currentShortestPath) return;
                    if (node != null && (path[path.Count - 1].point == startSearchingSpline.GetPoint(i) || path[path.Count - 2].point == startSearchingSpline.GetPoint(i))) return;

                    pointPercent = startSearchingSpline.GetPointPercent(i);
                    SplineNode connectedNode = node != null ? new SplineNode(true, true, false) : new SplineNode();
                    path.Add(new TravelPoint(startSearchingSpline.GetPoint(i), i, startSearchingSpline.GetPointPercent(i), startSearchingSpline, connectedNode));

                    if (node != null)
                    {
                        if (node.GetComponent<NodeCheck>().hasOnlyOneWay)
                            connectedNode = new SplineNode(true);
                        List<TravelPoint> branchPath = CreateNewBranch(path);
                        Node.Connection targetCon = node.GetConnections().First(x => x.spline != startSearchingSpline);
                        double otherTrackPointPercent = targetCon.spline.GetPointPercent(targetCon.pointIndex);
                        SplineNode targetNode = new SplineNode(true, node.GetComponent<NodeCheck>().hasOnlyOneWay, node.GetComponent<NodeCheck>().hasOnlyOneWay);
                        branchPath.Add(new TravelPoint(targetCon.spline.GetPoint(targetCon.pointIndex), targetCon.pointIndex, otherTrackPointPercent, targetCon.spline, targetNode));
                        FindingBest(branchPath, currentDist, otherTrackPointPercent, otherTrackPointPercent, targetCon.pointIndex, targetCon.spline, targetPathSpline, targetPointPathPercent, lastPointBeforeStart);

                        if (path.Exists(x => startSearchingSpline.GetNodes().TryGetValue(i, out Node node) && !node.GetComponent<NodeCheck>().hasOnlyOneWay && !node.GetComponent<NodeCheck>().isLoopConnection && x.point == startSearchingSpline.GetPoint(i) && x.node.hasBackward)) return;

                    }

                }
            }
        }
    }

    private void CallMethodForLongDrawInSpline()
    {
        currentShortestPath = Mathf.Infinity;

        List<TravelPoint> searchPathStart = CreateNewBranch(travelPoints);
        List<TravelPoint> otherNodeSearchList = CreateNewBranch(searchPathStart);

        travelPaths = new List<TravelPath>();
        bestPath = null;
        int sendIndex = searchPathStart[searchPathStart.Count - 1].pointNo;
        double tracerPercent = searchPathStart[searchPathStart.Count - 1].percent;
        if (sendIndex < 0)
        {
            if (searchPathStart[0].percent < searchPathStart[1].percent)
            {
                sendIndex = previousSpline.PercentToPointIndex(searchPathStart[searchPathStart.Count - 1].percent);
                tracerPercent += 0.001;
            }
            else if (searchPathStart[0].percent > searchPathStart[1].percent)
            {
                sendIndex = previousSpline.PercentToPointIndex(searchPathStart[searchPathStart.Count - 1].percent) + 1;
                tracerPercent -= 0.001;

            }
        }

        if (otherNodeSearchList[otherNodeSearchList.Count - 1].spline.GetNodes().TryGetValue(otherNodeSearchList[otherNodeSearchList.Count - 1].pointNo, out Node node1))
        {
            if (otherNodeSearchList[otherNodeSearchList.Count - 1].spline != otherNodeSearchList[otherNodeSearchList.Count - 2].spline && otherNodeSearchList[otherNodeSearchList.Count - 2].node.isNode &&
                otherNodeSearchList[otherNodeSearchList.Count - 2].spline.GetNodes().TryGetValue(otherNodeSearchList[otherNodeSearchList.Count - 2].pointNo, out Node node2))
            {
                if (!node2.GetComponent<NodeCheck>().isLoopConnection)
                    otherNodeSearchList.Remove(otherNodeSearchList[otherNodeSearchList.Count - 1]);
            }
            else
            {
                Node.Connection targetCon = node1.GetConnections().First(x => x.spline != otherNodeSearchList[otherNodeSearchList.Count - 1].spline);
                double otherTrackPointPercent = targetCon.spline.GetPointPercent(targetCon.pointIndex);
                otherNodeSearchList.Add(new TravelPoint(targetCon.spline.GetPoint(targetCon.pointIndex), targetCon.pointIndex, otherTrackPointPercent, targetCon.spline, new SplineNode(true, node1.GetComponent<NodeCheck>().hasOnlyOneWay, node1.GetComponent<NodeCheck>().hasOnlyOneWay)));

            }
            FindingBest(otherNodeSearchList, 0,
                otherNodeSearchList[otherNodeSearchList.Count - 1].percent,
                otherNodeSearchList[otherNodeSearchList.Count - 1].percent,
                otherNodeSearchList[otherNodeSearchList.Count - 1].pointNo,
                otherNodeSearchList[otherNodeSearchList.Count - 1].spline, splines[currentIndex],
                projector[currentIndex].GetPercent(), otherNodeSearchList[otherNodeSearchList.Count - 1]);
        }
        FindingBest(searchPathStart, 0,
             searchPathStart[searchPathStart.Count - 1].percent,
             tracerPercent,
             sendIndex,
             searchPathStart[searchPathStart.Count - 1].spline, splines[currentIndex],
             projector[currentIndex].GetPercent(), searchPathStart[searchPathStart.Count - 1]);

        double dist = double.PositiveInfinity;
        for (int i = 0; i < travelPaths.Count; i++)
        {
            if (travelPaths[i].distance < dist)
            {
                bestPath = travelPaths[i];
                dist = bestPath.distance;
            }
        }
        if (bestPath != null)
        {
            DrawNewLines();
            travelPoints = bestPath.points;
        }
    }

    private void UpdateDrawInSpline()
    {
        SplinePoint newPoint = new SplinePoint();
        newPoint.SetPosition(targetSpline.EvaluatePosition(projector[currentIndex].GetPercent()));
        newPoint.size = 1;
        newPoint.normal = Vector3.up;
        newPoint.color = Color.white;
        double projectorPercent = projector[currentIndex].GetPercent();
        double projectorPassedPointPercentForward = targetSpline.GetPointPercent(targetSpline.PercentToPointIndex(projector[currentIndex].GetPercent()));
        int projectorPassedPointIndexForward = targetSpline.PercentToPointIndex(projector[currentIndex].GetPercent());
        SplinePoint projectorPassedPointForward = targetSpline.GetPoint(targetSpline.PercentToPointIndex(projector[currentIndex].GetPercent()));

        double projectorPassedPointPercentBackward = targetSpline.GetPointPercent(targetSpline.PercentToPointIndex(projector[currentIndex].GetPercent()) + 1);
        int projectorPassedPointIndexBackward = targetSpline.PercentToPointIndex(projector[currentIndex].GetPercent()) + 1;
        SplinePoint projectorPassedPointBackward = targetSpline.GetPoint(targetSpline.PercentToPointIndex(projector[currentIndex].GetPercent()) + 1);

        if (projectorPassedPointPercentForward <= projectorPercent &&
            projectorPassedPointPercentForward >= points[points.Count - 1].percent &&
            travelPoints[travelPoints.Count - 1].point != projectorPassedPointForward)
        {
            targetSpline.GetNodes().TryGetValue(projectorPassedPointIndexForward, out Node node);
            SplineNode connectedNode = node != null ? new SplineNode(true, false, true) : new SplineNode();
            TravelPoint travelPoint = new TravelPoint(projectorPassedPointForward, projectorPassedPointIndexForward, projectorPassedPointPercentForward, targetSpline, connectedNode);
            travelPoints.Add(travelPoint);
            points.Add(new PointHolder(newPoint, travelPoint, points.Count, points.Count, projectorPassedPointPercentForward, targetSpline));

        }
        else if (projectorPassedPointPercentBackward >= projectorPercent &&
            projectorPassedPointPercentBackward <= points[points.Count - 1].percent &&
            travelPoints[travelPoints.Count - 1].point != projectorPassedPointBackward)
        {
            targetSpline.GetNodes().TryGetValue(projectorPassedPointIndexBackward, out Node node);
            SplineNode connectedNode = node != null ? new SplineNode(true, true, false) : new SplineNode();
            TravelPoint travelPoint = new TravelPoint(projectorPassedPointBackward, projectorPassedPointIndexBackward, projectorPassedPointPercentBackward, targetSpline, connectedNode); ;
            travelPoints.Add(travelPoint);
            points.Add(new PointHolder(newPoint, travelPoint, points.Count, points.Count, projectorPassedPointPercentBackward, targetSpline));

        }
        else
        {
            points.Add(new PointHolder(newPoint, null, points.Count, points.Count, projector[currentIndex].GetPercent(), targetSpline));

        }
    }

    private void RemovePassPoints()
    {
        List<PointHolder> outOfBounds = points.Where(x => x.pointNo >= 0 && trainEngine.linePath.GetPointPercent(x.realNo) > trainEngine.mainProjector.GetPercent()).ToList();
        foreach (var item in outOfBounds)
        {
            if (item.boundedPoint != null)
            {
                travelPoints.Remove(item.boundedPoint);
            }
            points.Remove(item);
        }
    }

    private bool IsValidSplineTransition(SplineComputer fromSpline, SplineComputer toSpline, double fromPercent, double toPercent)
    {
        // If it's the same spline, it's valid
        if (fromSpline == toSpline) return true;

        // Get the closest nodes for both percentages
        int fromNodeIndex = fromSpline.PercentToPointIndex(fromPercent);
        int toNodeIndex = toSpline.PercentToPointIndex(toPercent);

        // Check if we have nodes at these positions
        Node fromNode = null;
        Node toNode = null;
        fromSpline.GetNodes().TryGetValue(fromNodeIndex, out fromNode);
        toSpline.GetNodes().TryGetValue(toNodeIndex, out toNode);

        // Both points must be at nodes to allow transition
        if (fromNode == null || toNode == null) return false;

        // Check if these nodes are directly connected
        foreach (var connection in fromNode.GetConnections())
        {
            if (connection.spline == toSpline && connection.pointIndex == toNodeIndex)
            {
                return true;
            }
        }

        return false;
    }

    private bool IsValidPointAddition(SplineComputer currentSpline, double currentPercent)
    {
        if (points.Count == 0) return true;

        PointHolder lastPoint = points[points.Count - 1];

        // If on the same spline, allow the point
        if (currentSpline == lastPoint.spline)
        {
            return true;
        }

        // If trying to switch splines, only allow it if both points are very close to a node connection
        foreach (var nodeKvp in lastPoint.spline.GetNodes())
        {
            Node lastNode = nodeKvp.Value;
            double lastNodePercent = lastPoint.spline.GetPointPercent(nodeKvp.Key);

            // Check if last point is close to this node
            if (Mathf.Abs((float)(lastPoint.percent - lastNodePercent)) < 0.1f)
            {
                // Check if this node connects to the new spline
                foreach (var connection in lastNode.GetConnections())
                {
                    if (connection.spline == currentSpline)
                    {
                        double connectionNodePercent = currentSpline.GetPointPercent(connection.pointIndex);
                        // Check if current point is close to the connected node
                        if (Mathf.Abs((float)(currentPercent - connectionNodePercent)) < 0.1f)
                        {
                            return true;
                        }
                    }
                }
            }
        }

        return false;
    }

    private void UpdatePoints()
    {
        RemovePassPoints();

        // Always try to add points, but validate them
        if (points.Count == 0 || IsValidPointAddition(splines[currentIndex], projector[currentIndex].GetPercent()))
        {
            AddNewPoint();
        }

        UpdatePathVisuals();
    }

    private bool CanAddNewPoint()
    {
        if (points.Count == 0) return true;

        PointHolder lastPoint = points[points.Count - 1];
        double minPointDistance = 0.02; // Minimum distance between points

        // Only allow points to be added along the same spline or at valid node connections
        if (splines[currentIndex] == lastPoint.spline)
        {
            float distanceBetweenPoints = Mathf.Abs((float)(projector[currentIndex].GetPercent() - lastPoint.percent));
            return distanceBetweenPoints >= minPointDistance;
        }

        // For different splines, rely on IsValidPointAddition check
        return false;
    }

    private void AddNewPoint()
    {
        SplinePoint newPoint = new SplinePoint();
        newPoint.SetPosition(targetSpline.EvaluatePosition(projector[currentIndex].GetPercent()));
        newPoint.size = 1;
        newPoint.normal = Vector3.up;
        newPoint.color = Color.white;

        // Create travel point if we're at a node
        TravelPoint travelPoint = null;
        int pointIndex = targetSpline.PercentToPointIndex(projector[currentIndex].GetPercent());

        if (targetSpline.GetNodes().TryGetValue(pointIndex, out Node node))
        {
            SplineNode splineNode = new SplineNode(true, true, true);
            travelPoint = new TravelPoint(newPoint, pointIndex, projector[currentIndex].GetPercent(), targetSpline, splineNode);
            travelPoints.Add(travelPoint);
        }

        points.Add(new PointHolder(newPoint, travelPoint, points.Count, points.Count,
            projector[currentIndex].GetPercent(), targetSpline));
    }

    private void UpdatePathVisuals()
    {
        if (trainEngine == null) return;

        List<SplinePoint> pathPoints = points.Select(x => x.point).ToList();
        trainEngine.linePath.SetPoints(pathPoints.ToArray());
        trainEngine.lineSmoothPath.SetPoints(pathPoints.ToArray());
    }

    private void AddFirst()
    {
        points = new List<PointHolder>();
        travelPoints = new List<TravelPoint>();
        SplinePoint newPoint = new SplinePoint();
        double addingFirst = trainEngine.backProjector.GetPercent() < trainEngine.frontProjector.GetPercent() ? -0.015 : 0.015;
        double addingSecond = 0;

        if (trainEngine.backProjector.GetPercent() + addingFirst <= 0)
        {
            addingFirst = -trainEngine.backProjector.GetPercent() + 0.00002;
        }
        else if (trainEngine.backProjector.GetPercent() + addingFirst >= 1)
        {
            addingFirst = 1 - trainEngine.backProjector.GetPercent() - 0.00002;

        }
        newPoint.SetPosition(trainEngine.backProjector.spline.EvaluatePosition(trainEngine.backProjector.GetPercent() + addingFirst));

        newPoint.size = 1;
        newPoint.normal = Vector3.up;
        newPoint.color = Color.white;

        SplinePoint secondPoint = new SplinePoint();
        if ((trainEngine.backProjector.GetPercent() > trainEngine.frontProjector.GetPercent() && trainEngine.frontProjector.GetPercent() == 0) || (trainEngine.frontProjector.GetPercent() == 0 && trainEngine.frontProjector.GetPercent() == 0))
        {
            addingSecond += 0.00001;
        }
        if ((trainEngine.backProjector.GetPercent() < trainEngine.frontProjector.GetPercent() && trainEngine.backProjector.GetPercent() == 1) || (trainEngine.frontProjector.GetPercent() == 1 && trainEngine.frontProjector.GetPercent() == 1))
        {
            addingSecond -= 0.00001;
        }
        secondPoint.SetPosition(trainEngine.backProjector.spline.EvaluatePosition(trainEngine.backProjector.GetPercent() + addingSecond));
        secondPoint.size = 1;
        secondPoint.normal = Vector3.up;
        secondPoint.color = Color.white;
        if (trainEngine.backProjector.GetPercent() < trainEngine.frontProjector.GetPercent())
        {
            if (TrainsManager.Instance.isReverse)
            {
                TravelPoint secondTravel = new TravelPoint(secondPoint, -1, trainEngine.backProjector.GetPercent() + addingSecond, trainEngine.backProjector.spline, new SplineNode());
                points.Add(new PointHolder(secondPoint, secondTravel, -1, 0, trainEngine.backProjector.GetPercent() + addingSecond, trainEngine.backProjector.spline));
                travelPoints.Add(secondTravel);

                TravelPoint firstTravel = new TravelPoint(newPoint, -2, trainEngine.backProjector.GetPercent() + addingFirst, trainEngine.backProjector.spline, new SplineNode());
                points.Add(new PointHolder(newPoint, firstTravel, -2, 1, trainEngine.backProjector.GetPercent() + addingFirst, trainEngine.backProjector.spline));
                travelPoints.Add(firstTravel);


            }
            else
            {
                TravelPoint firstTravel = new TravelPoint(newPoint, -1, trainEngine.backProjector.GetPercent() + addingFirst, trainEngine.backProjector.spline, new SplineNode());
                points.Add(new PointHolder(newPoint, firstTravel, -1, 0, trainEngine.backProjector.GetPercent() + addingFirst, trainEngine.backProjector.spline));
                travelPoints.Add(firstTravel);

                TravelPoint secondTravel = new TravelPoint(secondPoint, -2, trainEngine.backProjector.GetPercent() + addingSecond, trainEngine.backProjector.spline, new SplineNode());
                points.Add(new PointHolder(secondPoint, secondTravel, -2, 1, trainEngine.backProjector.GetPercent() + addingSecond, trainEngine.backProjector.spline));
                travelPoints.Add(secondTravel);
            }

        }
        else
        {
            if (TrainsManager.Instance.isReverse)
            {
                TravelPoint secondTravel = new TravelPoint(secondPoint, -1, trainEngine.backProjector.GetPercent() + addingSecond, trainEngine.backProjector.spline, new SplineNode());
                points.Add(new PointHolder(secondPoint, secondTravel, -1, 0, trainEngine.backProjector.GetPercent() + addingSecond, trainEngine.backProjector.spline));
                travelPoints.Add(secondTravel);


                TravelPoint firstTravel = new TravelPoint(newPoint, -2, trainEngine.backProjector.GetPercent() + addingFirst, trainEngine.backProjector.spline, new SplineNode());
                points.Add(new PointHolder(newPoint, firstTravel, -2, 1, trainEngine.backProjector.GetPercent() + addingFirst, trainEngine.backProjector.spline));
                travelPoints.Add(firstTravel);


            }
            else
            {
                TravelPoint firstTravel = new TravelPoint(newPoint, -1, trainEngine.backProjector.GetPercent() + addingFirst, trainEngine.backProjector.spline, new SplineNode());
                points.Add(new PointHolder(newPoint, firstTravel, -1, 0, trainEngine.backProjector.GetPercent() + addingFirst, trainEngine.backProjector.spline));
                travelPoints.Add(firstTravel);

                TravelPoint secondTravel = new TravelPoint(secondPoint, -2, trainEngine.backProjector.GetPercent() + addingSecond, trainEngine.backProjector.spline, new SplineNode());
                points.Add(new PointHolder(secondPoint, secondTravel, -2, 1, trainEngine.backProjector.GetPercent() + addingSecond, trainEngine.backProjector.spline));
                travelPoints.Add(secondTravel);
            }

        }
        trainEngine.linePath.SetPoints(points.Select(x => x.point).ToArray());
        trainEngine.lineSmoothPath.SetPoints(points.Select(x => x.point).ToArray());
    }
    private List<TravelPoint> CreateNewBranch(List<TravelPoint> copy)
    {
        List<TravelPoint> paste = new List<TravelPoint>();
        for (int i = 0; i < copy.Count; i++)
            paste.Add(new TravelPoint(copy[i].point, copy[i].pointNo, copy[i].percent, copy[i].spline, new SplineNode(copy[i].node.isNode, copy[i].node.hasForward, copy[i].node.hasBackward)));
        return paste;
    }

}
[System.Serializable]
public class TravelPath
{
    public List<TravelPoint> points = new List<TravelPoint>();
    public double distance;
    public TravelPoint lastPointBeforeSearch;
    public TravelPath(List<TravelPoint> _points, double _distance, TravelPoint _lastPointBeforeSearch)
    {
        lastPointBeforeSearch = _lastPointBeforeSearch;
        foreach (var item in _points)
            points.Add(item);

        distance = _distance;
    }
}
[System.Serializable]
public class TravelPoint
{
    public SplinePoint point;
    public int pointNo;
    public double percent;
    public SplineComputer spline;
    public SplineNode node;
    public TravelPoint(SplinePoint _point, int _pointNo, double _percent, SplineComputer _spline, SplineNode _node)
    {
        pointNo = _pointNo;
        point = _point;
        percent = _percent;
        spline = _spline;
        node = _node;
    }
}
[System.Serializable]
public class PointHolder
{
    public SplinePoint point;
    public TravelPoint boundedPoint;
    public SplineComputer spline;
    public int pointNo;
    public int realNo;
    public double percent;

    public PointHolder(SplinePoint _point, TravelPoint _boundedPoint, int _pointNo, int _realNo, double _percent, SplineComputer _spline)
    {
        boundedPoint = _boundedPoint;
        pointNo = _pointNo;
        realNo = _realNo;
        point = _point;
        percent = _percent;
        spline = _spline;
    }
}
[System.Serializable]
public class SplineNode
{
    public bool isNode;
    public bool hasForward;
    public bool hasBackward;
    public SplineNode(bool _isNode = false, bool _hasForward = true, bool _hasBackward = true)
    {
        isNode = _isNode;
        hasForward = _hasForward;
        hasBackward = _hasBackward;
    }
}