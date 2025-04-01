using System;
using TMPro.Examples;
using UnityEditor;
using UnityEditor.ShaderGraph;
using UnityEngine;

public class DroneViewCam : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 50f;
    [SerializeField] private float rotationSpeed = 8.0f;
    [SerializeField] private Camera cam; // Reference to the XR camera

    public GameObject[] drones;

    //singleton of drone. used for viewing one particular drone in the scene
    //Maybe we can get away with this being private but im'm not sure. =V=
    //Depends on statistic genetation scripts which have yet to be written.
    public DroneMover drone;

    //used for cycling between drones
    public int numberOfDrones;

    
    //DashBoard Toggles
    public GameObject gauges;
    public GameObject LeftStickView;
    public GameObject RightStickView;
    public Canvas popupPanel;


    private float followX;
    private float followY;
    private float followZ;
    private float followRoll;
    private float followPitch;
    private float followYaw;
    private float offsetX = 0;
    private float offsetY = 5;
    private float offsetZ = -10;

    [SerializeField]
    private int mode;

    private DataManager dataManager;

    void Start()
    {   
        mode = 2;
        dataManager = DataManager.Instance;
        if(dataManager == null)
            Debug.Log("DataManager not found.");
        //drone = DataManager.Instance.GetSelectedDrone();
        //Default to noCLip since there can be no drones at the start of a race
    }

    void Update()
    {
        if(dataManager == null)
        {
            dataManager = DataManager.Instance;
            return;
        }
        if(dataManager.GetNumActiveDrones() == 0)
        {
            //noClip();
        }
        else{
            drone = dataManager.GetSelectedDrone();
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
    
    private void firstPerson()
    {
        //Set Gauges and stick to be active in first person mode
        if(gauges != null)
            gauges.SetActive(true); // Turn off the gauges in no-clip mode
        if(LeftStickView != null && RightStickView != null)
        {
            LeftStickView.SetActive(true); // Turn off the left stick view in no-clip mode
            RightStickView.SetActive(true); // Turn off the right stick view in no-clip mode
        }
        drone = dataManager.GetSelectedDrone();
        if (drone == null)
        {
            Debug.LogWarning("No drone selected.");
            return;
        }
        // Follow the drone's position
        followX = drone.transform.position.x;
        followY = drone.transform.position.y;
        followZ = drone.transform.position.z+2; // Offset so the camera is slightly in front of the drone

        // Get the drone's rotation
        followRoll = drone.transform.rotation.eulerAngles.x;
        followPitch = drone.transform.rotation.eulerAngles.y;
        followYaw = drone.transform.rotation.eulerAngles.z;

        // Reduce the effect of roll (left/right tilt) by dividing it by 2
        followRoll /= 2;

        // Apply the adjusted rotation and position to the rig
        //transform.rotation = Quaternion.Euler(followRoll, followPitch, followYaw);
        transform.position = new Vector3(followX, followY, followZ);
    }

    //Needs refactoring. This is a mess
    private void noClip()
    {
        //Set gauges and stick to be inactive in no-clip mode
        if(gauges != null)
            gauges.SetActive(false); // Turn off the gauges in no-clip mode
        if(LeftStickView != null && RightStickView != null)
        {
            LeftStickView.SetActive(false); // Turn off the left stick view in no-clip mode
            RightStickView.SetActive(false); // Turn off the right stick view in no-clip mode
        }

        // Ensure the ControllerInput instance exists
        if (ControllerInput.Instance == null)
        {
            Debug.LogError("ControllerInput instance is null. Ensure ControllerInput is in the scene.");
            return;
        }

        // Ensure the camera reference exists
        if (cam == null)
        {
            Debug.LogError("Camera reference is missing. Ensure the camera is assigned.");
            return;
        }

        // Get stick inputs
        Vector2 leftStick = ControllerInput.Instance.LeftStickInput;  // Movement input
        Vector2 rightStick = ControllerInput.Instance.RightStickInput; // Rotation input


        // Use the camera's forward and right vectors for movement, ignoring the Y component
        Vector3 forward = cam.transform.forward;
                forward.Normalize();

        Vector3 right = cam.transform.right;
                right.Normalize();

        // Calculate movement direction based on left stick input
        Vector3 moveDirection = (forward * leftStick.y + right * leftStick.x) * moveSpeed * Time.deltaTime; // Increased movement sensitivity by 10
        transform.position += moveDirection;

        // Use the right stick to rotate the rig around the Y-axis (yaw)
        float rotationY = rightStick.x * moveSpeed * rotationSpeed * Time.deltaTime; // Increased rotation sensitivity by 5
        transform.Rotate(0, rotationY, 0, Space.World);
    }
    private Vector3 findAngleFromCameraToDrone(){
        return new Vector3(drone.transform.position.x - transform.position.x, drone.transform.position.y - transform.position.y, drone.transform.position.z - transform.position.z);
    }
    private void thirdPerson()
    {
        //Set Gauges and stick to be active in third person mode
        if(gauges != null)
            gauges.SetActive(true); // Turn off the gauges in no-clip mode
        if(LeftStickView != null && RightStickView != null)
        {
            LeftStickView.SetActive(true); // Turn off the left stick view in no-clip mode
            RightStickView.SetActive(true); // Turn off the right stick view in no-clip mode
        }
        // Define the offset behind the drone
        Vector3 offset = new Vector3(0, offsetY, offsetZ);

        if (drone == null)
        {
            Debug.LogWarning("No drone selected.");
            return;
        }
        // Calculate the camera's position relative to the drone
        Vector3 desiredPosition = drone.transform.position + drone.transform.rotation * offset;

        // Set the camera's position
        transform.position = desiredPosition;

        // Make the camera look in the same direction as the drone
        transform.rotation = Quaternion.Euler(0, drone.transform.rotation.eulerAngles.y, 0);
    }

    public int GetCurrentMode()
    {
        return mode;
    }
    public int cycleMode()
    {
        mode++;
        mode = mode % 3;
        return mode;
    }
    
}
