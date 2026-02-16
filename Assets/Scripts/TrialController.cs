using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;

public class TrialController : MonoBehaviour
{
    // --- Configurable fields ---
    [Header("Stimulus Images")]
    // To be assigned in Inspector
    public Sprite[] images; 

    [Header("UI Elements")]
    // To be assigned in Inspector
    public Image leftEyeDisplay;
    public Image rightEyeDisplay;

    // --- Internal state ---
    private TrialData currentTrial;
    private float trialStartTime;
    private bool waitingForResponse = false;

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
        if (images != null && images.Length > 0)
        {
            leftEyeDisplay.sprite = images[UnityEngine.Random.Range(0, images.Length)];
            rightEyeDisplay.sprite = images[UnityEngine.Random.Range(0, images.Length)];
        }

        // Show image and reset timer (ensure the UI has rendered first)
        leftEyeDisplay.gameObject.SetActive(true);
        rightEyeDisplay.gameObject.SetActive(true);
        Canvas.ForceUpdateCanvases();
        yield return null; // wait one frame to ensure stimuli are visible
        trialStartTime = Time.realtimeSinceStartup;
        waitingForResponse = true;

        // Wait for input
        while (waitingForResponse)
        {
            if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                OnRightEyeButtonPressed();
            }
            else if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                OnLeftEyeButtonPressed();
            }

            yield return null; // Give unity the control back to update the next frame
        }

        // Record response time
        currentTrial.responseTime = Time.realtimeSinceStartup - trialStartTime;
        Debug.Log("Trial number: " + currentTrial.trialNumber + " | Response recorded in " + currentTrial.responseTime + " seconds.");

        // Clear stimuli so pause UI is the only thing visible
        if (leftEyeDisplay != null)
            leftEyeDisplay.gameObject.SetActive(false);
        if (rightEyeDisplay != null)
            rightEyeDisplay.gameObject.SetActive(false);

        // Return result to caller
        onFinished?.Invoke(currentTrial);
    }

    public void OnLeftEyeButtonPressed()
    {
        if (waitingForResponse && currentTrial != null)
        {
            currentTrial.whichEye = false; // Reminder: false for left eye
            waitingForResponse = false;
        }
    }

    public void OnRightEyeButtonPressed()
    {
        if (waitingForResponse && currentTrial != null)
        {
            currentTrial.whichEye = true; // Reminder: true for right eye
            waitingForResponse = false;
        }
    }
}