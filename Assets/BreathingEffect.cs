using UnityEngine;

public class BreathingEffect : MonoBehaviour
{
    [Header("Breathing Settings")]
    public float scaleMultiplier = 1.1f; // Maximum scale multiplier for the breathing effect
    public float breathingSpeed = 1.0f; // Speed of the breathing effect

    private Vector3 originalScale; // The original scale of the object
    private bool isBreathing = false; // Whether the breathing effect is active

    void OnEnable()
    {
        // Store the original scale and start the breathing effect
        originalScale = transform.localScale;
        isBreathing = true;
    }

    void OnDisable()
    {
        // Stop the breathing effect and reset the scale
        isBreathing = false;
        transform.localScale = originalScale;
    }

    void Update()
    {
        if (isBreathing)
        {
            // Create a pulsing effect using Mathf.Sin
            float scale = Mathf.Sin(Time.time * breathingSpeed) * 0.5f + 0.5f; // Range [0, 1]
            scale = Mathf.Lerp(1.0f, scaleMultiplier, scale); // Scale between original and max
            transform.localScale = originalScale * scale;
        }
    }
}
