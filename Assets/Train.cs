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
        if (!isFollow) return;
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

        for (int i = 0; i < coaches.Length; i++)
        {
            float offsetPercent;
            float firstengineOffset = (float)(engineToCoachOffset / splineFollower.spline.CalculateLength());
            // Calculate the offset percentage for each coach
            if (i == 0)
            {
                offsetPercent = firstengineOffset;
            }
            else
            {
                offsetPercent = firstengineOffset + (float)(i * coachToCoachOffset / splineFollower.spline.CalculateLength());
            }

            // Calculate the position on the spline for each coach relative to the train
            float coachPosition = trainPosition - offsetPercent;
            if (coachPosition < 0) coachPosition += 1; // Wrap around if necessary

            // Set the position of the coach on the spline
            SplineSample sample = new SplineSample();
            splineFollower.spline.Evaluate(coachPosition, ref sample);
            coaches[i].transform.position = sample.position;
            coaches[i].transform.rotation = sample.rotation;
        }
    }
}
