using UnityEngine;

public class NoClipCam : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float ascendDescendSpeed = 5f;
    [SerializeField] private float mouseSensitivity = 2f;

    public GameObject drone;

    private Vector3 moveDirection;

    private float followX;
    private float followY;
    private float followZ;
    private float followRoll;
    private float followPitch;
    private float followYaw;

    bool isFollowing = false;

    void Start()
    {
        // Lock the cursor for a better no-clip experience
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Space))
        {
            isFollowing = !isFollowing;
        }
        if(isFollowing)
        {
            followDrone();
        }
        else
        {
            noClip();
        }
    
        
    }
    
    private void followDrone(){
        followX = drone.transform.position.x;
        followY = drone.transform.position.y;
        followZ = drone.transform.position.z;

        followRoll = drone.transform.rotation.eulerAngles.x;
        followPitch = drone.transform.rotation.eulerAngles.y;
        followYaw = drone.transform.rotation.eulerAngles.z;

        transform.rotation = Quaternion.Euler(followRoll, followPitch, followYaw);
        transform.position = new Vector3(followX, followY, followZ);
    }
    private void noClip(){
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
        transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y, 0);
    }
}
