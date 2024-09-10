using UnityEngine;

public class staticRotation : MonoBehaviour
{
    private Quaternion initialRotation;

    void Start()
    {
        // Store the initial rotation of the GameObject
        initialRotation = transform.rotation;
    }

    void Update()
    {
        // Maintain the initial rotation in the X and Z directions
        Vector3 currentRotation = transform.rotation.eulerAngles;
        transform.rotation = Quaternion.Euler(new Vector3(initialRotation.eulerAngles.x, currentRotation.y, initialRotation.eulerAngles.z));
    }
}
