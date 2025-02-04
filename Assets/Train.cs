using System.Collections;
using System.Collections.Generic;
using Dreamteck.Splines;
using UnityEngine;

public class Train : MonoBehaviour
{
    public GameObject[] coaches;

    public SplineFollower splineFollower;

    public float engineToCoachOffset = 10.0f; // Offset from engine to other coach
    public float coachToCoachOffset = 5.0f;   // Offset between each coach

    private SplineComputer currentSpline;

    public bool isFollow;
    void Start()
    {

    }

    void Update()
    {
        // if (!isFollow) return;
        // Check if the splineFollower or its spline is null
        if (splineFollower == null || splineFollower.spline == null)
        {
            Debug.LogError("SplineFollower or its components are not initialized.");
            return;
        }

        // Check if the spline has changed and update the splineFollower
        if (splineFollower.spline != currentSpline)
        {
            currentSpline = splineFollower.spline;
            Debug.Log("Spline has been updated.");
        }

        // Get the current position on the spline for the train
        float trainPosition = (float)splineFollower.result.percent;

        // Ensure the train starts from the beginning of the spline
        if (trainPosition == 1.0f)
        {
            trainPosition = 0.0f; // Start from the beginning of the spline
        }

        float splineLength = splineFollower.spline.CalculateLength();

        for (int i = 0; i < coaches.Length; i++)
        {
            // Calculate the offset percentage for each coach
            float offsetDistance = engineToCoachOffset + i * coachToCoachOffset;
            float offsetPercent = offsetDistance / splineLength;

            // Calculate the position on the spline for each coach
            float coachPosition = trainPosition - offsetPercent;
            if (coachPosition < 0) coachPosition += 1.0f; // Wrap around if necessary

            // Set the position of the coach on the spline
            SplineSample sample = new SplineSample();
            splineFollower.spline.Evaluate(coachPosition, ref sample);

            // Smoothly interpolate the position and rotation with adjusted speed
            float interpolationSpeed = 0.00001f; // Adjust this value as needed
            coaches[i].transform.position = Vector3.Lerp(coaches[i].transform.position, sample.position, Time.deltaTime * interpolationSpeed);
            coaches[i].transform.rotation = Quaternion.Slerp(coaches[i].transform.rotation, sample.rotation, Time.deltaTime * interpolationSpeed);
        }
    }
}
