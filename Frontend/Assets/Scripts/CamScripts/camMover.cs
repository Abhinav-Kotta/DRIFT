using System;
using TMPro.Examples;
using UnityEditor;
using UnityEditor.ShaderGraph;
using UnityEngine;

public class NoClipCam : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float ascendDescendSpeed = 5f;
    [SerializeField] private float mouseSensitivity = 2f;

    //array of drones. Assignment of which stream goes to which drone yet to be determined
    //TO DO: Get Drone List from data manager
    public GameObject[] drones;

    //singleton of drone. used for viewing one particular drone in the scene
    //Maybe we can get away with this being private but im'm not sure. =V=
    //Depends on statistic genetation scripts which have yet to be written.
    public GameObject drone;

    //used for cycling between drones
    public int numberOfDrones;

    private Vector3 moveDirection;

    private float followX;
    private float followY;
    private float followZ;
    private float followRoll;
    private float followPitch;
    private float followYaw;
    private float offsetX = 0;
    private float offsetY = 5;
    private float offsetZ = -10;
    private int mode;

    void Start()
    {   
        //initialize the drone to the first drone in the array
        drone = drones[0];
        numberOfDrones = drones.Length;

        Debug.Log("Num  of drones: " + numberOfDrones);

        mode = 2;
        // Lock the cursor for a better no-clip experience
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        //It works but might slow down the game. maybe refactor later
        for ( int i = 1; i <= 9; ++i )
        {
            if ( Input.GetKeyDown( "" + i ) )
            {
                if(i <= numberOfDrones)
                    drone = drones[i-1];
            }
        }
        if(Input.GetKeyDown(KeyCode.Tab))
        {
            drone = drones[(Array.IndexOf(drones, drone) + 1) % numberOfDrones];
        }
        if(Input.GetKeyDown(KeyCode.Space))
        {
            mode++;
            mode = mode % 3;
        }
        switch(mode)
        {
            case 0:
                firstPerson();
                break;
            case 1:
                thirdPerson();
                break;
            case 2:
                noClip();
                break;
        }
    
        
    }
    
    private void firstPerson(){
        followX = drone.transform.position.x;
        followY = drone.transform.position.y;
        //offset so that camera is slightly in front of drone
        followZ = drone.transform.position.z + 1.8f; 

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
    private Vector3 findAngleFromCameraToDrone(){
        return new Vector3(drone.transform.position.x - transform.position.x, drone.transform.position.y - transform.position.y, drone.transform.position.z - transform.position.z);
    }
    private void thirdPerson(){
        Vector3 temp = findAngleFromCameraToDrone();


        //dont ask me how this works. but it does. Please don't touch
        //make sure angle doesn't change wtih scrolling. So multiply the scroll delta by the normalized y/z component of the angle that way it doesn't change the angle
        offsetZ += (Input.mouseScrollDelta.y * temp.normalized.z);
        offsetY += (Input.mouseScrollDelta.y * temp.normalized.y);
        //make sure angle doesn't change with scrolling
        //Debug.Log("If this value changes from scrolling alone it's fucked -> Tan y/z is" + Math.Atan(offsetY/offsetZ));

        followX = drone.transform.position.x + offsetX;
        followY = drone.transform.position.y + offsetY;
        followZ = drone.transform.position.z + offsetZ;
        
        //make sure angle doesn't change with scrolling
        //Debug.Log("If this value changes from scrolling alone it's fucked -> Tan y/z is" + Math.Atan(offsetY/offsetZ));

        transform.position = new Vector3(followX, followY, followZ);
        transform.forward = temp.normalized;
    }

}
