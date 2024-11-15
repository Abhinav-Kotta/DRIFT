using UnityEngine;

public class NoClipCam : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float ascendDescendSpeed = 5f;
    [SerializeField] private float mouseSensitivity = 2f;

    private Vector3 moveDirection;

    void Start()
    {
        // Lock the cursor for a better no-clip experience
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        // Get WASD input for movement along the XZ plane
        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");
        
        // Calculate forward/backward/left/right movement
        moveDirection = (transform.right * moveX + transform.forward * moveZ) * moveSpeed;

        // Ascend and descend with Q and E
        if (Input.GetKey(KeyCode.Q))
        {
            moveDirection += transform.up * ascendDescendSpeed;
        }
        if (Input.GetKey(KeyCode.E))
        {
            moveDirection -= transform.up * ascendDescendSpeed;
        }

        // Apply the movement
        transform.position += moveDirection * Time.deltaTime;

        // Handle mouse look
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        // Rotate the camera based on mouse input
        transform.Rotate(Vector3.up, mouseX, Space.World);
        transform.Rotate(Vector3.right, -mouseY, Space.Self);
    }
}
