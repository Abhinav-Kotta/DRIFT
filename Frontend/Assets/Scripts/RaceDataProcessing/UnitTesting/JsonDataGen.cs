using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class StreamFormat
{
    public float Timestamp;
    public int DID;
    public Position Position;
    public Attitude Attitude;
    public Velocity Velocity;
    public Gyro Gyro;
    public DroneInput DroneInput;
    public Battery Battery;
    public MotorRPM MotorRPM;
}

[System.Serializable]
public class Position
{
    public float PositionX;
    public float PositionY;
    public float PositionZ;
}

[System.Serializable]
public class Attitude
{
    public float AttitudeX;
    public float AttitudeY;
    public float AttitudeZ;
    public float AttitudeW;
}

[System.Serializable]
public class Velocity
{
    public float SpeedX;
    public float SpeedY;
    public float SpeedZ;
}

[System.Serializable]
public class Gyro
{
    public float GyroPitch;
    public float GyroRoll;
    public float GyroYaw;
}

[System.Serializable]
public class DroneInput
{
    public float InputThrottle;
    public float InputYaw;
    public float InputPitch;
    public float InputRoll;
}

[System.Serializable]
public class Battery
{
    public float BatteryVoltage;
    public float BatteryPercentage;
}

[System.Serializable]
public class MotorRPM
{
    public int MotorCount;
    public float Motor1;
    public float Motor2;
    public float Motor3;
    public float Motor4;
}

[System.Serializable]
public class JsonData
{
    public string EndPoint;
    public StreamFormat StreamFormat;
}

public class JsonDataGen : MonoBehaviour
{
    // Unit test for generating fake json data to test frontside data processing
    /*
    Json Format is 
    {
        "EndPoint": "127.0.0.1:9001",
        "StreamFormat": {
            "Timestamp": 1234567890.123,
            "DID": 1,
            "Position": {
                "PositionX": 1.23,
                "PositionY": 4.56,
                "PositionZ": 7.89
            },
            "Attitude": {
                "AttitudeX": 0.12,
                "AttitudeY": 3.45,
                "AttitudeZ": 6.78,
                "AttitudeW": 9.01
            },
            "Velocity": {
                "SpeedX": 1.23,
                "SpeedY": 4.56,
                "SpeedZ": 7.89
            },
            "Gyro": {
                "GyroPitch": 0.12,
                "GyroRoll": 3.45,
                "GyroYaw": 6.78
            },
            "Input": {
                "InputThrottle": 1.23,
                "InputYaw": 4.56,
                "InputPitch": 7.89,
                "InputRoll": 0.12
            },
            "Battery": {
                "BatteryVoltage": 3.45,
                "BatteryPercentage": 6.78
            },
            "MotorRPM": {
                "MotorCount": 4,
                "Motor1": 1000.0,
                "Motor2": 2000.0,
                "Motor3": 3000.0,
                "Motor4": 4000.0
            }
        }
    }
    */
    public static string GenerateFakeJsonData(string endPoint, double timestamp, int droneID, Vector3 position, Quaternion attitude, Vector3 velocity, Vector3 gyro, Vector4 DroneInput, Vector2 battery, float[] motorRPM)
    {
        string json = "{\n";
        json += $"  \"EndPoint\": \"{endPoint}\",\n";
        json += "  \"StreamFormat\": {\n";
        json += $"    \"Timestamp\": {timestamp},\n";
        json += $"    \"DID\": {droneID},\n";
        json += "    \"Position\": {\n";
        json += $"      \"PositionX\": {position.x},\n";
        json += $"      \"PositionY\": {position.y},\n";
        json += $"      \"PositionZ\": {position.z}\n";
        json += "    },\n";
        json += "    \"Attitude\": {\n";
        json += $"      \"AttitudeX\": {attitude.x},\n";
        json += $"      \"AttitudeY\": {attitude.y},\n";
        json += $"      \"AttitudeZ\": {attitude.z},\n";
        json += $"      \"AttitudeW\": {attitude.w}\n";
        json += "    },\n";
        json += "    \"Velocity\": {\n";
        json += $"      \"SpeedX\": {velocity.x},\n";
        json += $"      \"SpeedY\": {velocity.y},\n";
        json += $"      \"SpeedZ\": {velocity.z}\n";
        json += "    },\n";
        json += "    \"Gyro\": {\n";
        json += $"      \"GyroPitch\": {gyro.x},\n";
        json += $"      \"GyroRoll\": {gyro.y},\n";
        json += $"      \"GyroYaw\": {gyro.z}\n";
        json += "    },\n";
        json += "    \"Input\": {\n";
        json += $"      \"InputThrottle\": {DroneInput.x},\n";
        json += $"      \"InputYaw\": {DroneInput.y},\n";
        json += $"      \"InputPitch\": {DroneInput.z},\n";
        json += $"      \"InputRoll\": {DroneInput.w}\n";
        json += "    },\n";
        json += "    \"Battery\": {\n";
        json += $"      \"BatteryVoltage\": {battery.x},\n";
        json += $"      \"BatteryPercentage\": {battery.y}\n";
        json += "    },\n";
        json += "    \"MotorRPM\": {\n";
        json += $"      \"MotorCount\": {motorRPM.Length},\n";
        for (int i = 0; i < motorRPM.Length; i++)
        {
            json += $"      \"Motor{i + 1}\": {motorRPM[i]}";
            if (i < motorRPM.Length - 1)
            {
                json += ",";
            }
            json += "\n";
        }
        json += "    }\n";
        json += "  }\n";
        json += "}";
        return json;
    }

