using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class DroneMover : MonoBehaviour
{

    private Vector3 lastPosition;
    private Vector3 currentPosition;
    private float timeSinceLastUpdate = 0f;
    private float velocityUpdateInterval = 0.1f;
    private float calculatedSpeed = 0f;

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

    GameObject propeller1;
    GameObject propeller2;
    GameObject propeller3;
    GameObject propeller4;

    void Start()
    {
        lastPosition = transform.position;
        currentPosition = lastPosition;
        initX = gameObject.transform.position.x;
        initY = gameObject.transform.position.y;
        initZ = gameObject.transform.position.z;

        y = initY;

        propeller1 = transform.Find("propeller.1").gameObject;
        propeller2 = transform.Find("propeller.2").gameObject;
        propeller3 = transform.Find("propeller.3").gameObject;  
        propeller4 = transform.Find("propeller.4").gameObject;
        if (propeller1 == null || propeller2 == null || propeller3 == null || propeller4 == null)
        {
            Debug.LogError("Propellers not found");
        }
    }

    void Update()
    {
        if (!artificialMovement)
        {
            setMovement(x, y, z);
        }
        else
        {
            SimulateMovement();
        }
        if (!artificialRotation)
        {
            // setRotation(/*some data from liftoff as char array*/);
        }
        else
        {
            SimulateRotation(motorSpeed1, motorSpeed2, motorSpeed3, motorSpeed4);
        }

        transform.position = getPosition();
        transform.rotation = getRotation();
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
        timeSinceLastUpdate += Time.deltaTime;
        
        if (timeSinceLastUpdate >= velocityUpdateInterval)
        {
            currentPosition = transform.position;
            
            Vector3 displacement = currentPosition - lastPosition;
            
            calculatedSpeed = displacement.magnitude / timeSinceLastUpdate;
            
            lastPosition = currentPosition;
            timeSinceLastUpdate = 0f;
        }
        
        return calculatedSpeed;
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
        this.x = x;
        this.y = y;
        this.z = z;
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
        this.velocity = new Vector3(droneData.velocity.x, droneData.velocity.y, droneData.velocity.z);
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

    public void setProps(char[] rotationStats)
    {
        // May need to change type of this depending on format received from liftoff. For now we will use char array as it is the most flexible

        /*MotorRPM (1 byte + (1 float * number of motors)) - the rotations per minute for each
        motor. The byte at the front of this piece of data defines the amount of motors on the
        drone, and thus how many floats you can expect to find next. The sequence of motors for
        a quadcopter in Liftoff is as follows: left front, right front, left back, right back.*/
        // Given we are going to use four propellers we only care about 17 bytes which is 136 bits
        
        // Get the number of motors if not four. WTF. just set the rotation to some number
        int numMotors = rotationStats[0];
        
        if (numMotors != 4)
        {
            Debug.LogError("Number of motors is not 4");
            // Front two propellers
            propeller1.transform.Rotate(0, 0, 1200 * Time.deltaTime);
            propeller2.transform.Rotate(0, 0, 1200 * Time.deltaTime);

            // Back two propellers
            propeller3.transform.Rotate(0, 0, -1200 * Time.deltaTime);
            propeller4.transform.Rotate(0, 0, -1200 * Time.deltaTime);
            return;
        }
       
        // 17 bytes total. 1 byte for number of motors and 4 floats (which are 4 bytes total) for each motor
        // Decode the floats for each motor (each float is 4 bytes)
        float motor1 = BitConverter.ToSingle(new byte[] { (byte)rotationStats[1], (byte)rotationStats[2], (byte)rotationStats[3], (byte)rotationStats[4] }, 0);
        float motor2 = BitConverter.ToSingle(new byte[] { (byte)rotationStats[5], (byte)rotationStats[6], (byte)rotationStats[7], (byte)rotationStats[8] }, 0);
        float motor3 = BitConverter.ToSingle(new byte[] { (byte)rotationStats[9], (byte)rotationStats[10], (byte)rotationStats[11], (byte)rotationStats[12] }, 0);
        float motor4 = BitConverter.ToSingle(new byte[] { (byte)rotationStats[13], (byte)rotationStats[14], (byte)rotationStats[15], (byte)rotationStats[16] }, 0);

        // Front two propellers
        propeller1.transform.Rotate(0, 0, motor1 * Time.deltaTime);
        propeller2.transform.Rotate(0, 0, motor2 * Time.deltaTime);

        // Back two propellers. May need to change the sign of the rotation if propeller value from liftoff is signed
        propeller3.transform.Rotate(0, 0, -motor3 * Time.deltaTime);
        propeller4.transform.Rotate(0, 0, -motor4 * Time.deltaTime);
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

    public void SimulateRotation(float motor1, float motor2, float motor3, float motor4)
    {
        // Create the test motor data
        char[] testRotationStats = CreateMotorData(motor1, motor2, motor3, motor4);

        // Call the setRotation method to simulate
        setProps(testRotationStats);
    }
    void SimulateMovement()
    {
        // Make the drone move in a circle
        x = initX + (float)Math.Cos(angle) * 10f;
        z = initZ + (float)Math.Sin(angle) * 10f; 

        // Makes the drone roll and pitch to match with movement
        roll = (float)Math.Sin(angle) * 25f;
        pitch = (float)Math.Cos(angle) * 25f;
        yaw = (float)Math.Cos(angle) * 10f;

        // Period of the movement over a time period
        angle += artificalSpeed * Time.deltaTime;

        setMovement(x, y, z);
    }
}