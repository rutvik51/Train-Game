using System.Collections;
using System.Collections.Generic;
using Dreamteck.Splines;
using Dreamteck.Splines.Examples;
using UnityEngine;

public class TrainsManager : MonoBehaviour
{
    public static TrainsManager Instance;
    public List<TrainEngine> trains;
    public bool isReverse;
    private void OnEnable()
    {
        if (Instance == null) Instance = this;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            ReverseButtonAction();
        }
    }
    public void ReverseButtonAction()
    {
        if (!isReverse)
        {
            isReverse = true;
            foreach (var item in trains)
            {
                item.Reverse();

            }
        }
        else
        {
            isReverse = false;
            foreach (var item in trains)
            {
                item.Normal();

            }
        }

    }
    public void GoBackNormalDirection()
    {
        if (isReverse)
        {
            isReverse = false;
            foreach (var item in trains)
            {
                item.Normal();

            }
        }

    }
}
