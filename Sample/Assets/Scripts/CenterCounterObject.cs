using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CenterCounterObject : MonoBehaviour
{
    public Camera targetCamera; // The camera to center this object on
    public Vector3 offset; // Offset from the center, if needed

    void LateUpdate()
    {
        if (targetCamera != null)
        {
            // Update position
            Vector3 centerPoint = targetCamera.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, targetCamera.nearClipPlane + 10)); // Adjust 10 based on scene scale
            transform.position = centerPoint + offset;

            // Update rotation to match the camera's rotation
            transform.rotation = targetCamera.transform.rotation;
        }
    }
}
