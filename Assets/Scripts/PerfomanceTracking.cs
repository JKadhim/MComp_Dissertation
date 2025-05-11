using UnityEngine;
using TMPro;

public class PerformanceTracking : MonoBehaviour
{
    public TextMeshProUGUI fpsDisplay; // Reference to a TextMeshProUGUI element to display the FPS
    public float updateInterval = 1f; // Time interval to update the FPS display
    public float startDelay = 3f; // Delay in seconds before starting FPS tracking

    private int frameCount = 0;
    private float elapsedTime = 0f;

    private float minFps = 300;
    private float maxFps = 300;

    private bool isTracking = false; // Flag to control when FPS tracking starts

    private void Start()
    {
        // Start the coroutine to delay FPS tracking
        StartCoroutine(DelayStartTracking());
    }

    private void Update()
    {
        if (!isTracking) return; // Skip FPS tracking if the delay hasn't passed

        frameCount++;
        elapsedTime += Time.deltaTime;

        if (elapsedTime >= updateInterval)
        {
            // Calculate average FPS
            float fps = frameCount / elapsedTime;

            // Update min and max FPS
            if (fps < minFps)
            {
                minFps = fps;
            }
            if (fps > maxFps)
            {
                maxFps = fps;
            }

            // Update the TextMeshProUGUI element
            if (fpsDisplay != null)
            {
                fpsDisplay.text = $"AVG FPS: {fps:F2}";
            }

            // Reset counters
            frameCount = 0;
            elapsedTime = 0f;
        }
    }

    private void OnApplicationQuit()
    {
        // Code to execute when the application is quitting
        Debug.Log($"Application is quitting. Min FPS: {minFps:F2}, Max FPS: {maxFps:F2}");
    }

    private System.Collections.IEnumerator DelayStartTracking()
    {
        // Wait for the specified delay
        yield return new WaitForSeconds(startDelay);

        // Enable FPS tracking
        isTracking = true;
    }
}

