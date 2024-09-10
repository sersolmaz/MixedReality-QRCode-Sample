using UnityEngine;
using System.IO;

[System.Serializable]
public class BoundingBoxData
{
    public Vector3 boundingBoxPosition;
    public Vector3 boundingBoxRotationEulerAngles;
    public Vector3 boundingBoxScale;
    public Vector3 cameraPosition;
    public Vector3 cameraRotationEulerAngles;
}

public class SaveBoundingBox : MonoBehaviour
{
    public GameObject boundingBox; // Reference to your bounding box object
    public GameObject mainCamera; // Reference to your camera

    private int captureCounter = 0; // Initialize the counter
    private float captureInterval = 10f; // Interval in seconds
    private bool isFirstTime = true;

    private void Start()
    {
        // Don't start capturing immediately in Start
    }

    public void SaveBoundingBoxToFile()
    {
        if (isFirstTime)
        {
            isFirstTime = false;
            // Start capturing bounding box data at regular intervals
            InvokeRepeating("CaptureData", 0f, captureInterval);
        }
    }

    public void StopBoundingBoxCapture()
    {
        // Check if we're currently capturing data
        if (!isFirstTime)
        {
            // Cancel the repeating capture
            CancelInvoke("CaptureData");

            // Reset the flag to allow capturing to be started again
            isFirstTime = true;

            Debug.Log("Stopped bounding box data capture.");
        }
    }

    private void CaptureData()
    {
        BoundingBoxData data = new BoundingBoxData();

        // Populate the data
        data.boundingBoxPosition = boundingBox.transform.position;
        data.boundingBoxRotationEulerAngles = boundingBox.transform.rotation.eulerAngles;
        data.boundingBoxScale = boundingBox.transform.localScale;
        data.cameraPosition = mainCamera.transform.position;
        data.cameraRotationEulerAngles = mainCamera.transform.rotation.eulerAngles;

        // Convert data to JSON
        string jsonData = JsonUtility.ToJson(data);

        string filePath = GetFilePath(); // Define the path where you want to save the file

        // Write JSON data to file
        File.WriteAllText(filePath, jsonData);

        Debug.Log("Data saved to: " + filePath);
    }

    private string GetFilePath()
    {
        // Increment the image counter
        captureCounter++;

        // Get current date and time for a unique file name
        string timestamp = System.DateTime.Now.ToString("yyyyMMdd_HHmmss");

        // Combine the timestamp with the desired file name
        string fileName = captureCounter + "_PoseData_" + timestamp + ".json";

        // Combine the file name with the persistent data path
        return Path.Combine(Application.persistentDataPath, fileName);
    }
}
