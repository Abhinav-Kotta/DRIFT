using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class DroneMover : MonoBehaviour
{
    [SerializeField] public String Name = "Drone";

    private Vector3 lastPosition;
    private Vector3 currentPosition;
    private float timeSinceLastUpdate = 0f;
    private float velocityUpdateInterval = 0.1f;
    private float calculatedSpeed = 0f;    
    private int numGates; 

    // Can be used for testing for movement and rotation
    [SerializeField] bool artificialMovement = true;
    [SerializeField] bool artificialRotation = true;

    // Artificial speed of the drone for testing
    [SerializeField] [Range(0,20)] private float artificalSpeed = 10f;

    // Testing purposes. Used to simulate movement rolling and pitching behavior
    public float angle = 0f;


    public string DID { get; private set; } // Drone ID. Used for filter through total data for movement.
    // Initial position of the drone, used only for simulating movement
    private float initX;
    private float initY;
    private float initZ;

    // Position of drone
    [SerializeField] float x;
    [SerializeField] float y;
    [SerializeField] float z;

    // Angle of drone
    [SerializeField] float roll;
    [SerializeField] float pitch;
    [SerializeField] float yaw;

    // Velocity of drone
    [SerializeField] Vector3 velocity;

    // Gyro data
    [SerializeField] float gyroPitch;
    [SerializeField] float gyroRoll;
    [SerializeField] float gyroYaw;

    // Inputs
    [SerializeField] float throttleInput;
    [SerializeField] float yawInput;
    [SerializeField] float pitchInput;
    [SerializeField] float rollInput;

    // Battery data
    [SerializeField] float batteryPercentage;
    [SerializeField] float batteryVoltage;

    // Motor data
    [SerializeField] int motorCount;
    [SerializeField] float[] motorRpms;

    // Used for rotating propeller. Measured in rotations per minute??
    [SerializeField] [Range(0, 5000)] int motorSpeed1 = 0;  //Measured in rotations per minute??
    [SerializeField] [Range(0, 5000)] int motorSpeed2 = 0;  
    [SerializeField] [Range(0, 5000)] int motorSpeed3 = 0;
    [SerializeField] [Range(0, 5000)] int motorSpeed4 = 0;


    public GameObject propeller1;
    public GameObject propeller2;
    public GameObject propeller3;
    public GameObject propeller4;

    void Start()
    {
        lastPosition = transform.position;
        currentPosition = lastPosition;
        initX = gameObject.transform.position.x;
        initY = gameObject.transform.position.y;
        initZ = gameObject.transform.position.z;

        y = initY;

    }

    void Update()
    {
        if(artificialMovement)
            SimulateMovement();
        //Comment out for testing
        transform.position = getPosition();
        //transform.rotation = getRotation();
    }

    public Vector3 getPosition()
    {
        return new Vector3(x, y, z);
    }
    //needs testing. Will also need scaling for when we figure that out
    // public float getSpeed()
    // {
    //     Debug.Log("Velocity: " + velocity.magnitude);
    //     return velocity.magnitude;
    // }
    public float CalculateVelocityMagnitude()
    {
        float speed = velocity.magnitude;
        return speed;
    }

    public float GetCalculatedSpeed()
    {
        return calculatedSpeed;
    }
    public Quaternion getRotation()
    {
        return Quaternion.Euler(pitch, yaw, roll);
    }

    public void setMovement(float x, float y, float z)
    {
        // Set the position of the drone
        this.x = x *  4.9117f;
        this.y = y * 5.0f;
        this.z = z * 4.9109f;
    }

    public void setRotation(float x, float y, float z, float w)
    {
        Quaternion rotation = new Quaternion(x, y, z, w);
        transform.rotation = rotation;
    }

    public void SetDroneData(DroneData droneData)
    {
        this.DID = droneData.drone_id;
        //Stuff that is reflected
        setMovement(droneData.position.x, droneData.position.y, droneData.position.z);
        setRotation(droneData.attitude.x, droneData.attitude.y, droneData.attitude.z, droneData.attitude.w);
        setProps(droneData.motor_rpms); //needs testing. Should work. may throw type exception if anything. edit function decl below.
        this.velocity = new Vector3(droneData.velocity.x, droneData.velocity.y, droneData.velocity.z);
        //Debug.Log($"velocity: {this.velocity}");
        this.gyroPitch = droneData.gyro.pitch;
        this.gyroRoll = droneData.gyro.roll;
        this.gyroYaw = droneData.gyro.yaw;
        this.throttleInput = droneData.inputs.throttle;
        this.yawInput = droneData.inputs.yaw;
        this.pitchInput = droneData.inputs.pitch;
        this.rollInput = droneData.inputs.roll;
        this.batteryPercentage = droneData.battery.percentage;
        this.batteryVoltage = droneData.battery.voltage;
        this.motorCount = droneData.motor_count;
        this.motorRpms = droneData.motor_rpms;
    }

    public void setProps(float[] rotationStats)
    {
        // Check if motorRpms is null or has fewer than 4 elements
        if (rotationStats == null || rotationStats.Length < 4)
        {
            Debug.LogError("Invalid motor RPM data. Ensure the array is not null and has at least 4 elements.");
            return;
        }

        // Assign motor RPMs to the local array
        motorRpms = rotationStats;

        // Extract motor RPM values
        float motor1 = motorRpms[0];
        float motor2 = motorRpms[1];
        float motor3 = motorRpms[2];
        float motor4 = motorRpms[3];

        // Rotate the propellers based on motor RPMs
        if (propeller1 != null)
            propeller1.transform.Rotate(0, 0, motor1 * Time.deltaTime);

        if (propeller2 != null)
            propeller2.transform.Rotate(0, 0, motor2 * Time.deltaTime);

        if (propeller3 != null)
            propeller3.transform.Rotate(0, 0, -motor3 * Time.deltaTime); // Negative for counter-rotation
        if (propeller4 != null)
            propeller4.transform.Rotate(0, 0, -motor4 * Time.deltaTime); // Negative for counter-rotation
    }

    // Getters

    //Lambdas for OOP naming bs
    public float Pitch => pitch;
    public float Roll => roll;
    public float Yaw => yaw;
    public Vector3 Velocity => velocity;
    public float GyroPitch => gyroPitch;
    public float GyroRoll => gyroRoll;
    public float GyroYaw => gyroYaw;
    public float ThrottleInput => throttleInput;
    public float YawInput => yawInput;
    public float PitchInput => pitchInput;
    public float RollInput => rollInput;
    public float BatteryPercentage => batteryPercentage;
    public float BatteryVoltage => batteryVoltage;
    public int MotorCount => motorCount;
    public float[] MotorRpms => motorRpms;

    // Fake data stuff
    public char[] CreateMotorData(float motor1, float motor2, float motor3, float motor4)
    {
        // Initialize array with 1 byte for motor count + 4 floats (4 bytes each)
        char[] rotationStats = new char[17];
        rotationStats[0] = (char)4; // Number of motors

        // Encode each float into 4 bytes and store in the array
        byte[] motor1Bytes = BitConverter.GetBytes(motor1);
        byte[] motor2Bytes = BitConverter.GetBytes(motor2);
        byte[] motor3Bytes = BitConverter.GetBytes(motor3);
        byte[] motor4Bytes = BitConverter.GetBytes(motor4);

        // Fill the char array with byte data
        Array.Copy(motor1Bytes, 0, rotationStats, 1, 4);
        Array.Copy(motor2Bytes, 0, rotationStats, 5, 4);
        Array.Copy(motor3Bytes, 0, rotationStats, 9, 4);
        Array.Copy(motor4Bytes, 0, rotationStats, 13, 4);

        return rotationStats;
    }

    // public void SimulateRotation(float motor1, float motor2, float motor3, float motor4)
    // {
    //     // Create the test motor data
    //     char[] testRotationStats = CreateMotorData(motor1, motor2, motor3, motor4);

    //     // Call the setRotation method to simulate
    //     setProps(testRotationStats);
    // }
    //private float speedChangeTimer = 0f;
    void SimulateMovement()
    {
         

        // // Make the drone move in a circle
        x = initX + (float)Math.Cos(angle) * 10f;
        z = initZ + (float)Math.Sin(angle) * 10f; 

        // Makes the drone roll and pitch to match with movement
        roll = (float)Math.Sin(angle) * 25f;
        pitch = (float)Math.Cos(angle) * 25f;
        yaw = (float)Math.Cos(angle) * 10f;

        // Period of the movement over a time period
        angle += artificalSpeed * Time.deltaTime;

        
        setMovement(x, y, z);



        // speedChangeTimer += Time.deltaTime;

        // // Change the speed every 0.5 seconds
        // if (speedChangeTimer >= 0.5f)
        // {
        // artificalSpeed = UnityEngine.Random.Range(7f, 20f); // Generate a new random speed
        // speedChangeTimer = 0f; // Reset the timer
        // }

        // // Move the drone in the Z direction based on the random speed
        // z += artificalSpeed * Time.deltaTime;
        // transform.position = new Vector3(transform.position.x, transform.position.y, z);

    }
    public void PassGate()
    {
        numGates++;
        Debug.Log("Passed gate: " + numGates);
        DataManager.Instance.GetLeaderBoard();
    }
    public int getPlacement()
    {
        return numGates;
    }
}