    // Generator function for Position
    public static Vector3 GeneratePosition()
    {
        return new Vector3(Random.Range(0.0f, 100.0f), Random.Range(0.0f, 100.0f), Random.Range(0.0f, 100.0f));
    }

    // Generator function for Attitude
    public static Quaternion GenerateAttitude()
    {
        return new Quaternion(Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f));
    }

    // Generator function for Velocity
    public static Vector3 GenerateVelocity()
    {
        return new Vector3(Random.Range(0.0f, 50.0f), Random.Range(0.0f, 50.0f), Random.Range(0.0f, 50.0f));
    }

    // Generator function for Gyro
    public static Vector3 GenerateGyro()
    {
        return new Vector3(Random.Range(0.0f, 10.0f), Random.Range(0.0f, 10.0f), Random.Range(0.0f, 10.0f));
    }

    // Generator function for Input
    public static Vector4 GenerateInput()
    {
        return new Vector4(Random.Range(0.0f, 5.0f), Random.Range(0.0f, 5.0f), Random.Range(0.0f, 5.0f), Random.Range(0.0f, 5.0f));
    }

    // Generator function for Battery
    public static Vector2 GenerateBattery()
    {
        return new Vector2(Random.Range(0.0f, 100.0f), Random.Range(0.0f, 100.0f));
    }

    // Generator function for MotorRPM
    public static float[] GenerateMotorRPM(int motorCount)
    {
        float[] motorRPM = new float[motorCount];
        for (int i = 0; i < motorCount; i++)
        {
            motorRPM[i] = Random.Range(0.0f, 5000.0f);
        }
        return motorRPM;
    }

    void Start()
    {
    }

    public string GenerateAndLogJsonData(int droneID)
    {
        string endPoint = "127.0.0.1:9001";
        double timestamp = Time.time; // Use Unity's Time.time to get the current time in seconds
        Vector3 position = GeneratePosition();
        Quaternion attitude = GenerateAttitude();
        Vector3 velocity = GenerateVelocity();
        Vector3 gyro = GenerateGyro();
        Vector4 DroneInput = GenerateInput();
        Vector2 battery = GenerateBattery();
        float[] motorRPM = GenerateMotorRPM(4);
        string jsonData = GenerateFakeJsonData(endPoint, timestamp, droneID, position, attitude, velocity, gyro, DroneInput, battery, motorRPM);
        Debug.Log(jsonData);
        return jsonData;
    }
}
