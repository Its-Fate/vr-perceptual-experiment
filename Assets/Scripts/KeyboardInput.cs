using UnityEngine;

public class KeyboardInput : IResponseInput
{
    public string GetCurrentState() // Holding (Tasks 1 and 2)
    {
        // Horizontal lines -> Up/Down movement
        if (Input.GetKey(KeyCode.W)) return "up";
        if (Input.GetKey(KeyCode.S)) return "down";

        // Vertical lines -> Left/Right movement
        if (Input.GetKey(KeyCode.A)) return "left";
        if (Input.GetKey(KeyCode.D)) return "right";

        // Right Diagonal lines -> Up-Left/Down-Right movement
        if (Input.GetKey(KeyCode.E)) return "up-left";
        if (Input.GetKey(KeyCode.X)) return "down-right";

        // Left Diagonal lines -> Up-Right/Down-Left movement
        if (Input.GetKey(KeyCode.Q)) return "up-right";
        if (Input.GetKey(KeyCode.Z)) return "down-left";

        // Piecemeal
        if (Input.GetKey(KeyCode.Space)) return "piecemeal";
        return "none";
    }

    public string GetCurrentKeyDown() // Tapping (Tasks 3 and 4)
    {
        // Horizontal lines -> Up/Down movement
        if (Input.GetKeyDown(KeyCode.W)) return "up";
        if (Input.GetKeyDown(KeyCode.S)) return "down";

        // Vertical lines -> Left/Right movement
        if (Input.GetKeyDown(KeyCode.A)) return "left";
        if (Input.GetKeyDown(KeyCode.D)) return "right";

        // Right Diagonal lines -> Up-Left/Down-Right movement
        if (Input.GetKeyDown(KeyCode.E)) return "up-left";
        if (Input.GetKeyDown(KeyCode.X)) return "down-right";

        // Left Diagonal lines -> Up-Right/Down-Left movement
        if (Input.GetKeyDown(KeyCode.Q)) return "up-right";
        if (Input.GetKeyDown(KeyCode.Z)) return "down-left";

        // Piecemeal
        if (Input.GetKeyDown(KeyCode.Space)) return "piecemeal";
        return "none";
    }
}
