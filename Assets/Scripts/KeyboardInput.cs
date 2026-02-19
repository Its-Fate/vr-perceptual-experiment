using UnityEngine;

public class KeyboardInput : IResponseInput
{
    public bool GetResponse(out bool whichEye, out float inputTime)
    {
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            whichEye = true;
         inputTime = Time.realtimeSinceStartup;
            return true;
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            whichEye = false;
         inputTime = Time.realtimeSinceStartup;
            return true;
        }

        // No response; return default values
        whichEye = false;
        inputTime = 0f;
        return false;
    }
}
