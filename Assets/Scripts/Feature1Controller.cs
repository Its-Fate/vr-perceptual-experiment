using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.IO;

public class Feature1Controller : MonoBehaviour
{
    // --- Configurable fields ---
    [Header("Stimulus Images")]
    // To be assigned in Inspector
    public Sprite[] images; 

    [Header("UI Elements")]
    // To be assigned in Inspector
    public Image leftEyeDisplay;
    public Image rightEyeDisplay;

    [Header("Trial Data")]
    private List<TrialData> trials = new List<TrialData>();
    private TrialData currentTrial;
    private float trialStartTime;
    public int totalTrials = 10;

    // --- Internal state ---
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

    void Start()
    {
        StartCoroutine(StartTrial());
    }

    IEnumerator StartTrial()
    {
        //Initialize trial data
        currentTrial = new TrialData
        {
            trialNumber = trials.Count + 1,
            timestamp = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff")
        };
        
        // Show stimuli to each eye separately
        leftEyeDisplay.sprite = images[Random.Range(0, images.Length)];
        rightEyeDisplay.sprite = images[Random.Range(0, images.Length)];

        // Reset timer
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
        trials.Add(currentTrial);

        Debug.Log("Trial number: " + currentTrial.trialNumber + " | Response rescorded in " + currentTrial.responseTime + " seconds.");

        // Check if we have reached the end of the experiment
        if (trials.Count < totalTrials)
        {
            StartCoroutine(StartTrial());
        }
        else
        {
            SaveResults();
            Debug.Log("Experiment completed.");
        }
        
    }

    public void OnLeftEyeButtonPressed()
    {
        if (waitingForResponse)
        {
            currentTrial.whichEye = false; // Reminder: false for left eye
            waitingForResponse = false;
        }
    }

    public void OnRightEyeButtonPressed()
    {
        if (waitingForResponse)
        {
            currentTrial.whichEye = true; // Reminder: true for right eye
            waitingForResponse = false;
        }
    }

    // Save results to a JSON file
    private void SaveResults()
    {
        string path = Path.Combine(Application.persistentDataPath, "trial_results.json");
        string json = JsonConvert.SerializeObject(trials, Formatting.Indented);
        File.WriteAllText(path, json);
        Debug.Log("Results saved to: " + path);
    }
}