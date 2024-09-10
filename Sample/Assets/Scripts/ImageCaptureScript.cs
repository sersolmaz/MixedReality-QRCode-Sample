using UnityEngine;
using UnityEngine.Windows.WebCam;
using System.IO;
using System;
using System.Linq;

public class ImageCaptureScript : MonoBehaviour
{
    private PhotoCapture photoCaptureObject = null;
    private CameraParameters cameraParameters;
    private Transform mainCameraTransform; // Reference to the main camera transform
    private int desiredWidth = 1920;
    private int desiredHeight = 1080;
    private float captureInterval = 10f; // Interval in seconds
    private bool isFirstTime = true;
    private int captureCounter = 0; // Initialize the counter

    private void Start()
    {
        // Assuming your main camera is tagged as "MainCamera"
        GameObject mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
        if (mainCamera != null)
        {
            mainCameraTransform = mainCamera.transform;
        }
        else
        {
            Debug.LogError("Main camera not found. Make sure it is tagged as 'MainCamera'.");
        }

        // Don't start capturing immediately in Start
    }

    public void CaptureImage()
    {
        if (isFirstTime)
        {
            isFirstTime = false;
            // Start capturing images at regular intervals
            InvokeRepeating("CaptureImageInternal", 0f, captureInterval);
        }
    }

    public void StopCaptureImage()
    {
        // Check if we're currently capturing images
        if (!isFirstTime)
        {
            // Cancel the repeating capture
            CancelInvoke("CaptureImageInternal");

            // Check if a PhotoCapture object exists
            if (photoCaptureObject != null)
            {
                // Stop photo mode and clean up
                photoCaptureObject.StopPhotoModeAsync(result =>
                {
                    photoCaptureObject.Dispose();
                    photoCaptureObject = null;
                    Debug.Log("Stopped and disposed photo capture.");
                });
            }

            // Reset the flag to allow capturing to be started again
            isFirstTime = true;
        }
    }

    private void CaptureImageInternal()
    {
        // Find the closest supported resolution
        Resolution[] supportedResolutions = PhotoCapture.SupportedResolutions.ToArray();
        Resolution closestResolution = supportedResolutions.OrderBy(res =>
            Mathf.Abs(res.width - desiredWidth) + Mathf.Abs(res.height - desiredHeight)
        ).First();

        // Store camera parameters with the desired resolution
        cameraParameters = new CameraParameters();
        cameraParameters.hologramOpacity = 0.0f;
        cameraParameters.cameraResolutionWidth = closestResolution.width;
        cameraParameters.cameraResolutionHeight = closestResolution.height;
        cameraParameters.pixelFormat = CapturePixelFormat.BGRA32;

        PhotoCapture.CreateAsync(false, delegate (PhotoCapture captureObject)
        {
            photoCaptureObject = captureObject;

            // Create a new CameraParameters object for photo capture with the desired resolution
            CameraParameters captureParameters = new CameraParameters();
            captureParameters.hologramOpacity = 0.0f;
            captureParameters.cameraResolutionWidth = desiredWidth;
            captureParameters.cameraResolutionHeight = desiredHeight;
            captureParameters.pixelFormat = CapturePixelFormat.BGRA32;

            captureObject.StartPhotoModeAsync(captureParameters, delegate (PhotoCapture.PhotoCaptureResult result)
            {
                if (result.success)
                {
                    // Capture the camera parameters
                    CameraParametersData parametersData = new CameraParametersData();
                    parametersData.focalLengthX = 1457.8501f;
                    parametersData.focalLengthY = 1459.032f;
                    parametersData.principalPointX = captureParameters.cameraResolutionWidth / 2;
                    parametersData.principalPointY = captureParameters.cameraResolutionHeight / 2;
                    parametersData.imageResolutionWidth = captureParameters.cameraResolutionWidth;
                    parametersData.imageResolutionHeight = captureParameters.cameraResolutionHeight;

                    // Capture the camera pose
                    if (mainCameraTransform != null)
                    {
                        parametersData.cameraPosePosition = mainCameraTransform.position;
                        parametersData.cameraPoseRotation = mainCameraTransform.rotation.eulerAngles;
                    }

                    // Save the camera parameters and pose to a file
                    SaveCameraParametersToFile(parametersData);

                    // Capture the image using the newly created CameraParameters object
                    photoCaptureObject.TakePhotoAsync((capturedResult, capturedFrame) =>
                    {
                        OnCapturedPhotoToMemory(capturedResult, capturedFrame, parametersData);
                    });
                }
                else
                {
                    Debug.LogError("Failed to start photo mode: " + result.hResult);
                }
            });
        });
    }

