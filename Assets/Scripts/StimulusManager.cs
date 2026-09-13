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

        #if UNITY_ANDROID && !UNITY_EDITOR
            // VR headset: set distance to 200mm (0.2 units)
            if (leftStimulus != null && rightStimulus != null)
            {
                leftStimulus.transform.position = new Vector3(-1.5f, 0, 0.2f);
                rightStimulus.transform.position = new Vector3(1.5f, 0, 0.2f);
            }                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                    
        #endif
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
            leftGrating.gaussianSharpness = spec.gaussianSharpnessL;
        }
        if (rightGrating != null)
        {
            rightGrating.speed = spec.speedR;
            rightGrating.direction = spec.directionR;
            rightGrating.contrast = spec.contrastR;
            rightGrating.frequency = spec.frequencyR;
            rightGrating.gaussianSharpness = spec.gaussianSharpnessR;
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
