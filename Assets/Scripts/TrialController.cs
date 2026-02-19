using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;

public class TrialController : MonoBehaviour
{
    // --- Configurable fields ---
    public StimulusManager stimulusManager; // To be assigned in Inspector

    // --- Internal state ---
    private TrialData currentTrial;
    private bool whichEye;
    private float inputTime;
    private float trialStartTime;
    private bool waitingForResponse = false;

    // --- Initialize input method ---
    private IResponseInput responseInput;
    private void Awake()
    {
        // Using keyboard input for now (can be changed to other input methods later)
        responseInput = new KeyboardInput();
    }

    // A data structure to record each trial's data
    [System.Serializable]
    public class TrialData
    {
        public int trialNumber;
        public bool whichEye; // true for right eye, false for left eye
        public float responseTime;
        public string timestamp;
    }

    // Run a single trial. FlowController will call this and collect the TrialData.
    public IEnumerator RunTrial(int trialNumber, Action<TrialData> onFinished)
    {
        // Initialize trial data
        currentTrial = new TrialData
        {
            trialNumber = trialNumber,
            timestamp = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff")
        };

        // Show stimuli to each eye separately
        stimulusManager.ShowStimuli();

        // Wait one frame to ensure stimuli are visible
        yield return null; 

        // Reset waiting state and start the timer
        trialStartTime = Time.realtimeSinceStartup;
        waitingForResponse = true;

        // Wait for input
        while (waitingForResponse)
        {
            if (responseInput.GetResponse(out bool whichEye, out float inputTime))
            {
                currentTrial.whichEye = whichEye;
                currentTrial.responseTime = inputTime - trialStartTime;
                waitingForResponse = false;
            }

            yield return null; // Give unity the control back to update the next frame
        }

        // Log response time
        Debug.Log("Trial number: " + currentTrial.trialNumber + " | Response recorded in " + currentTrial.responseTime + " seconds.");

        // Clear stimuli so pause UI is the only thing visible
        stimulusManager.HideStimuli();

        // Return result to caller
        onFinished?.Invoke(currentTrial);
    }
}