using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
public class SettingPanel : MonoBehaviour
{
    private const string HapticKey = "HapticEnabled";
    private const string SoundKey = "SoundEnabled";

    public bool isHapticEnabled;
    public bool isSoundEnabled;

    public Image soundSliderBG;
    public Image soundToggle;
    public Image hapticSliderBG;
    public Image hapticToggle;

    public Sprite activeSlider;
    public Sprite deActiveSlider;
    public Sprite activeToggle;
    public Sprite deActiveToggle;
    public GameObject debugIndication;
    private void Start()
    {
        isHapticEnabled = PlayerPrefs.GetInt(HapticKey, 1) == 1;
        isSoundEnabled = PlayerPrefs.GetInt(SoundKey, 1) == 1;

        UpdateHapticState();
        UpdateSoundState();
    }

    [ButtonInspector]
    public void ToggleHaptic()
    {
        isHapticEnabled = !isHapticEnabled;
        PlayerPrefs.SetInt(HapticKey, isHapticEnabled ? 1 : 0);
        PlayerPrefs.Save();
        UpdateHapticState();
    }

    [ButtonInspector]
    public void ToggleSound()
    {
        isSoundEnabled = !isSoundEnabled;
        PlayerPrefs.SetInt(SoundKey, isSoundEnabled ? 1 : 0);
        PlayerPrefs.Save();
        UpdateSoundState();
    }

    private void UpdateHapticState()
    {
        if (isHapticEnabled)
        {
            hapticToggle.sprite = activeToggle;
            hapticSliderBG.sprite = activeSlider;
            hapticToggle.rectTransform.DOAnchorPosX(60, 0.3f);
        }
        else
        {
            hapticToggle.sprite = deActiveToggle;
            hapticSliderBG.sprite = deActiveSlider;
            hapticToggle.rectTransform.DOAnchorPosX(-60, 0.3f);
        }
    }

    private void UpdateSoundState()
    {
        if (isSoundEnabled)
        {
            soundToggle.sprite = activeToggle;
            soundSliderBG.sprite = activeSlider;
            soundToggle.rectTransform.DOAnchorPosX(60, 0.3f);
        }
        else
        {
            soundToggle.sprite = deActiveToggle;
            soundSliderBG.sprite = deActiveSlider;
            soundToggle.rectTransform.DOAnchorPosX(-60, 0.3f);
        }

        GameController.instance.ApplyMuteState();
    }

    public void Show()
    {
        transform.localScale = Vector3.zero;
        this.gameObject.SetActive(true);
        transform.DOScale(Vector3.one, 0.5f);
        LevelHandler.instance.DisablePointController();
    }

    public void OnClickBack()
    {
        LevelHandler.instance.EnablePointController();
        Hide();
    }

    public void Hide()
    {
        this.gameObject.SetActive(false);
    }

    public void OnClickSkip()
    {
        GameController.instance.OnClickNext();
        Hide();
    }

    private int clickCount = 0;
    private const int maxClicks = 6;
    private bool isActive = false;
    public Image debugIndicationImage;
    public Color greenColor = Color.green;
    public Color redColor = Color.red;

    public void OnButtonClick()
    {
        clickCount++;

        if (clickCount >= maxClicks)
        {
            isActive = !isActive;

            // Set color based on the boolean state
            debugIndicationImage = debugIndication.GetComponent<Image>();
            debugIndicationImage.color = isActive ? greenColor : redColor;

            debugIndication.SetActive(true);

            // Delayed hiding of the indication object
            DOVirtual.DelayedCall(0.3f, () =>
            {
                debugIndication.SetActive(false);
            });

            GameController.instance.isReverseBtnTesting = isActive;
            clickCount = 0;

            Debug.Log($"Boolean state changed: {isActive}");
        }
    }
}
