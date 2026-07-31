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
    public int totalTrials = 3; // For now set to 3, but can be altered later to the count of all the variation of specs generated
    // TODO: public int totalTrials = trialSpecs.Count; in line 40

    // UI text to show on completion
    public Text completionMessage; 
    // UI text to show between trials
    public Text pauseMessage;

    private List<TrialData> allTrialData = new List<TrialData>();

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

        allTrialData.Clear();
        
        //Generate all the different variations of trials
        List<TrialSpec> trialSpecs = GenerateTrialList();
        // TODO: int totalTrials = trialSpecs.Count;

        Debug.Log($"Starting expriment with {totalTrials} trials.");

        // Ensure pause UI is hidden at start
        if (pauseMessage != null)
            pauseMessage.gameObject.SetActive(false);

        for (int i = 0; i < totalTrials; i++)
        {
            bool finished = false;
            // Run the trial and collect result via callback
            yield return StartCoroutine(trialController.RunTrial(i + 1, trialSpecs[i], (data) =>
            {
                allTrialData.Add(data);
                finished = true;
            }));

            // ensure we wait until callback has been invoked
            while (!finished)
                yield return null;

            // If this is not the last trial, show pause message and wait for any key
            if (i < totalTrials - 1)
            {
                if (pauseMessage != null)
                {
                    pauseMessage.text = "Trial complete.\nPress any key to continue.";
                    pauseMessage.gameObject.SetActive(true);
                }

                while (!Input.anyKeyDown)
                    yield return null;

                // Consume the frame with the keydown and wait for release to avoid carryover
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

    // Generate a list of trial specifications for the experiment
    private List<TrialSpec> GenerateTrialList()
    {
        List<TrialSpec> trialList = new List<TrialSpec>();

        // 4 direction combination
        Vector2 left = new Vector2(-1, 0);
        Vector2 right = new Vector2(1, 0);
        
        Vector2[] leftDirections = {right, left, right, left};
        Vector2[] rightDirections = {right, left, left, right};

        // Repetition per condition (direction)
        int repetitions = 15;

        for (int rep = 0; rep < repetitions; rep++)
        {
            for (int cond = 0; cond < 4; cond++)
            {
                // Randomize speed, contrast, frequency, and Gaussian sharpness
                TrialSpec spec = new TrialSpec();

                // Left Eye
                spec.directionL = leftDirections[cond];
                spec.speedL = Random.Range(5f, 10f);
                spec.contrastL = Random.Range(0.8f, 1.2f);
                spec.frequencyL = Random.Range(30f, 50f);
                spec.gaussianSharpnessL = Random.Range(5f, 15f);

                // Right Eye
                spec.directionR = rightDirections[cond];
                spec.speedR = Random.Range(5f, 10f);
                spec.contrastR = Random.Range(0.8f, 1.2f);
                spec.frequencyR = Random.Range(30f, 50f);
                spec.gaussianSharpnessR = Random.Range(5f, 15f);

                spec.isControl = (leftDirections[cond] == rightDirections[cond]);

                trialList.Add(spec);
            }
        }
        

        // int controlNum = 4;
        // // Add control trials (both eyes have identical parameters)
        // for (int i = 0; i < controlNum; i++)
        // {
        //     // Choose parameters randomly
        //     TrialSpec control_spec = new TrialSpec();
        //     float s = speeds[Random.Range(0, speeds.Length)];
        //     Vector2 d = directions[Random.Range(0, directions.Length)];
        //     float c = contrasts[Random.Range(0, contrasts.Length)];
        //     float f = frequencies[Random.Range(0, frequencies.Length)];
            
        //     // Left eye parameters
        //     control_spec.speedL = s;
        //     control_spec.directionL = d;
        //     control_spec.contrastL = c;
        //     control_spec.frequencyL = f;

        //     // Right eye parameters
        //     control_spec.speedR = s;
        //     control_spec.directionR = d;
        //     control_spec.contrastR = c;
        //     control_spec.frequencyR = f;

        //     trialList.Add(control_spec);
        // }

        // Shuffle the generated list of trial specs (Fisher-Yates algorithm was used)
        for (int i = 0; i < trialList.Count; i++)
        {
            int rand = Random.Range(i, trialList.Count);
            TrialSpec temp = trialList[i];
            trialList[i] = trialList[rand];
            trialList[rand] = temp;
        }

        return trialList;
    }
    
    

    private void SaveResults()
    {
        string path = Path.Combine(Application.persistentDataPath, "trial_results.json");
        var settings = new JsonSerializerSettings
        {
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            Formatting = Formatting.Indented
        };
        string json = JsonConvert.SerializeObject(allTrialData, settings);
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

        // Consume the keydown frame and wait for release
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
