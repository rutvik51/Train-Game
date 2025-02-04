using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class GameplayScreen : MonoBehaviour
{
    public TMP_Text levelText;
    public Button retryBtn;
    public Button reverseBtn;
    public Image fingerSprite;
    public Image reversefingerSprite;
    public Image fingerForThirdTutSprite;
    public GameObject hintMessagePopup;
    public TMP_Text hintMessageText;
    public GameObject noAdsPopup;
    public GameObject skipBtn;

    public GameObject noadsIcon, countIcon;
    public GameObject forceTutorialPanel;
    private void OnEnable()
    {
        retryBtn.onClick.AddListener(OnClickRetry);
        SyncReverseBtnStatus();
        ReverseBtnEnableState(true);
        UpdateLevel_Index_Text();
    }
    public void OnClickReplay()
    {
        GameController.instance.OnClickReplay();
    }

    public void OnClickSettingBtn()
    {
        UIController.instance.EnableSettingScreen();
    }

    public void UpdateLevel_Index_Text()
    {
        levelText.text = "LEVEL " + (GameController.instance.fakeLevelIndex + 1);
    }

    public void OnClickRetry()
    {
        GameController.instance.OnClickReplay();
        retryBtn.gameObject.SetActive(false);

        DOVirtual.DelayedCall(1.5f, () =>
        {
            retryBtn.gameObject.SetActive(true);
        });
    }
    public void Show()
    {
        this.gameObject.SetActive(true);
    }

    public void Hide()
    {
        this.gameObject.SetActive(false);
    }
    public void OnClickReturn()
    {
        if (LevelHandler.instance.isReverseBtnUsedOneTime && !GameController.instance.isReverseBtnTesting
        && GameController.instance.fakeLevelIndex != 6)
        {
            ShowNoAdsPopup();
            return;
        }

        if (GameController.instance.fakeLevelIndex != 6)
        {
            LevelHandler.instance.ReverseBtnUsed();
        }
        TrainsManager.Instance.ReverseButtonAction();
        if (!GameController.instance.IsReverseTutorialCompleted())
        {
            ShowHintPopup();
        }
        ReverseBtnEnableState(false);
    }

    public void ReverseBtnEnableState(bool state)
    {
        if (state)
        {
            SyncReverseBtnStatus();
            HideHintPopup();
        }
        else
        {
        }
        reverseBtn.gameObject.SetActive(state);
    }

    bool isReverseUsed;
    public void SyncReverseBtnStatus()
    {
        if (LevelHandler.instance)
        {
            isReverseUsed = LevelHandler.instance.isReverseBtnUsedOneTime;
        }
        noadsIcon.SetActive(isReverseUsed);
        countIcon.SetActive(!isReverseUsed);
    }
    public void TutorialforFirstLevel()
    {
        fingerSprite.gameObject.SetActive(true);
        fingerSprite.rectTransform.DOAnchorPosY(630, 2).SetLoops(-1, LoopType.Restart);
    }

    public void TutorialforReverseLevel()
    {
        forceTutorialPanel.SetActive(false);
        LevelHandler.instance.pointController.enabled = true;
        reversefingerSprite.gameObject.SetActive(true);
        reversefingerSprite.rectTransform.DOAnchorPosY(630, 2).SetLoops(-1, LoopType.Restart);
        TriggerActionsAfterDelay();
    }

    public void ActiveForceTutorialForReverse()
    {
        forceTutorialPanel.SetActive(true);
    }
    public void TurnOffTutorialOfReverseLevel()
    {
        reversefingerSprite.gameObject.SetActive(false);
    }

    public void TutorialforThirdLevel()
    {
        fingerForThirdTutSprite.gameObject.SetActive(true);
        fingerForThirdTutSprite.rectTransform.DOAnchorPosY(-400, 2).SetLoops(-1, LoopType.Restart);
    }

    public void TurnOffTutorial()
    {
        fingerSprite.gameObject.SetActive(false);
        fingerForThirdTutSprite.gameObject.SetActive(false);
    }

    #region POPUP
    [ButtonInspector]
    public void ShowNoAdsPopup()
    {
        if (noAdsPopup.activeInHierarchy) return;

        noAdsPopup.SetActive(true);

        noAdsPopup.transform.localScale = Vector3.zero;

        noAdsPopup.transform.DOScale(Vector3.one, 0.5f)
            .SetEase(Ease.OutBack)
            .OnComplete(() =>
            {
                DOVirtual.DelayedCall(1, () =>
                {
                    HideNoAdsPopup();
                });
            });
    }

    private void HideNoAdsPopup()
    {
        noAdsPopup.transform.DOScale(Vector3.zero, 0.5f)
            .SetEase(Ease.InBack)
            .OnComplete(() =>
            {
                noAdsPopup.SetActive(false);
            });
    }

    [ButtonInspector]
    public void ShowHintPopup()
    {
        if (hintMessagePopup.activeInHierarchy) return;

        if (GameController.instance.fakeLevelIndex == 6)
        {
            hintMessageText.text = "Hold and Drag the last wagon back";
        }
        else
        {
            hintMessageText.text = "Hold and drag any train backward";
        }
        hintMessagePopup.SetActive(true);

        hintMessagePopup.transform.localScale = Vector3.zero;

        hintMessagePopup.transform.DOScale(Vector3.one, 0.5f)
            .SetEase(Ease.OutBack);
    }

    public void HideHintPopup()
    {
        hintMessagePopup.transform.DOScale(Vector3.zero, 0.1f)
            .SetEase(Ease.InBack)
            .OnComplete(() =>
            {
                hintMessagePopup.SetActive(false);
            });
    }

    #endregion


    #region TUTORIAL SKIP BUTTON
    public void TriggerActionsAfterDelay()
    {
        StartCoroutine(ExecuteAfterDelay());
    }

    private IEnumerator ExecuteAfterDelay()
    {
        yield return new WaitForSeconds(10f);

        if (GameController.instance.fakeLevelIndex == 6)
        {
            skipBtn.SetActive(true);
        }
    }

    public void DeactiveSkipBtn()
    {
        skipBtn.SetActive(false);
    }

    public void OnClickSkip()
    {
        GameController.instance.OnClickNext();
        DeactiveSkipBtn();
    }
    #endregion

}
