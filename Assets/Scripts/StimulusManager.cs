using UnityEngine;
using UnityEngine.UI;

public class StimulusManager : MonoBehaviour
{
    // --- Configurable fields ---
    [Header("Stimulus Images")]
    // To be assigned in Inspector
    public Sprite[] images; 

    [Header("UI Elements")]
    // To be assigned in Inspector
    public Image leftEyeDisplay;
    public Image rightEyeDisplay;

    // Show a different stimuli to each eye
    public void ShowStimuli()
    {
        if (images != null && images.Length > 0)
        {
            leftEyeDisplay.sprite = images[UnityEngine.Random.Range(0, images.Length)];
            rightEyeDisplay.sprite = images[UnityEngine.Random.Range(0, images.Length)];
        }

        // Show image and update UI immidiately
        leftEyeDisplay.gameObject.SetActive(true);
        rightEyeDisplay.gameObject.SetActive(true);
        Canvas.ForceUpdateCanvases();
    }

    public void HideStimuli()
    {
        if (leftEyeDisplay != null && rightEyeDisplay != null) 
        {
            leftEyeDisplay.gameObject.SetActive(false);
            rightEyeDisplay.gameObject.SetActive(false);
        }
    }
}
