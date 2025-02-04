using System.Collections.Generic;
using DG.Tweening;
using Unity.VisualScripting.ReorderableList;
using UnityEngine;
using Dreamteck.Splines;
public class MI : MonoBehaviour
{
    public int TrackLayer = 8;
    public int TrainLayer = 9;
    public int NodeLayer = 10;
    public Camera cam;
    public GameObject train;
    public bool isHeld = false;
    public Material red;
    public Material green;
    public List<Transform> nodes = new();
    public SplineComputer splinecomputer;
    public SplineFollower trainFollower;
    public Train trainengine;
    public float detectionRadius = 1f;

    private void Update()
    {
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Input.GetMouseButtonDown(0))
        {
            trainFollower.follow = false;
            trainengine.isFollow = false;
            splinecomputer.SetPoints(new SplinePoint[0]);
            trainFollower.spline = null;
            if (Physics.Raycast(ray, out hit, Mathf.Infinity, 1 << TrainLayer))
            {
                int layer = hit.collider.gameObject.layer;
                Collider[] colliders = new Collider[3];
                Debug.Log("Raycasst detected Train");

                if (Physics.OverlapSphereNonAlloc(hit.point, detectionRadius, colliders, 1 << NodeLayer) > 0)
                {
                    Collider closestCollider = null;
                    float closestDistance = Mathf.Infinity;

                    for (int i = 0; i < colliders.Length; i++)
                    {
                        if (colliders[i] != null)
                        {
                            float distance = Vector3.Distance(hit.point, colliders[i].transform.position);
                            if (distance < closestDistance)
                            {
                                closestDistance = distance;
                                closestCollider = colliders[i];
                            }
                        }
                    }

                    isHeld = true;
                    if (!nodes.Exists(x => x == closestCollider.transform))
                    {
                        closestCollider.gameObject.GetComponent<MeshRenderer>().material = green;
                        closestCollider.gameObject.GetComponent<PathNode>().isActive = true;
                        nodes.Add(closestCollider.transform);
                        AddTransformToSpline(closestCollider.transform);
                    }


                    Debug.Log("Raycasst detected Node");
                }
            }
        }
        else if (Input.GetMouseButtonUp(0))
        {
            for (int i = 0; i < nodes.Count; i++)
            {
                nodes[i].gameObject.GetComponent<MeshRenderer>().material = red;
            }
            nodes.Clear();
            isHeld = false;

            if (trainFollower != null && splinecomputer != null)
            {
                trainFollower.spline = splinecomputer;
                trainFollower.SetDistance(0);
                trainFollower.follow = true;
                trainengine.isFollow = true;
            }

        }

        RaycastHit hit1;
        if (Input.GetMouseButton(0))
        {
            if (isHeld)
            {
                if (Physics.Raycast(ray, out hit1, Mathf.Infinity, 1 << TrackLayer))
                {
                    int layer = hit1.collider.gameObject.layer;
                    Collider[] colliders = new Collider[3];
                    Debug.Log("Raycasst detected Track");

                    if (Physics.OverlapSphereNonAlloc(hit1.point, 1f, colliders, 1 << NodeLayer) > 0)
                    {
                        Collider closestCollider = null;
                        float closestDistance = Mathf.Infinity;

                        for (int i = 0; i < colliders.Length; i++)
                        {
                            if (colliders[i] != null)
                            {
                                float distance = Vector3.Distance(hit1.point, colliders[i].transform.position);

                                if (distance < closestDistance)
                                {
                                    closestDistance = distance;
                                    closestCollider = colliders[i];

                                }
                            }
                        }

                        if (!closestCollider.GetComponent<PathNode>().isActive)
                        {
                            if (Vector3.Distance(closestCollider.transform.position, nodes[nodes.Count - 1].transform.position) < 1.1f)
                            {
                                if (!nodes.Exists(x => x == closestCollider.transform))
                                {
                                    closestCollider.gameObject.GetComponent<MeshRenderer>().material = green;
                                    closestCollider.gameObject.GetComponent<PathNode>().isActive = true;
                                    nodes.Add(closestCollider.transform);
                                    AddTransformToSpline(closestCollider.transform);
                                }
                            }
                        }
                        else
                        {
                            if (nodes.Count > 1)
                            {
                                if (Vector3.Distance(hit1.point, nodes[nodes.Count - 1].transform.position) > 1f)
                                {
                                    nodes[nodes.Count - 1].gameObject.GetComponent<PathNode>().isActive = false;
                                    nodes[nodes.Count - 1].gameObject.GetComponent<MeshRenderer>().material = red;
                                    RemoveTransformFromSpline(nodes[nodes.Count - 1].transform);
                                    nodes.Remove(nodes[nodes.Count - 1]);
                                }
                            }
                        }
                    }
                }
            }
        }
    }

    public Transform one;
    public Transform two;
    [ContextMenu("Calculate Distance")]
    public void CalculateDistance()
    {
        float distance = Vector3.Distance(one.position, two.position);
        Debug.Log($"Distance: {distance}");
    }

    public void AddTransformToSpline(Transform transform)
    {
        if (splinecomputer == null)
        {
            Debug.LogError("SplineComputer is not assigned.");
            return;
        }

        // Create a new SplinePoint at the transform's position
        SplinePoint newPoint = new SplinePoint(transform.position);

        // Add the new point to the spline
        List<SplinePoint> points = new List<SplinePoint>(splinecomputer.GetPoints());
        points.Add(newPoint);
        splinecomputer.SetPoints(points.ToArray());

        Debug.Log($"Added point at {transform.position} to spline.");
    }

    public void RemoveTransformFromSpline(Transform transform)
    {
        if (splinecomputer == null)
        {
            Debug.LogError("SplineComputer is not assigned.");
            return;
        }

        // Get the current points from the spline
        List<SplinePoint> points = new List<SplinePoint>(splinecomputer.GetPoints());

        // Find the point that matches the transform's position
        SplinePoint pointToRemove = points.Find(p => p.position == transform.position);

        if (pointToRemove != null)
        {
            // Remove the point from the list
            points.Remove(pointToRemove);

            // Update the spline with the new list of points
            splinecomputer.SetPoints(points.ToArray());

            Debug.Log($"Removed point at {transform.position} from spline.");
        }
        else
        {
            Debug.LogWarning($"No point found at {transform.position} to remove.");
        }
    }
}