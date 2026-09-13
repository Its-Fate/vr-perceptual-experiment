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
    private float trialDuration = 10f; // For now set to 10 (TODO: maybe 60 seconds later)

    // --- Initialize input method ---
    private IResponseInput responseInput;
    private void Awake()
    {
        // Using keyboard input for now (can be changed to other input methods later)
        responseInput = new KeyboardInput();
    }


    // Run a single trial (FlowController will call this and collect the TrialData)
    public IEnumerator RunTrial(int trialNumber, TrialSpec spec, int taskType, Action<TrialData> onFinished)
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

        // Task routing: task 1 and task 2 (holding)
        if (taskType == 1 || taskType == 2)
        {
            // Wait for the trial duration to end
            while (Time.realtimeSinceStartup - trialStartTime < trialDuration)
            {
                string currentState = responseInput.GetCurrentState();

                if (currentState != previousState)
                {
                    float timestamp = Time.realtimeSinceStartup - trialStartTime;
                    logEntries.Add(new TrialData.LogEntry {time = timestamp, state = currentState});
                    previousState = currentState;
                }

                yield return null; // Give unity the control back to update the next frame
            }
        }
        // Task routing: task 3 and task 4 (tapping)
        else if (taskType == 3 || taskType == 4)
        {
            // Wait for the trial duration to end
            while (Time.realtimeSinceStartup - trialStartTime < trialDuration)
            {
                string currentKeyDown = responseInput.GetCurrentKeyDown();

                if (currentKeyDown != "none")
                {
                    float timestamp = Time.realtimeSinceStartup - trialStartTime;
                    logEntries.Add(new TrialData.LogEntry {time = timestamp, state = currentKeyDown});
                }

                yield return null; // Give unity the control back to update the next frame
            }
        }

        // Hide stimuli
        stimulusManager.HideStimuli();

        // Double-click scrubbing: remove consecutive identical dominance logs (only for task 3 and task 4)
        if (taskType == 3 || taskType == 4)
        {
            logEntries = ScrubDoubleClicks(logEntries);
        }

        // Initialize a new TrialData
        TrialData data = new TrialData
        {
            trialNumber = trialNumber,
            spec = spec,
            startTime = trialStartTime,
            logEntries = logEntries,
            taskType = taskType
        };

        // Return result to caller
        onFinished?.Invoke(data);
    }

    private List<TrialData.LogEntry> ScrubDoubleClicks(List<TrialData.LogEntry> entries)
    {
        List<TrialData.LogEntry> scrubbed = new List<TrialData.LogEntry>();
        string lastState = "none";

        foreach (var entry in entries)
        {
            if (entry.state != lastState || entry.state == "piecemeal") // Keep piecemeal entries even if they are consecutive
            {
                scrubbed.Add(entry);
                lastState = entry.state;
            }
        }
        return scrubbed;
    }
}