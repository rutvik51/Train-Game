using System.Collections;
using System.Collections.Generic;
using Dreamteck.Splines;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Audio;
public class TrainView : MonoBehaviour
{

    public int trainID;
    public int colourID;
    public bool isTrainCollideWithCave;
    public bool isTrainCollideWithAnotherTrain;
    public bool isTrainCompletedTrack;
    public List<EngineAndWagonView> engineAndWagonViews;
    public GameObject blastParticle;
    public Material[] trainColorMaterials;
    public Animation[] trainAnimations;

    public AudioSource trainRunningSound;
    public AudioSource trainStopSound;
    public AudioSource trainCrashSound;
    public GameObject lineRendererParent;

    [ButtonInspector]
    public void TrainColourSetup()
    {
        for (int i = 0; i < engineAndWagonViews.Count; i++)
        {
            engineAndWagonViews[i].meshRenderer.material = trainColorMaterials[colourID];
        }
    }

    [ButtonInspector]
    public void RemoveSplineFollower()
    {
        for (int i = 1; i < engineAndWagonViews.Count; i++)
        {
            if (engineAndWagonViews[i].GetComponent<SplineFollower>())
            {
                DestroyImmediate(engineAndWagonViews[i].GetComponent<SplineFollower>());
            }
            if (engineAndWagonViews[i].GetComponent<Animation>())
            {
                engineAndWagonViews[i].GetComponent<Animation>().enabled = false;
            }
        }
    }
    private void Start()
    {
        LineRendererActiveStatus();
    }

    public void LineRendererActiveStatus()
    {
        for (int i = 0; i < lineRendererParent.transform.childCount; i++)
        {
            lineRendererParent.transform.GetChild(i).gameObject.SetActive(false);
            DrawLine drawLine = lineRendererParent.transform.GetChild(i).gameObject.GetComponent<DrawLine>();
            drawLine.colorID = colourID;
        }
        lineRendererParent.transform.GetChild(0).gameObject.SetActive(true);
    }
    public void UpdateTrainCompleteCount()
    {
        if (isTrainCompletedTrack) return;
        isTrainCompletedTrack = true;
        LevelHandler.instance.UpdateTrainCompleteCount();
        Debug.LogError("TRAIN COMPLETED:: COUNT::" + LevelHandler.instance.trainCompleteCount);
    }
    public void TrainCollideWithOtherTrain(TrainView otherTrain = null)
    {
        if (isTrainCollideWithAnotherTrain) return;
        isTrainCollideWithAnotherTrain = true;
        TriggerBlastEffect();

        if (otherTrain != null)
        {
            otherTrain.isTrainCollideWithAnotherTrain = true;
            otherTrain.TriggerBlastEffect();
        }

        LevelHandler.instance.LevelFailed();
        Debug.LogError("GAME OVER WITH COLLIDE ANOTHER TRAIN");
    }

    public void TrainCollideWithCave()
    {
        if (isTrainCollideWithCave) return;
        isTrainCollideWithCave = true;
        TriggerBlastEffect();
        LevelHandler.instance.LevelFailed();
        Debug.LogError("GAME OVER WITH DIFFERENT END POINT");
    }

    public void TriggerBlastEffect()
    {
        TrainCrashingSound();
        Vector3 blastCenter = transform.position;

        Instantiate(blastParticle, engineAndWagonViews[0].gameObject.transform.position, Quaternion.identity, transform.parent);

        foreach (var view in engineAndWagonViews)
        {

            view.rb = view.AddComponent<Rigidbody>();
            if (view.rb != null)
            {
                view.boxCollider.isTrigger = false;
                // Enable physics by turning off isKinematic
                view.rb.isKinematic = false;

                if (view.splinefollower != null)
                {
                    view.splinefollower.enabled = false;
                }

                if (view.splinepositioner != null)
                {
                    view.splinepositioner.enabled = false;
                }

                // Apply explosion force
                view.rb.AddExplosionForce(20, blastCenter, 10, 1.0f, ForceMode.Impulse);
            }
        }
    }

    public void StartTrainAnimation()
    {
        for (int i = 0; i < trainAnimations.Length; i++)
        {
            trainAnimations[i].Play();
        }
        StartTrainEngineSound();
    }

    public void StopTrainAnimation()
    {
        for (int i = 0; i < trainAnimations.Length; i++)
        {
            trainAnimations[i].Stop();
        }
        StopTrainEngineSound();
    }

    public void StartTrainEngineSound()
    {
        if (GameController.instance && !GameController.instance.IsSound()) return;
        if (!trainRunningSound.isPlaying)
        {
            trainRunningSound.Play();
        }
    }

    public void StopTrainEngineSound()
    {
        if (GameController.instance && !GameController.instance.IsSound()) return;

        if (trainRunningSound.isPlaying)
        {
            trainRunningSound.Stop();
            trainStopSound.Play();
        }
    }

    public void TrainCrashingSound()
    {
        if (GameController.instance && !GameController.instance.IsSound()) return;

        if (trainRunningSound.isPlaying)
        {
            trainRunningSound.Stop();
        }
        trainCrashSound.Play();
    }
}
