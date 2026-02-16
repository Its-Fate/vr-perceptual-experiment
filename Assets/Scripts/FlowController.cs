using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.IO;

public class FlowController : MonoBehaviour
{
    [Header("References")]
    // To be assigned in Inspector
    public TrialController trialController;
    public int totalTrials = 10;
    // UI text to show on completion
    public Text completionMessage; 
    // UI text to show between trials
    public Text pauseMessage;

    private List<TrialController.TrialData> trials = new List<TrialController.TrialData>();

    void Start()
    {
        StartCoroutine(RunExperiment());
    }

    private IEnumerator RunExperiment()
    {
        if (trialController == null)
        {
            Debug.LogError("TrialController reference not set on FlowController.");
            yield break;
        }

        trials.Clear();

        // Ensure pause UI is hidden at start
        if (pauseMessage != null)
            pauseMessage.gameObject.SetActive(false);

        for (int i = 1; i <= totalTrials; i++)
        {
            bool finished = false;
            // Run the trial and collect result via callback
            yield return StartCoroutine(trialController.RunTrial(i, (data) =>
            {
                trials.Add(data);
                finished = true;
            }));

            // ensure we wait until callback has been invoked
            while (!finished)
                yield return null;

            // If this is not the last trial, show pause message and wait for any key
            if (i < totalTrials)
            {
                if (pauseMessage != null)
                {
                    pauseMessage.text = "Trial complete.\nPress any key to continue.";
                    pauseMessage.gameObject.SetActive(true);
                }

                while (!Input.anyKeyDown)
                    yield return null;

                // consume the frame with the keydown and wait for release to avoid carryover
                yield return null;
                while (Input.anyKey)
                    yield return null;

                if (pauseMessage != null)
                    pauseMessage.gameObject.SetActive(false);

                // give one frame for UI to update before next trial
                yield return null;
            }
        }

        SaveResults();
        Debug.Log("Experiment completed.");

        StartCoroutine(ShowCompletionAndClose());
    }

    private void SaveResults()
    {
        string path = Path.Combine(Application.persistentDataPath, "trial_results.json");
        string json = JsonConvert.SerializeObject(trials, Formatting.Indented);
        File.WriteAllText(path, json);
        Debug.Log("Results saved to: " + path);
    }

    private IEnumerator ShowCompletionAndClose()
    {
        // Display completion message if UI element exists
        if (completionMessage != null)
        {
            completionMessage.text = "Experiment Complete!\n\nPress any key to exit.";
            completionMessage.gameObject.SetActive(true);
        }

        // Wait for any input
        while (!Input.anyKeyDown)
            yield return null;

        // consume the keydown frame and wait for release
        yield return null;
        while (Input.anyKey)
            yield return null;

        // Close the application (or stop play in editor)
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
}
