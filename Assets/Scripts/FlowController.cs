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

    [Range(1, 4)]
    public int taskType = 1; // Can be 1, 2, 3, or 4 (for now, default to 1)

    // UI text to show on completion
    public Text completionMessage; 

    private List<TrialData> allTrialData = new List<TrialData>();

    void Start()
    {
        StartCoroutine(RunExperiment());
    }

    private IEnumerator RunExperiment()
    {
        yield return null; // Wait one frame to ensure all references are set
        
        if (trialController == null)
        {
            Debug.LogError("TrialController reference not set on FlowController.");
            yield break;
        }

        allTrialData.Clear();
        
        //Generate all the different variations of trials
        List<TrialSpec> trialSpecs = GenerateTrialList();
        // TODO: int totalTrials = trialSpecs.Count;

        Debug.Log($"Starting experiment with {totalTrials} trials.");

        for (int i = 0; i < totalTrials; i++)
        {
            bool finished = false;
            // Run the trial and collect result via callback
            yield return StartCoroutine(trialController.RunTrial(i + 1, trialSpecs[i], this.taskType, (data) =>
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
                Debug.Log("Next trial in 5...");

                float restTimer = 5f;
                while (restTimer > 0)
                {
                    restTimer -= Time.deltaTime;
                    Debug.Log($"Next trial in {Mathf.CeilToInt(restTimer)}...");
                    yield return null;
                }

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

        // Contrast levels for the stimuli
        float[] contrastLevels = {0.1f, 0.3f, 0.5f, 0.7f, 0.9f};
        float baseContrast = 0.5f; // Base contrast for the other eye (Tasks 1, 2, 3)

        // Direction of movement vectors (0/90/45/-45 degrees)
        Vector2[,] movements = new Vector2[4, 2]
        {
            { new Vector2(0, 1), new Vector2(0, -1) },                          // Horizontal
            { new Vector2(1, 0), new Vector2(-1, 0) },                          // Vertical
            { new Vector2(-1, 1).normalized, new Vector2(1, -1).normalized },   // Right Diagonal
            { new Vector2(1, 1).normalized, new Vector2(-1, -1).normalized }    // Left Diagonal
        };
        
        // Orientation pairs
        int[,] conditions = new int[10, 2]
        {
            // Controls (4)
            { 0, 0 }, // H vs H
            { 1, 1 }, // V vs V
            { 2, 2 }, // RD vs RD
            { 3, 3 }, // LD vs LD
            
            // Rivalry (6)
            { 0, 1 }, // H vs V
            { 2, 3 }, // RD vs LD
            { 0, 2 }, // H vs RD
            { 0, 3 }, // H vs LD
            { 1, 2 }, // V vs RD
            { 1, 3 }  // V vs LD
        };

        // Repetition per condition (direction)
        // int repetitions = 15;

        for (int cond = 0; cond < conditions.GetLength(0); cond++)
        {
            int leftOrientation = conditions[cond, 0];
            int rightOrientation = conditions[cond, 1];

            foreach (float contrast in contrastLevels)
            {
                TrialSpec spec = new TrialSpec();

                // Movement directions per eye (which are perpendicular to the orientation)
                int leftMoveIdx = Random.Range(0, 2);
                int rightMoveIdx = Random.Range(0, 2);

                spec.directionL = movements[leftOrientation, leftMoveIdx];
                spec.directionR = movements[rightOrientation, rightMoveIdx];

                // Levelt contrast manipulation
                if (taskType == 1 || taskType == 2 || taskType == 3)
                {
                    // Unilateral: left fixed, right varies
                    spec.contrastL = baseContrast;
                    spec.contrastR = contrast;
                }
                else if (taskType == 4)
                {
                    // Bilateral: both vary together
                    spec.contrastL = contrast;
                    spec.contrastR = contrast;
                }

                spec.speedL = 5f;
                spec.speedR = 5f;
                spec.frequencyL = 10f;
                spec.frequencyR = 10f;
                spec.gaussianSharpnessL = 80f;
                spec.gaussianSharpnessR = 80f;
                spec.isControl = (spec.directionL == spec.directionR); // Control if both directions are the same

                trialList.Add(spec);
            }
        }
        
        

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
        try {File.WriteAllText(path, json);
        Debug.Log("Results saved to: " + path);}
        catch (System.Exception e) {Debug.LogError("Failed to save results: " + e.Message);}
    }

    private IEnumerator ShowCompletionAndClose()
    {
        // Hide the stimuli at the end of the experiment
        trialController.stimulusManager.leftStimulus.SetActive(false);
        trialController.stimulusManager.rightStimulus.SetActive(false);

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