    private void SaveCameraParametersToFile(CameraParametersData parametersData)
    {
        // Define the path where you want to save the file
        string filePath = GetCameraParametersFilePath();

        using (StreamWriter writer = new StreamWriter(filePath))
        {
            // Write camera parameters
            writer.WriteLine("Focal Length X: " + parametersData.focalLengthX);
            writer.WriteLine("Focal Length Y: " + parametersData.focalLengthY);
            writer.WriteLine("Principal Point X: " + parametersData.principalPointX);
            writer.WriteLine("Principal Point Y: " + parametersData.principalPointY);
            writer.WriteLine("Image Resolution Width: " + parametersData.imageResolutionWidth);
            writer.WriteLine("Image Resolution Height: " + parametersData.imageResolutionHeight);
            // Write camera pose
            writer.WriteLine("Camera Pose Position: " + parametersData.cameraPosePosition);
            writer.WriteLine("Camera Pose Rotation: " + parametersData.cameraPoseRotation);
            writer.WriteLine("-----------");
        }

        Debug.Log("Camera parameters saved to: " + filePath);
    }

    private string GetCameraParametersFilePath()
    {
        // Increment the image counter
        captureCounter++;

        // Get current date and time for a unique file name
        string timestamp = System.DateTime.Now.ToString("yyyyMMdd_HHmmss");

        // Combine the timestamp with the desired file name
        string fileName = captureCounter + "_CameraParameters_" + timestamp + ".txt";

        // Combine the file name with the persistent data path
        return Path.Combine(Application.persistentDataPath, fileName);
    }

    private void OnCapturedPhotoToMemory(PhotoCapture.PhotoCaptureResult result, PhotoCaptureFrame photoCaptureFrame, CameraParametersData parametersData)
    {
        if (result.success)
        {
            // Create a new Texture2D with the desired resolution
            Texture2D texture = new Texture2D(desiredWidth, desiredHeight, TextureFormat.RGBA32, false);
            photoCaptureFrame.UploadImageDataToTexture(texture);

            // Example: Use focal length for perspective correction
            float focalLengthX = parametersData.focalLengthX;
            float focalLengthY = parametersData.focalLengthY;

            // Example: Perform perspective correction or other computations
            // For illustration purposes, assuming some hypothetical correction function
            Texture2D correctedTexture = PerspectiveCorrection(texture, focalLengthX, focalLengthY);

            // Encode the corrected texture to JPEG format
            byte[] jpegData = correctedTexture.EncodeToJPG();

            // Save the JPEG data to a file with date and time in the filename
            string fileName = captureCounter + "_CorrectedImage_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".jpg";
            string filePath = Path.Combine(Application.persistentDataPath, fileName);

            File.WriteAllBytes(filePath, jpegData);

            Debug.Log("Corrected image saved to: " + filePath);
        }
        else
        {
            Debug.LogError("Failed to capture photo: " + result.hResult);
        }

        // Dispose of the PhotoCaptureFrame
        photoCaptureFrame.Dispose();

        // Stop photo mode and release the PhotoCapture object
        photoCaptureObject.StopPhotoModeAsync(OnStoppedPhotoMode);
    }

    // Example hypothetical function for perspective correction
    private Texture2D PerspectiveCorrection(Texture2D inputTexture, float focalLengthX, float focalLengthY)
    {
        // Example implementation for perspective correction using focal length
        // Implement your specific correction algorithm here
        // This is just a placeholder to illustrate using focal length for correction

        // For simplicity, let's say we return the input texture as-is in this example
        return inputTexture;
    }

    private void OnStoppedPhotoMode(PhotoCapture.PhotoCaptureResult result)
    {
        // Release the PhotoCapture object
        photoCaptureObject.Dispose();
        photoCaptureObject = null;
    }

    [Serializable]
    public class CameraParametersData
    {
        public float focalLengthX;
        public float focalLengthY;
        public int principalPointX;
        public int principalPointY; // Change to int if it's supposed to be an integer
        public int imageResolutionWidth;
        public int imageResolutionHeight;
        public Vector3 cameraPosePosition;
        public Vector3 cameraPoseRotation;
        // Add more camera parameters as needed
    }
}
