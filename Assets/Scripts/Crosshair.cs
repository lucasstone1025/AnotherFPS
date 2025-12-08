using UnityEngine;

public class Crosshair : MonoBehaviour
{
    public Color crosshairColor = Color.white;
    public float crosshairSize = 10f;
    public float crosshairThickness = 2f;
    public bool showDot = true;
    public float dotSize = 4f;

    private Texture2D crosshairTexture;

    void Start()
    {
        // Create a simple white texture
        crosshairTexture = new Texture2D(1, 1);
        crosshairTexture.SetPixel(0, 0, Color.white);
        crosshairTexture.Apply();
    }

    void OnGUI()
    {
        // Get screen center
        float centerX = Screen.width / 2f;
        float centerY = Screen.height / 2f;

        // Set color
        GUI.color = crosshairColor;

        if (showDot)
        {
            // Draw center dot
            GUI.DrawTexture(new Rect(centerX - dotSize / 2f, centerY - dotSize / 2f, dotSize, dotSize), crosshairTexture);
        }
        else
        {
            // Draw crosshair lines
            // Horizontal line (left)
            GUI.DrawTexture(new Rect(centerX - crosshairSize - crosshairThickness, centerY - crosshairThickness / 2f, crosshairSize, crosshairThickness), crosshairTexture);
            // Horizontal line (right)
            GUI.DrawTexture(new Rect(centerX + crosshairThickness, centerY - crosshairThickness / 2f, crosshairSize, crosshairThickness), crosshairTexture);
            // Vertical line (top)
            GUI.DrawTexture(new Rect(centerX - crosshairThickness / 2f, centerY - crosshairSize - crosshairThickness, crosshairThickness, crosshairSize), crosshairTexture);
            // Vertical line (bottom)
            GUI.DrawTexture(new Rect(centerX - crosshairThickness / 2f, centerY + crosshairThickness, crosshairThickness, crosshairSize), crosshairTexture);
        }

        // Reset color
        GUI.color = Color.white;
    }
}
