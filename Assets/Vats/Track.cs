using UnityEngine;

public class Track : MonoBehaviour
{
    public enum TrackType { Straight, Curved };
    public TrackType type;
    public Transform startP;
    public Transform endP;

    private void Start()
    {
        // if (type == TrackType.Straight)
        // {
        //     // startP = transform.GetChild(0);
        //     // endP = transform.GetChild(1);

        //     float length = Vector3.Distance(startP.position, endP.position);

        //     Vector3 offset = Vector3.zero;

        //     if (transform.eulerAngles.y == 0)
        //     {
        //         offset = Vector3.forward;
        //     }
        //     else if (transform.eulerAngles.y == 90)
        //     {
        //         offset = Vector3.right;
        //     }
        //     else if (transform.eulerAngles.y == 180)
        //     {
        //         offset = Vector3.forward;
        //     }
        //     else if (transform.eulerAngles.y == 270)
        //     {
        //         offset = Vector3.left;
        //     }

        //     for (int i = 1; i < length; i++)
        //     {
        //         // GameObject node = Instantiate(startP.gameObject, startP.position + offset * i, Quaternion.identity);
        //         // node.transform.SetParent(transform);
        //     }
        // }
        // else
        // {
        //     // startP = transform.GetChild(0);
        //     // endP = transform.GetChild(1);

        //     // GameObject node = Instantiate(startP.gameObject, GenerateMidpoints(startP.position, endP.position), Quaternion.identity);
        //     // node.transform.SetParent(transform);
        // }
    }

    private float threshold = Mathf.Sqrt(2);
    // void GenerateMidpoints(Vector3 A, Vector3 B)
    // {
    //     float length = Vector3.Distance(A, B);

    //     if (length <= threshold)
    //     {
    //         return;
    //     }

    //     GameObject node = Instantiate(startP.gameObject, (A + B) / 2, Quaternion.identity);
    //     node.transform.SetParent(transform);
    //     node.transform.localPosition /= 2;
    //     node.name = "Corner";

    //     Vector3 C = node.transform.position;

    //     GenerateMidpoints(A, C);  // Divide the left half
    //     GenerateMidpoints(C, B);  // Divide the right half
    // }

    public Vector3 GenerateMidpoints(Vector3 A, Vector3 B)
    {
        // Chord midpoint
        Vector3 chordMidpoint = (A + B) / 2f;

        // Chord vector and its perpendicular
        Vector3 chord = B - A;
        Vector3 perpendicular = Vector3.Cross(chord, Vector3.forward).normalized;

        // Calculate the radius using the perpendicular bisector method
        float chordLength = chord.magnitude;
        float halfChordLength = chordLength / 2f;
        float sagitta = 0.5f; // Estimated sagitta (adjust if needed based on arc curvature)
        float radius = (halfChordLength * halfChordLength + sagitta * sagitta) / (2 * sagitta);

        // Find the circle center
        Vector3 center = chordMidpoint + perpendicular * (radius - sagitta);

        // Calculate angles
        Vector3 dirA = (A - center).normalized;
        Vector3 dirB = (B - center).normalized;
        float angle = Vector3.SignedAngle(dirA, dirB, Vector3.forward);

        // Rotate dirA by half the angle to find the arc midpoint
        Quaternion rotation = Quaternion.AngleAxis(angle / 2f, Vector3.forward);
        Vector3 arcMidpoint = center + rotation * dirA * radius;

        // Arc facing direction (tangent at the midpoint)
        Vector3 radiusVector = arcMidpoint - center;
        Vector3 arcDirection = Vector3.Cross(Vector3.forward, radiusVector).normalized;

        // Debugging
        Debug.DrawLine(A, B, Color.green, 5f); // Chord
        Debug.DrawLine(chordMidpoint, center, Color.yellow, 5f); // Perpendicular bisector
        Debug.DrawLine(center, arcMidpoint, Color.blue, 5f); // Radius
        Debug.DrawRay(arcMidpoint, arcDirection, Color.red, 5f); // Tangent (facing direction)

        // Log the results
        Debug.Log("Arc Midpoint: " + arcMidpoint);
        Debug.Log("Center: " + center);
        Debug.Log("Radius: " + radius);
        Debug.Log("Facing Direction (transform.forward): " + arcDirection);

        return arcMidpoint;
    }
}
