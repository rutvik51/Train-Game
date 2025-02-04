using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIController : MonoBehaviour
{
    public static UIController instance;

    private void Awake()
    {
        instance = this;
    }
    public GameplayScreen gamePlayPanel;
    public LevelFailedScreen levelFailedPanel;
    public LevelPassScreen levelPassPanel;

    public FadeController fadeControllerPanel;

    public SettingPanel settingPanel;
    public void EnableLevelCompleteScreen()
    {
        if (AudioController.instance != null)
        {
            AudioController.instance.PlayLevelCompleteSound();
        }
        levelPassPanel.Show();
    }

    public void EnableLevelFailedScreen()
    {
        levelFailedPanel.Show();
    }

    public void EnableSettingScreen()
    {
        settingPanel.Show();
    }

    [ButtonInspector]
    public void DoFadeIn()
    {
        fadeControllerPanel.FadeIn();
    }

    [ButtonInspector]
    public void DoFadeOut()
    {
        fadeControllerPanel.FadeOut();
    }


}
