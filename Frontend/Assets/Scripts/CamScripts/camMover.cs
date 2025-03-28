using System;
using TMPro.Examples;
using UnityEditor;
using UnityEditor.ShaderGraph;
using UnityEngine;

public class DroneViewCam : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float mouseSensitivity = 2f;

    //array of drones. Assignment of which stream goes to which drone yet to be determined
    //TO DO: Get Drone List from data manager
    public GameObject[] drones;

    //singleton of drone. used for viewing one particular drone in the scene
    //Maybe we can get away with this being private but im'm not sure. =V=
    //Depends on statistic genetation scripts which have yet to be written.
    public DroneMover drone;

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
            //No drone for moving Just no clip
            return;
        }
        drone = dataManager.GetSelectedDrone();
        //drone = dataManager.GetSelectedDrone();
        //Debug.Log("Mode num: " + mode + " Drone: " + dataManager.selectedDroneIndex);
       
        // if(Input.GetKeyDown(KeyCode.Tab))
        // {
        //     drone = drones[(Array.IndexOf(drones, drone) + 1) % numberOfDrones];
        // }
        // if(Input.GetKeyDown(KeyCode.Space))
        // {
        //     mode++;
        //     mode = mode % 3;
        // }
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

    //Needs refactoring. This is a mess
    private void noClip(){
        // Get the ControllerInput instance
        ControllerInput controllerInput = FindObjectOfType<ControllerInput>();
        if (controllerInput == null)
        {
            Debug.LogError("ControllerInput not found in the scene.");
            return;
        }

        // Get stick inputs
        Vector2 leftStick = controllerInput.LeftStickInput;  // Movement input
        Vector2 rightStick = controllerInput.RightStickInput; // Rotation input

        // Use left stick for movement
        Vector3 moveDirection = new Vector3(leftStick.x, 0, leftStick.y) * moveSpeed * Time.deltaTime;
        transform.Translate(moveDirection, Space.Self);

        // Use right stick for rotation
        float rotationX = -rightStick.y * mouseSensitivity; // Pitch (up/down)
        float rotationY = rightStick.x * mouseSensitivity;  // Yaw (left/right)
        transform.Rotate(rotationX, rotationY, 0, Space.Self);
    }
    private Vector3 findAngleFromCameraToDrone(){
        return new Vector3(drone.transform.position.x - transform.position.x, drone.transform.position.y - transform.position.y, drone.transform.position.z - transform.position.z);
    }
    private void thirdPerson(){
        
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
