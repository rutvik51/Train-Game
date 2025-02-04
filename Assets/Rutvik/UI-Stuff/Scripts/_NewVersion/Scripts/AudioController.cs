using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioController : MonoBehaviour
{
    public static AudioController instance;
    [Header("Sound Effects")]
    public AudioSource[] levelCompleteSFX;
    public AudioSource[] tunnelSFX;

    private void Awake()
    {
        instance = this;
    }

    public void PlaySound(AudioSource[] audioSources)
    {
        if (!GameController.instance.IsSound()) return;

        if (audioSources == null || audioSources.Length == 0) return;

        int randomIndex = Random.Range(0, audioSources.Length);
        if (audioSources[randomIndex] != null)
        {
            audioSources[randomIndex].Play();
        }
    }

    public void StopSound(AudioSource[] audioSources)
    {

        if (audioSources == null || audioSources.Length == 0) return;

        foreach (var audioSource in audioSources)
        {
            if (audioSource != null)
            {
                audioSource.Stop();
            }
        }
    }

    public void PlayLevelCompleteSound()
    {
        PlaySound(levelCompleteSFX);
    }

    public void PlayTunnelSound()
    {
        PlaySound(tunnelSFX);
    }
}
