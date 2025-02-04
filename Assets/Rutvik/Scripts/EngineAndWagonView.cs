using System.Collections;
using System.Collections.Generic;
using Dreamteck.Splines;
using Dreamteck.Splines.Examples;
using UnityEngine;

public class EngineAndWagonView : MonoBehaviour
{
    [SerializeField]
    public MeshRenderer meshRenderer;

    private TrainView trainView;
    public int trainID;
    public int colourID;

    [HideInInspector]
    public SplineFollower splinefollower;
    [HideInInspector]
    public SplinePositioner splinepositioner;
    [HideInInspector]
    public BoxCollider boxCollider;

    TrainEngine trainEngine;

    LastWagon LastWagon;

    [HideInInspector]
    public Rigidbody rb;

    bool isCompleted;
    void Start()
    {
        trainEngine = GetComponent<TrainEngine>();
        LastWagon = GetComponent<LastWagon>();
        boxCollider = GetComponent<BoxCollider>();
        splinepositioner = GetComponent<SplinePositioner>();
        splinefollower = GetComponent<SplineFollower>();
        rb = transform.GetComponent<Rigidbody>();
        trainView = transform.parent.gameObject.GetComponent<TrainView>();
        trainID = trainView.trainID;
        colourID = trainView.colourID;

        if (trainEngine || LastWagon)
        {
            offset = new Vector3(0, 0.66f, 0.47f);
        }
    }

    public void DisableAllTrainProperties()
    {
        if (splinefollower != null)
        {
            splinefollower.enabled = false;
        }

        if (splinepositioner != null)
        {
            splinepositioner.enabled = false;
        }
    }

    private Vector3 rayDirection = new Vector3(0, 0, 1);
    private float rayLength = 0.84f;
    private float rayRadius = 0.1f;
    private bool debugRay = true;
    public Vector3 offset;

    public bool isReverse;
    Ray ray;

    void Update()
    {
        RayCastLogic();
    }

    RaycastHit hit;
    public void RayCastLogic()
    {
        if (isReverse)
        {
            ray = new Ray(transform.position + offset, transform.TransformDirection(rayDirection));
        }
        else
        {
            ray = new Ray(transform.position + offset, transform.TransformDirection(rayDirection));
        }

        if (Physics.Raycast(ray, out hit, rayLength))
        {
            EndPoint endPoint = hit.collider.gameObject.GetComponent<EndPoint>();

            if (hit.collider.gameObject.CompareTag("Blast"))
            {
                CollideDetection detection = hit.collider.gameObject.GetComponent<CollideDetection>();

                if (!detection.trainView.isTrainCollideWithAnotherTrain && detection.trainView.trainID != trainID)
                {
                    trainView.TrainCollideWithOtherTrain(detection.trainView);
                    Debug.LogError("Hit Object Name: " + hit.collider.gameObject.name);
                }

            }

            if (endPoint != null)
            {
                if (endPoint.isPowerUp)
                {
                    if (trainEngine != null)
                    {
                        endPoint.PlayTunelAnimation();
                        trainEngine.OnCollisionAction(endPoint.transform.GetChild(0));
                        trainView.UpdateTrainCompleteCount();
                    }

                    if (LastWagon != null)
                    {
                        endPoint.PlayTunelAnimation();
                        LastWagon.engine.OnCollisionAction(endPoint.transform.GetChild(0));
                        trainView.UpdateTrainCompleteCount();
                    }
                    return;
                }

                if (endPoint.colourID == colourID)
                {
                    if (trainEngine != null)
                    {
                        endPoint.PlayTunelAnimation();
                        trainEngine.OnCollisionAction(endPoint.transform.GetChild(0));
                        trainView.UpdateTrainCompleteCount();
                    }

                    if (LastWagon != null)
                    {
                        endPoint.PlayTunelAnimation();
                        LastWagon.engine.OnCollisionAction(endPoint.transform.GetChild(0));
                        trainView.UpdateTrainCompleteCount();
                    }
                }
                else
                {
                    if (trainEngine != null)
                    {
                        trainEngine.OnCollisionAction(endPoint.transform.GetChild(0));
                    }
                    trainView.TrainCollideWithCave();
                }
            }

        }
    }

    void OnDrawGizmos()
    {
        if (debugRay)
        {
            // Set the Gizmos color
            Gizmos.color = Color.red;

            // Draw a thicker ray using a cylinder to represent the radius
            Gizmos.DrawRay(transform.position + offset, transform.TransformDirection(rayDirection) * rayLength);
            Gizmos.DrawWireSphere(transform.position + offset + transform.TransformDirection(rayDirection) * rayLength, rayRadius);
        }
    }
}
