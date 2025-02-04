using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrainsHandler : MonoBehaviour
{
    public static TrainsHandler instance;

    private void OnEnable()
    {
        if (instance == null) instance = this;
    }
    public TrainView[] trainViews;
}
