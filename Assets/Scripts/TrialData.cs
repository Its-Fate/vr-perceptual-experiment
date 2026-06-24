using System.Collections.Generic;

[System.Serializable]
public class TrialData
{
    public int trialNumber;
    public TrialSpec spec; // So that we can know how the parameters were set
    public float startTime;
    public List<LogEntry> logEntries; // To record events during the trial

    [System.Serializable]
    public class LogEntry
    {
        public float time; // Time since the start of the trial
        public string state; // "up", "down", "left", "right", "none"
    }
}