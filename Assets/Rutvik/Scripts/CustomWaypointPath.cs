using UnityEngine;
using Dreamteck.Splines;
using System.Collections.Generic;

public class CustomWaypointPath : MonoBehaviour
{
    [System.Serializable]
    public class SplineData
    {
        public SplineComputer splineComputer; // Spline computer to manage the spline
        public Transform waypointParent; // Parent object that holds waypoints
    }

    public SplineData[] splineDataArray; // Array to hold multiple spline data

    void Start()
    {
        // Optionally initialize or log the spline data here
    }

    [ButtonInspector]
    public void PathCreation()
    {
        foreach (var splineData in splineDataArray)
        {
            if (splineData.splineComputer == null || splineData.waypointParent == null)
            {
                Debug.LogWarning("SplineComputer or waypoint parent is not set up correctly for one of the spline data.");
                continue;
            }

            // Clear any existing points on the spline
            splineData.splineComputer.SetPoints(new SplinePoint[0]);

            // Get all child transforms and convert to list
            Transform[] waypointsArray = splineData.waypointParent.GetComponentsInChildren<Transform>();
            var waypoints = new List<Transform>(waypointsArray);
            waypoints.RemoveAt(0); // Remove parent

            // Sort waypoints based on position
            List<Transform> sortedWaypoints = new List<Transform>();
            sortedWaypoints.Add(waypoints[0]); // Add first waypoint
            waypoints.RemoveAt(0);

            // Keep finding the nearest point until all points are sorted
            while (waypoints.Count > 0)
            {
                Transform lastPoint = sortedWaypoints[sortedWaypoints.Count - 1];
                Transform nearestPoint = null;
                float nearestDistance = float.MaxValue;

                // Find the nearest point to the last sorted point
                foreach (Transform point in waypoints)
                {
                    float distance = Vector3.Distance(lastPoint.position, point.position);
                    if (distance < nearestDistance)
                    {
                        nearestDistance = distance;
                        nearestPoint = point;
                    }
                }

                sortedWaypoints.Add(nearestPoint);
                waypoints.Remove(nearestPoint);
            }

            // Create spline points from sorted waypoints
            SplinePoint[] splinePoints = new SplinePoint[sortedWaypoints.Count];
            for (int i = 0; i < sortedWaypoints.Count; i++)
            {
                splinePoints[i] = new SplinePoint(sortedWaypoints[i].position);
            }

            // Assign the points to the SplineComputer
            splineData.splineComputer.SetPoints(splinePoints);
        }
    }

    public void ArrangeWaypointsSequentially(Transform waypointParent)
    {
        if (waypointParent == null) return;

        // Get all child transforms and convert to list
        var waypoints = new List<Transform>();
        foreach (Transform child in waypointParent)
        {
            waypoints.Add(child);
        }

        // Sort waypoints based on position
        List<Transform> sortedWaypoints = new List<Transform>();
        sortedWaypoints.Add(waypoints[0]); // Add first waypoint
        waypoints.RemoveAt(0);

        // Keep finding the nearest point until all points are sorted
        while (waypoints.Count > 0)
        {
            Transform lastPoint = sortedWaypoints[sortedWaypoints.Count - 1];
            Transform nearestPoint = null;
            float nearestDistance = float.MaxValue;

            // Find the nearest point to the last sorted point
            foreach (Transform point in waypoints)
            {
                float distance = Vector3.Distance(lastPoint.position, point.position);
                if (distance < nearestDistance)
                {
                    nearestDistance = distance;
                    nearestPoint = point;
                }
            }

            sortedWaypoints.Add(nearestPoint);
            waypoints.Remove(nearestPoint);
        }

        // Reorder the transforms in the hierarchy
        for (int i = 0; i < sortedWaypoints.Count; i++)
        {
            sortedWaypoints[i].SetSiblingIndex(i);
        }
    }

    [ButtonInspector]
    public void ArrangeAllWaypoints()
    {
        foreach (var splineData in splineDataArray)
        {
            ArrangeWaypointsSequentially(splineData.waypointParent);
        }
    }
}
