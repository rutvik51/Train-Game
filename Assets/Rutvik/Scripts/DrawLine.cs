using UnityEngine;
using System.Collections.Generic;
using Dreamteck.Splines;
using System.Collections;

public class DrawLine : MonoBehaviour
{
    public int colorID;
    public LineRenderer lineRenderer;
    public List<Vector3> points = new List<Vector3>();
    private SplineRenderer splineRenderer;
    private string[] colorCodes = { "#FF0000", "#00A9FF", "#4AFF34" };
    void Start()
    {
        splineRenderer = transform.parent.transform.parent.gameObject.GetComponent<SplineRenderer>();
        MeshRenderer meshRenderer = splineRenderer.GetComponent<MeshRenderer>();
        meshRenderer.enabled = true;

        if (colorID >= 0)
        {
            if (ColorUtility.TryParseHtmlString(colorCodes[0], out Color color))
            {
                // Assign the color to the material
                Debug.LogError("Change Colour");
                meshRenderer.material.color = color;
            }
        }
        // if (ColorUtility.TryParseHtmlString(colorCodes[colorID], out Color color))
        // {
        //     // Assign the color to the material
        //     Debug.LogError("Change Colour");
        //     meshRenderer.material.color = color;
        // }

        // meshRenderer.material.SetColor();
        if (lineRenderer == null)
        {
            lineRenderer = gameObject.AddComponent<LineRenderer>();
            lineRenderer.enabled = false;
            lineRenderer.gameObject.SetActive(false);
        }

        lineRenderer.positionCount = 0;
        lineRenderer.startWidth = 1.1f;
        lineRenderer.endWidth = 1.1f;
    }
    Vector3 posE;
    public void DrawLineR()
    {
        SplineComputer spline = splineRenderer.spline;
        if (spline == null || spline.pointCount <= 0)
        {
            return;
        }

        Vector3[] localPositions = new Vector3[spline.pointCount];
        for (int i = 0; i < spline.pointCount; i++)
        {
            if (spline.GetPoint(i).position.y > 0)
            {
                posE = new Vector3(spline.GetPoint(i).position.x, spline.GetPoint(i).position.y, spline.GetPoint(i).position.z);
            }
            else
            {
                posE = new Vector3(spline.GetPoint(i).position.x, 0, spline.GetPoint(i).position.z);
            }
            Vector3 pos = posE;
            localPositions[i] = pos;
        }

        lineRenderer.positionCount = localPositions.Length;
        lineRenderer.SetPositions(localPositions);
    }

    public float delay = 0.5f; // Time delay between removing points

    public void RemoveLine()
    {
        StartCoroutine(RemoveLineCoroutine());
    }

    private IEnumerator RemoveLineCoroutine()
    {
        for (int i = lineRenderer.positionCount - 1; i >= 0; i--)
        {
            lineRenderer.positionCount = i;
            yield return new WaitForSeconds(delay);
        }
    }
    public void ClearPoints()
    {
        lineRenderer.positionCount = 0;
        lineRenderer.startWidth = 1f;
        lineRenderer.endWidth = 1f;

    }
}
