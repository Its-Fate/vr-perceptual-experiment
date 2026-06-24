using UnityEngine;
using UnityEngine.UI;

public class StimulusManager : MonoBehaviour
{
    // --- Configurable fields ---
    [Header("Grating Objects")]
    // To be assigned in Inspector
    public GameObject leftStimulus; 
    public GameObject rightStimulus;

    private GratingController leftGrating;
    private GratingController rightGrating;

    void Start()
    {
        if (leftStimulus != null)
            leftGrating =  leftStimulus.GetComponent<GratingController>();
        if (rightStimulus != null)
            rightGrating = rightStimulus.GetComponent<GratingController>();
    }

    // Update the parameters of the gratings based on the trial specification 
    public void SetTrialParameters(TrialSpec spec)
    {
        if (leftGrating != null)
        {
            leftGrating.speed = spec.speedL;
            leftGrating.direction = spec.directionL;
            leftGrating.contrast = spec.contrastL;
            leftGrating.frequency = spec.frequencyL;
        }
        if (rightGrating != null)
        {
            rightGrating.speed = spec.speedR;
            rightGrating.direction = spec.directionR;
            rightGrating.contrast = spec.contrastR;
            rightGrating.frequency = spec.frequencyR;
        }
    }

    // Display the stimuli by enabling the GameObjects
    public void ShowStimuli()
    {
        if (leftStimulus != null)
            leftStimulus.SetActive(true);
        if (rightStimulus != null)
            rightStimulus.SetActive(true);
    }

    // Hide the stimuli by disabling the GameObjects
    public void HideStimuli()
    {
        if (leftStimulus != null)
            leftStimulus.SetActive(false);
        if (rightStimulus != null)
            rightStimulus.SetActive(false);
    }
}
