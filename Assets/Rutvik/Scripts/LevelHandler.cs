using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using Unity.VisualScripting;
using UnityEngine;

public class LevelHandler : MonoBehaviour
{

    public bool isLevelFailed;
    public bool islevelCompleted;
    public bool isReverseBtnUsedOneTime;
    public int trainTotalCount;
    public int trainCompleteCount;


    public static LevelHandler instance;

    public PointConroller pointController;
    private void OnEnable()
    {
        if (instance == null) instance = this;

        pointController = FindAnyObjectByType<PointConroller>();
    }

    private void Start()
    {
        trainTotalCount = TrainsHandler.instance.trainViews.Count();
        if (GameController.instance)
            GameController.instance.LevelStartEvent();

        TutorialSetup();

        if (GameController.instance && GameController.instance.fakeLevelIndex >= 6)
        {
            UIController.instance.gamePlayPanel.ReverseBtnEnableState(true);
        }
        else
        {
            UIController.instance.gamePlayPanel.ReverseBtnEnableState(false);
        }

        if (UIController.instance)
        {
            UIController.instance.gamePlayPanel.DeactiveSkipBtn();
        }
    }
    public void LevelFailed()
    {
        isLevelFailed = true;
        if (UIController.instance)
        {
            DOVirtual.DelayedCall(2, () =>
            {
                UIController.instance.EnableLevelFailedScreen();
            });
        }
    }

    public void LevelCompleted()
    {
        if (UIController.instance)
        {
            AudioController.instance.PlayLevelCompleteSound();
            UIController.instance.EnableLevelCompleteScreen();
        }
        Debug.LogError("LEVEL COMPLETED");
    }

    public bool IsGameCompleted()
    {
        if (trainCompleteCount >= trainTotalCount)
        {
            islevelCompleted = true;
            return true;
        }
        islevelCompleted = false;
        return false;
    }

    public void UpdateTrainCompleteCount()
    {
        trainCompleteCount += 1;

        if (IsGameCompleted())
        {
            LevelCompleted();
        }
    }

    public void OnClickNextBtnOnTutorialPanel()
    {

    }

    public void TutorialSetup()
    {
        if (GameController.instance.fakeLevelIndex == 0)
        {
            UIController.instance.gamePlayPanel.TutorialforFirstLevel();
        }
        if (GameController.instance.fakeLevelIndex == 2)
        {
            UIController.instance.gamePlayPanel.TutorialforThirdLevel();
        }
        if (GameController.instance.fakeLevelIndex == 1 || GameController.instance.fakeLevelIndex > 2)
        {
            UIController.instance.gamePlayPanel.TurnOffTutorial();
        }
    }

    public void EnablePointController()
    {
        pointController.enabled = true;
    }

    public void DisablePointController()
    {
        pointController.enabled = false;
    }

    public void ReverseBtnUsed()
    {
        isReverseBtnUsedOneTime = true;
    }

    public Transform camPos1;
    public Transform camPos2;
    public float cameraMoveDuration = 1.0f; // Duration for the camera to move between positions

    private bool isAtCamPos1 = true; // Track the current camera position

    public void MoveCamera()
    {
        if (camPos1 == null || camPos2 == null)
        {
            Debug.LogError("Camera positions are not set.");
            return;
        }

        Transform targetPosition = isAtCamPos1 ? camPos2 : camPos1;
        StartCoroutine(MoveCameraCoroutine(targetPosition));
        isAtCamPos1 = !isAtCamPos1;
    }

    private IEnumerator MoveCameraCoroutine(Transform targetPosition)
    {
        float elapsedTime = 0f;
        Vector3 startingPosition = Camera.main.transform.position;

        while (elapsedTime < cameraMoveDuration)
        {
            Camera.main.transform.position = Vector3.Lerp(startingPosition, targetPosition.position, (elapsedTime / cameraMoveDuration));
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        Camera.main.transform.position = targetPosition.position;
    }

}
