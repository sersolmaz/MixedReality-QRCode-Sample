using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class originBoundingBox : MonoBehaviour
{
    void Update()
    {
        GameObject cube = GameObject.Find("Cube"); // Find the GameObject named "Cube"

        if (cube != null) // Check if the "Cube" GameObject is found
        {
            // Copy the position of the "Cube" GameObject to this object's position
            transform.position = cube.transform.position;
        }
        else
        {
            Debug.LogWarning("GameObject named 'Cube' not found!"); // Log a warning if "Cube" GameObject is not found
        }
    }
}
