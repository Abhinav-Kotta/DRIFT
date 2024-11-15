[System.Serializable]
public class DroneData
{
    public float Timestamp;
    public Vector3Data Position;
    public QuaternionData Attitude;
    public Vector3Data Gyro;
    public DroneInputsData Inputs;
    public BatteryStateData Battery;
    public float[] MotorRPMs;
}

[System.Serializable]
public struct Vector3Data
{
    public float X, Y, Z;
}

[System.Serializable]
public struct QuaternionData
{
    public float X, Y, Z, W;
}

[System.Serializable]
public struct DroneInputsData
{
    public float Throttle, Yaw, Pitch, Roll;
}

[System.Serializable]
public struct BatteryStateData
{
    public float Voltage, Percentage;
}