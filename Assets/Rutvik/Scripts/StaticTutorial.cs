using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StaticTutorial : MonoBehaviour
{
    public PointConroller pointController;
    void Start()
    {
        if (GameController.instance.fakeLevelIndex == 6)
        {
            pointController.enabled = false;
            UIController.instance.gamePlayPanel.ReverseBtnEnableState(false);
            transform.GetChild(0).gameObject.SetActive(true);
        }
        else
        {
            transform.GetChild(0).gameObject.SetActive(false);
        }
    }

    public void OnClickNextBtnOnTutorial()
    {
        pointController.enabled = true;
        transform.GetChild(0).gameObject.SetActive(false);
        UIController.instance.gamePlayPanel.ActiveForceTutorialForReverse();
        UIController.instance.gamePlayPanel.ReverseBtnEnableState(true);
    }
}
