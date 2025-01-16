using UnityEngine;
using System.Collections;

public class ScreenshotCapture : MonoBehaviour
{
    // Define a hotkey to trigger the screenshot
    public KeyCode screenshotKey = KeyCode.F12;

    // Set the file name for the screenshot
    public string fileName = "Screenshot";

    private void Update()
    {
        // Check if the user presses the designated screenshot key
        if (Input.GetKeyDown(screenshotKey))
        {
            CaptureScreenshot();
        }
    }

    void CaptureScreenshot()
    {
        // Create a timestamp for the screenshot filename to avoid overwriting
        string timestamp = System.DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
        string path = $"{Application.persistentDataPath}/{fileName}_{timestamp}.png";

        // Capture the screenshot
        ScreenCapture.CaptureScreenshot(path);

        // Output the screenshot path in the console
        Debug.Log("Screenshot saved to: " + path);
    }
}
