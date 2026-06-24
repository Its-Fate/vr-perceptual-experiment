using UnityEngine;

public class KeyboardInput : IResponseInput
{
    public string GetCurrentState()
    {
        if (Input.GetKey(KeyCode.UpArrow)) return "up";
        if (Input.GetKey(KeyCode.DownArrow)) return "down";
        if (Input.GetKey(KeyCode.LeftArrow)) return "left";
        if (Input.GetKey(KeyCode.RightArrow)) return "right";
        return "none";
    }
}
