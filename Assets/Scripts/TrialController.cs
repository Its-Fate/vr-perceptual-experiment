using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class TrialController : MonoBehaviour
{
    // --- Configurable fields ---
    public StimulusManager stimulusManager; // To be assigned in Inspector

    // --- Internal state ---
    private float trialStartTime;
    private float trialDuration = 5f; // For now set to 10, but can be altered later

    // --- Initialize input method ---
    private IResponseInput responseInput;
    private void Awake()
    {
        // Using keyboard input for now (can be changed to other input methods later)
        responseInput = new KeyboardInput();
    }


    // Run a single trial (FlowController will call this and collect the TrialData)
    public IEnumerator RunTrial(int trialNumber, TrialSpec spec, Action<TrialData> onFinished)
    {
        // Show different stimuli to each eye
        stimulusManager.SetTrialParameters(spec);
        stimulusManager.ShowStimuli();

        // Wait one frame to ensure stimuli are visible
        yield return null; 

        // Start the timer
        trialStartTime = Time.realtimeSinceStartup;

        // Initialize a new list of LogEntries
        List<TrialData.LogEntry> logEntries = new List<TrialData.LogEntry>();

        // Set previous state
        string previousState = "none";

        // Wait for the trial duration to end
        while (Time.realtimeSinceStartup - trialStartTime < trialDuration)
        {
            string currentState = GetCurrentStateFromInput();

            if (currentState != previousState)
            {
                float timestamp = Time.realtimeSinceStartup - trialStartTime;
                logEntries.Add(new TrialData.LogEntry {time = timestamp, state = currentState});
                previousState = currentState;
            }

            yield return null; // Give unity the control back to update the next frame
        }

        // Clear stimuli
        stimulusManager.HideStimuli();

        // Initialize a new TrialData
        TrialData data = new TrialData
        {
            trialNumber = trialNumber,
            spec = spec,
            startTime = trialStartTime,
            logEntries = logEntries
        };

        // Return result to caller
        onFinished?.Invoke(data);
    }

    // Current state can be "up", "down", "left", "right", and "none"
    private string GetCurrentStateFromInput()
    {
        return responseInput.GetCurrentState();
    }
}