using UnityEngine;

[System.Serializable]
public class TrialSpec
{
    // Left Eye Parameters
    public float speedL;
    public Vector2 directionL;
    public float contrastL;
    public float frequencyL;

    // Right Eye Parameters
    public float speedR;
    public Vector2 directionR;
    public float contrastR;
    public float frequencyR;

    // Control flag (if true, the parameters for both eyes will be identical)
    public bool isControl;
}