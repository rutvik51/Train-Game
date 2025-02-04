using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollideDetection : MonoBehaviour
{
    public TrainView trainView;
    public EngineAndWagonView engineAndWagonView;
    public EndPoint endpoint;

    void Start()
    {
        GameObject trainViewobject = transform.parent.transform.parent.gameObject;
        GameObject engineAndWagonViewObject = transform.parent.gameObject;

        Debug.Log("Parent::" + trainViewobject.name);
        trainView = trainViewobject.GetComponent<TrainView>();
        engineAndWagonView = engineAndWagonViewObject.GetComponent<EngineAndWagonView>();
    }
}
