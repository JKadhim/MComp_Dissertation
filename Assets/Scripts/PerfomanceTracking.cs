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

    // Cumulative values for lifetime average FPS
    private int totalFrameCount = 0;
    private float totalElapsedTime = 0f;

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

        // Update cumulative values
        totalFrameCount++;
        totalElapsedTime += Time.deltaTime;

        if (elapsedTime >= updateInterval)
        {
            // Calculate average FPS for the current interval
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

            // Calculate lifetime average FPS
            float lifetimeAverageFps = totalFrameCount / totalElapsedTime;

            // Update the TextMeshProUGUI element
            if (fpsDisplay != null)
            {
                fpsDisplay.text = $"AVG FPS: {lifetimeAverageFps:F0}";
            }

            // Reset interval counters
            frameCount = 0;
            elapsedTime = 0f;
        }
    }

    private void OnApplicationQuit()
    {
        // Log min, max, and lifetime average FPS
        float lifetimeAverageFps = totalFrameCount / totalElapsedTime;
        Debug.Log($"Application is quitting. Min FPS: {minFps:F0}, Max FPS: {maxFps:F0}, Lifetime AVG FPS: {lifetimeAverageFps:F0}");
    }

    private System.Collections.IEnumerator DelayStartTracking()
    {
        // Wait for the specified delay
        yield return new WaitForSeconds(startDelay);

        // Enable FPS tracking
        isTracking = true;
    }
}

