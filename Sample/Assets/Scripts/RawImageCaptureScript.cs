using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Windows.WebCam;
using System.IO;
using System;
using System.Collections.Generic;
using System.Linq;

public class RawImageCaptureScript : MonoBehaviour
{
    private PhotoCapture photoCaptureObject = null;
    private CameraParameters cameraParameters;

 
    private void Start()
    {

    }

    public void CaptureImage()
    {
        Resolution[] supportedResolutions = PhotoCapture.SupportedResolutions.ToArray();
        Resolution cameraResolution = supportedResolutions.OrderByDescending((res) => res.width * res.height).First();

        PhotoCapture.CreateAsync(false, delegate (PhotoCapture captureObject)
        {
            photoCaptureObject = captureObject;

            // Store camera parameters
            cameraParameters = new CameraParameters();
            cameraParameters.hologramOpacity = 0.0f;
            cameraParameters.cameraResolutionWidth = cameraResolution.width;
            cameraParameters.cameraResolutionHeight = cameraResolution.height;
            cameraParameters.pixelFormat = CapturePixelFormat.BGRA32;

            captureObject.StartPhotoModeAsync(cameraParameters, delegate (PhotoCapture.PhotoCaptureResult result)
            {
                if (result.success)
                {
                    // Capture the image
                    photoCaptureObject.TakePhotoAsync(OnCapturedPhotoToMemory);
                }
                else
                {
                    Debug.LogError("Failed to start photo mode: " + result.hResult);
                }
            });
        });
    }

    private void OnCapturedPhotoToMemory(PhotoCapture.PhotoCaptureResult result, PhotoCaptureFrame photoCaptureFrame)
    {
        if (result.success)
        {
            // Get the raw image data
            List<byte> rawImageBytes = new List<byte>();
            photoCaptureFrame.CopyRawImageDataIntoBuffer(rawImageBytes);

            // Get the holographic data
            Matrix4x4 cameraToWorldMatrix;
            Matrix4x4 projectionMatrix;

            if (photoCaptureFrame.TryGetCameraToWorldMatrix(out cameraToWorldMatrix) &&
                photoCaptureFrame.TryGetProjectionMatrix(out projectionMatrix))
            {
                // Combine raw image and holographic data as needed

                // Save the combined data to a file with date and time in the filename
                string fileName = "CombinedCapturedData_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".dat";
                string filePath = Path.Combine(Application.persistentDataPath, fileName);

                using (FileStream fileStream = new FileStream(filePath, FileMode.Create))
                {
                    // Save raw image data
                    fileStream.Write(rawImageBytes.ToArray(), 0, rawImageBytes.Count);

                    // Save holographic data
                    WriteMatrixToFile(fileStream, cameraToWorldMatrix);
                    WriteMatrixToFile(fileStream, projectionMatrix);
                }

                Debug.Log("Combined data saved to: " + filePath);
            }
            else
            {
                Debug.LogError("Failed to get holographic data.");
            }
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

    private void OnStoppedPhotoMode(PhotoCapture.PhotoCaptureResult result)
    {
        // Release the PhotoCapture object
        photoCaptureObject.Dispose();
        photoCaptureObject = null;
    }

    private void WriteMatrixToFile(FileStream fileStream, Matrix4x4 matrix)
    {
        for (int i = 0; i < 16; i++)
        {
            byte[] floatBytes = BitConverter.GetBytes(matrix[i]);
            fileStream.Write(floatBytes, 0, floatBytes.Length);
        }
    }
}
