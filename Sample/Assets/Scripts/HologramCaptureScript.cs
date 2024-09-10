using UnityEngine;
using System.IO;
using UnityEngine.UI; 

public class HologramCaptureScript : MonoBehaviour
{
    public Camera hologramCamera; // Assign the camera that renders your holograms and 3D data
    private int captureWidth = 1920;
    private int captureHeight = 1080;
    private float captureInterval = 10f; // Interval in seconds
    private bool isCapturing = false;
    private int imageCounter = 0; // Initialize the counter

    public Text countdownText; // Reference to your TextMeshPro text component
    private float timeUntilNextCapture; // Time until next capture

    public float focalLength = 1460f; // Focal length of the camera

    private void Start()
    {
        // Optionally, start capturing immediately or wait for an external trigger
        // StartCapture();
    }

    public void StartCapture()
    {
        if (!isCapturing)
        {
            isCapturing = true;
            timeUntilNextCapture = captureInterval; // Initialize the countdown timer
            InvokeRepeating(nameof(CaptureHologramAnd3DData), 2f, captureInterval);
            InvokeRepeating(nameof(UpdateCountdown), 1f, 1f); // Update the countdown every second

        }
    }

    public void StopCapture()
    {
        if (isCapturing)
        {
            isCapturing = false;
            CancelInvoke(nameof(CaptureHologramAnd3DData));
            CancelInvoke(nameof(UpdateCountdown)); // Stop updating the countdown
        }
    }

    private void UpdateCountdown()
    {
        timeUntilNextCapture -= 1f; // Decrease the countdown timer
        if (countdownText != null)
            countdownText.text = $"{timeUntilNextCapture}s";

        if (timeUntilNextCapture <= 0)
        {
            timeUntilNextCapture = captureInterval; // Reset the countdown timer
        }
    }

    private void CaptureHologramAnd3DData()
    {
        RenderTexture renderTexture = new RenderTexture(captureWidth, captureHeight, 24);
        hologramCamera.targetTexture = renderTexture;
        Texture2D screenShot = new Texture2D(captureWidth, captureHeight, TextureFormat.RGBA32, false);

        // Setting the camera's focal length
        hologramCamera.focalLength = focalLength;

        hologramCamera.Render();
        RenderTexture.active = renderTexture;
        screenShot.ReadPixels(new Rect(0, 0, captureWidth, captureHeight), 0, 0);
        screenShot.Apply();

        // Reset the camera and RenderTexture
        hologramCamera.targetTexture = null;
        RenderTexture.active = null;
        Destroy(renderTexture);

        byte[] bytes = screenShot.EncodeToPNG();
        Destroy(screenShot);

        // Increment the image counter
        imageCounter++;

        // Save the captured image with numbering, focal length, and formatted date-time
        string datetime = System.DateTime.Now.ToString("yyyyMMdd_HHmmss");
        string filename = $"{imageCounter}_FL{focalLength}_HologramImage_{datetime}.png";
        string filepath = Path.Combine(Application.persistentDataPath, filename);
        File.WriteAllBytes(filepath, bytes);

        Debug.Log($"Saved hologram and 3D data capture to: {filepath}");
    }
}
