using UnityEngine;

public class StickToView : MonoBehaviour
{
    public Transform vrCamera; // Assign your VR camera (head) in the Inspector
    public float distance = 0.5f; // Distance from the camera
    public float heightOffset = -0.3f; // Adjust height to place at bottom of view
    public float smoothSpeed = 5f; // Adjust smoothness of movement

    void Update()
    {
        Vector3 targetPosition = vrCamera.position + vrCamera.forward * distance;
        targetPosition.y = vrCamera.position.y + heightOffset;
        
        transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * smoothSpeed);
        transform.LookAt(vrCamera);
        transform.Rotate(0, 180, 0); // Make sure it faces the user
    }
}
