using System;
//using System.Numerics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class DroneMover : MonoBehaviour
{
    //can used for testing for movement and rotation
    [SerializeField] bool artificialMovement = true;
    [SerializeField] bool artificialRotation = true;

    //testing porpuses. Used to simulate movement rolling and pitching behavior
    public float angle = 0f;

    //artificial speed of the drone for testing
    [SerializeField] [Range(0,20)] float artificalSpeed = 1f;

    //postion of drone
    [SerializeField] float x;
    [SerializeField] float y;
    [SerializeField] float z;

    //angle of drone
    [SerializeField] float roll;
    [SerializeField] float pitch;
    [SerializeField] float yaw;

    //Used for rotating propeller. Measured in rotations per minute??
    [SerializeField] [Range(0, 5000)] int motorSpeed1 = 0;  //Measured in rotations per minute??
    [SerializeField] [Range(0, 5000)] int motorSpeed2 = 0;
    [SerializeField] [Range(0, 5000)] int motorSpeed3 = 0;
    [SerializeField] [Range(0, 5000)] int motorSpeed4 = 0;

    GameObject propeller1;
    GameObject propeller2;
    GameObject propeller3;
    GameObject propeller4;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        propeller1 = GameObject.Find("propeller.1");
        propeller2 = GameObject.Find("propeller.2");

        propeller3 = GameObject.Find("propeller.3");   
        propeller4 = GameObject.Find("propeller.4");
        if(propeller1 == null || propeller2 == null || propeller3 == null || propeller4 == null)
        {
            Debug.LogError("Propellers not found");
        }
    }

    // Update is called once per frame
    void Update()
    {
        /*
        //basic idea is that if artificial movement is enabled, then simulate movement if not then use the set movement
        //simuate Movement will just make a position of the drone to move in a circle then call setMovement with the artificial values

        //similarly for rotation, if artificial rotation is enabled, then rotate the propellers, if not then use the setRotation
        //simulateRotation does some intereting thiongs for testing. basically it uses the given floats to make the dataType that is expected from liftoff
        //This is then given to setRotation which will then unconvert the data and rotate the propellers with the float.
        //this is done to simulate the data that is expected from liftoff and to test the setRotation function.
        
        //Logic is a little wierd here since we are simulating rotation by giving the set rotation the same type that it recieves from liftoff which is a little convuluted
        */


        
        if(!artificialMovement){
            simulateMovement(); 
        }else
        {
            setMovement(x,y,z);
        }
        if(!artificialRotation){
            //setRotation(/*some data from liftoff as char array*/);
        }else{
           simulateRotation(motorSpeed1, motorSpeed2, motorSpeed3, motorSpeed4);
        }


        transform.position = getPosition();
        transform.rotation = getRotation();
    }
    public Vector3 getPosition()
    {
        return new Vector3(x, y, z);
    }
    public Quaternion getRotation()
    {
        return Quaternion.Euler(pitch, yaw, roll);
    }

    //START MOVEMENT STUFF. Pretty simple stuff
    void simulateMovement()
    {
        //make the drone move in a circle
        x = (float)Math.Cos(angle) * 10f;
        z = (float)Math.Sin(angle) * 10f; 

        //makes the drone roll and pitch to match with movement
        roll = (float)Math.Sin(angle) * 25f;
        pitch = (float)Math.Cos(angle) * 25f;

        //Period of the movement over a time period
        angle += artificalSpeed * Time.deltaTime;

        setMovement(x,y,z);
    }
    public void setMovement(float x, float y, float z)
    {
        //set the position of the drone
        this.x = x;
        this.y = y;
        this.z = z;
    }
    //END MOVEMENT STUFF


    //START ROTATION STUFF. Probably the most complex stuff right here
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

    public void simulateRotation(float motor1, float motor2, float motor3, float motor4)
    {
        // Create the test motor data
        char[] testRotationStats = CreateMotorData(motor1, motor2, motor3, motor4);

        // Call the setRotation method to simulate
        setRotation(testRotationStats);
    }
    //we will be expecting only four blades. If something fucks up we will just set rotation to some num
    public void setRotation(char[] rotationStats)
    {
        //may need to change type of this depending on format recieved from liftoff. For now we will use char array as it is the most flexible

        /*MotorRPM (1 byte + (1 float * number of motors)) - the rotations per minute for each
        motor. The byte at the front of this piece of data defines the amount of motors on the
        drone, and thus how many floats you can expect to find next. The sequence of motors for
        a quadcopter in Liftoff is as follows: left front, right front, left back, right back.*/
        //given we are going to use four propellers we only care about 17 bytes which is 136 bits
        
        //get the number of motors if not four. WTF. just set the rotation to some number
        int numMotors = rotationStats[0];
        
        if(numMotors != 4)
        {
            Debug.LogError("Number of motors is not 4");
            //front two propellers
            propeller1.transform.Rotate(0, 0, 1200 * Time.deltaTime);
            propeller2.transform.Rotate(0, 0, 1200 * Time.deltaTime);

            //back two propellers
            propeller3.transform.Rotate(0, 0, -1200 * Time.deltaTime);
            propeller4.transform.Rotate(0, 0, -1200 * Time.deltaTime);
            return;
        }
       
        //17 bytes total. 1 byte for number of motors and 4 floats(which are 4 bytes total) for each motor
        // Decode the floats for each motor (each float is 4 bytes)
        float motor1 = BitConverter.ToSingle(new byte[] { (byte)rotationStats[1], (byte)rotationStats[2], (byte)rotationStats[3], (byte)rotationStats[4] }, 0);
        float motor2 = BitConverter.ToSingle(new byte[] { (byte)rotationStats[5], (byte)rotationStats[6], (byte)rotationStats[7], (byte)rotationStats[8] }, 0);
        float motor3 = BitConverter.ToSingle(new byte[] { (byte)rotationStats[9], (byte)rotationStats[10], (byte)rotationStats[11], (byte)rotationStats[12] }, 0);
        float motor4 = BitConverter.ToSingle(new byte[] { (byte)rotationStats[13], (byte)rotationStats[14], (byte)rotationStats[15], (byte)rotationStats[16] }, 0);

        //so if we pass in 4 04B0 04B0 04B0 04B0 we should get 1200 for each motor (0X404B004B004B004B0)
        Debug.Log("Motor 1: " + motor1);
        Debug.Log("Motor 2: " + motor2);    
        Debug.Log("Motor 3: " + motor3);
        Debug.Log("Motor 4: " + motor4);

        //front two propellers
        propeller1.transform.Rotate(0, 0, motor1 * Time.deltaTime);
        propeller2.transform.Rotate(0, 0, motor2 * Time.deltaTime);

        //back two propellers. May need to change the sign of the rotation if propeller value from liftoff is signed
        propeller3.transform.Rotate(0, 0, -motor3 * Time.deltaTime);
        propeller4.transform.Rotate(0, 0, -motor4 * Time.deltaTime);
    }
    //END ROTATION STUFF. Thank god that is over
}
