using System.Collections;
using DG.Tweening;
using UnityEngine;

public class GameController : MonoBehaviour
{
    public static GameController instance;
    public int currentLevelIndex;
    public int fakeLevelIndex;
    public int restartLevelIndex = 0;
    public GameObject[] levels;
    public bool isTest = false;
    public bool isReverseBtnTesting = false;
    public int directLevelIndex = 0;


    private FadeController fadeController;
    private GameObject currentLevel;
    private int currentIndex;

    private void Awake()
    {
        instance = this;
        if (directLevelIndex > 0 && !isTest)
        {
            PlayerPrefs.SetInt("LevelIndex", directLevelIndex);
        }

        // SupersonicWisdom.Api.AddOnReadyListener(OnSupersonicWisdomReady);
        // SupersonicWisdom.Api.Initialize();

        // void OnSupersonicWisdomReady()
        // {
        //     Init();
        // }

        ApplyMuteState();
    }
    private void Start()
    {
        Application.targetFrameRate = 60;
        Init();
    }

    private void Update()
    {
        currentLevelIndex = LEVELINDEX();

        if (Input.GetKeyDown(KeyCode.Space))
        {
            OnClickNext();
        }
    }

    public void Init()
    {
        fadeController = UIController.instance.fadeControllerPanel;
        fadeController.FadeOut();

        if (isTest)
        {
            currentIndex = directLevelIndex;
        }
        else
        {
            currentIndex = LEVELINDEX();
            Debug.LogError("FAKE INDEX::" + FAKELEVELINDEX());
            fakeLevelIndex = FAKELEVELINDEX();
        }

        LoadLevel(currentIndex);
        UIController.instance.gamePlayPanel.UpdateLevel_Index_Text();
    }

    public void OnClickNext()
    {
        if (!isTest)
        {
            LevelCompleteEvent();
            currentIndex++;
            fakeLevelIndex++;
            if (currentIndex >= levels.Length)
            {
                currentIndex = restartLevelIndex;
            }

            PlayerPrefs.SetInt("LevelIndex", currentIndex);
            PlayerPrefs.SetInt("FakeIndex", fakeLevelIndex);
        }
        StartCoroutine(SwitchLevel(currentIndex));
    }

    public void OnClickReplay()
    {
        LevelFailedEvent();
        StartCoroutine(SwitchLevel(currentIndex));
        LevelStartEvent();
    }

    private IEnumerator SwitchLevel(int levelIndex)
    {
        fadeController.FadeIn(() =>
        {
            if (currentLevel != null)
                Destroy(currentLevel);
        });

        yield return new WaitForSeconds(1f);


        // Load and fade OUT
        LoadLevel(levelIndex);
        UIController.instance.gamePlayPanel.UpdateLevel_Index_Text();
        fadeController.FadeOut();
    }

    private void LoadLevel(int levelIndex)
    {
        if (levelIndex >= 0 && levelIndex < levels.Length)
        {
            currentLevel = Instantiate(levels[levelIndex]);
        }
        else
        {
            Debug.LogWarning("Level index out of range.");
        }
    }

    public int LEVELINDEX()
    {
        return PlayerPrefs.GetInt("LevelIndex", 0);
    }

    public int FAKELEVELINDEX()
    {
        return PlayerPrefs.GetInt("FakeIndex");
    }

    private const string HapticKey = "HapticEnabled";
    private const string SoundKey = "SoundEnabled";
    private const string reverseTutorial = "ReverseTutorial";

    public bool IsReverseTutorialCompleted()
    {
        return PlayerPrefs.GetInt(reverseTutorial, 0) == 1;
    }

    public void CompleteTutorial()
    {
        PlayerPrefs.SetInt(reverseTutorial, 1);
        PlayerPrefs.Save();
    }

    public bool ISHaptic()
    {
        if (PlayerPrefs.GetInt(HapticKey, 1) == 1)
        {
            return true;
        }

        return false;
    }

    public bool IsSound()
    {
        if (PlayerPrefs.GetInt(SoundKey, 1) == 1)
        {
            return true;
        }

        return false;
    }
    public void ApplyMuteState()
    {
        // AudioListener.volume = IsSound() ? 0 : 1;
    }

    public void LevelStartEvent()
    {
        Debug.LogError("----LEVEL START::" + fakeLevelIndex);
        NotifyLevelStarted(fakeLevelIndex);
    }

    public void LevelFailedEvent()
    {
        Debug.LogError("----LEVEL FAILED::" + fakeLevelIndex);
        NotifyLevelFailed(fakeLevelIndex);
    }

    public void LevelCompleteEvent()
    {
        Debug.LogError("----LEVEL COMPLETED::" + fakeLevelIndex);
        NotifyLevelCompleted(fakeLevelIndex);
    }

    public void NotifyLevelStarted(int currentLevel)
    {
        // SupersonicWisdom.Api.NotifyLevelStarted(ESwLevelType.Regular, currentLevel, null);
        Debug.Log($"Level {currentLevel} started.");
        Debug.Log("Level _" + currentLevel + "_Start");
    }

    public void NotifyLevelCompleted(int currentLevel)
    {
        // SupersonicWisdom.Api.NotifyLevelCompleted(ESwLevelType.Regular, currentLevel, null);
        Debug.Log($"Level {currentLevel} ended.");
        Debug.Log("Level _" + currentLevel + "_Completed");
    }

    public void NotifyLevelFailed(int currentLevel)
    {
        // SupersonicWisdom.Api.NotifyLevelFailed(ESwLevelType.Regular, currentLevel, null);
        Debug.Log($"Level {currentLevel} failed.");
        Debug.Log("Level _" + currentLevel + "_Failed");
    }

}
