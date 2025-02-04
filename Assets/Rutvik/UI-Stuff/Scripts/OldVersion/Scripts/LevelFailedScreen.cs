using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
public class LevelFailedScreen : MonoBehaviour
{
    public GameObject[] uiElements;

    private void OnEnable()
    {
        for (int i = 0; i < uiElements.Length; i++)
        {
            uiElements[i].transform.localScale = Vector3.zero;
        }
        transform.DOScale(Vector3.one, 0.5f).OnComplete(() =>
        {
            StartCoroutine(ButtonsEnable());
            ButtonsEnable();
        });
    }

    public IEnumerator ButtonsEnable()
    {
        for (int i = 0; i < uiElements.Length; i++)
        {
            uiElements[i].transform.DOScale(Vector3.one, 0.5f);
            yield return new WaitForSeconds(0.4f);
        }
    }
    public void OnClickRetry()
    {
        Hide();

        GameController.instance.OnClickReplay();
    }

    public void Show()
    {
        // transform.localScale = Vector3.zero;
        // transform.DOScale(Vector3.one, 1);
        this.gameObject.SetActive(true);
    }

    public void Hide()
    {
        this.gameObject.SetActive(false);
    }
}
