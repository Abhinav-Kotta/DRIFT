using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

public class DroneData
{
    public float Timestamp { get; set; }
    public Vector3 Position { get; set; }
    public Quaternion Attitude { get; set; }
    public Vector3 Gyro { get; set; }
    public DroneInputs Inputs { get; set; }
    public BatteryState Battery { get; set; }
    public float[] MotorRPMs { get; set; }

    public DroneData()
    {
        Position = new Vector3();
        Attitude = new Quaternion();
        Gyro = new Vector3();
        Inputs = new DroneInputs();
        Battery = new BatteryState();
        MotorRPMs = new float[4];
    }
}

public class Vector3
{
    public float X { get; set; }
    public float Y { get; set; }
    public float Z { get; set; }
}

public class Quaternion
{
    public float X { get; set; }
    public float Y { get; set; }
    public float Z { get; set; }
    public float W { get; set; }
}

public class DroneInputs
{
    public float Throttle { get; set; }
    public float Yaw { get; set; }
    public float Pitch { get; set; }
    public float Roll { get; set; }
}

public class BatteryState
{
    public float Voltage { get; set; }
    public float Percentage { get; set; }
}

public class Middleware
{
    private UdpClient udpClient;
    private UdpClient unityPublisher;
    private bool isRunning = false;
    private const int BUFFER_SIZE = 1024;

    public Middleware(string listenAddress = "127.0.0.1", int listenPort = 9001)
    {
        udpClient = new UdpClient(new IPEndPoint(IPAddress.Parse(listenAddress), listenPort));
        unityPublisher = new UdpClient();
        Console.WriteLine($"Middleware listening on {listenAddress}:{listenPort}");
    }

    public async Task StartAsync()
    {
        isRunning = true;
        Console.WriteLine("Middleware started...");
        var unityEndpoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 5555);

        while (isRunning)
        {
            try
            {
                UdpReceiveResult result = await udpClient.ReceiveAsync();
                DroneData droneData = ParseTelemetry(result.Buffer);
                string json = JsonSerializer.Serialize(droneData);
                byte[] jsonBytes = Encoding.UTF8.GetBytes(json);
                
                // Debug prints
                Console.WriteLine($"Sending {jsonBytes.Length} bytes to Unity port 5555");
                await unityPublisher.SendAsync(jsonBytes, jsonBytes.Length, unityEndpoint);
                Console.WriteLine("Data sent to Unity");
                
                Console.WriteLine($"Position: ({droneData.Position.X:F2}, {droneData.Position.Y:F2}, {droneData.Position.Z:F2})");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }
    }

    private DroneData ParseTelemetry(byte[] data)
    {
        DroneData droneData = new DroneData();
        int offset = 0;

        try
        {
            // Parse timestamp (float - 4 bytes)
            droneData.Timestamp = BitConverter.ToSingle(data, offset);
            offset += 4;

            // Parse position (3 floats - 12 bytes)
            droneData.Position.X = BitConverter.ToSingle(data, offset);
            droneData.Position.Y = BitConverter.ToSingle(data, offset + 4);
            droneData.Position.Z = BitConverter.ToSingle(data, offset + 8);
            offset += 12;

            // Parse attitude (4 floats - 16 bytes)
            droneData.Attitude.X = BitConverter.ToSingle(data, offset);
            droneData.Attitude.Y = BitConverter.ToSingle(data, offset + 4);
            droneData.Attitude.Z = BitConverter.ToSingle(data, offset + 8);
            droneData.Attitude.W = BitConverter.ToSingle(data, offset + 12);
            offset += 16;

            // Parse gyro (3 floats - 12 bytes)
            droneData.Gyro.X = BitConverter.ToSingle(data, offset);
            droneData.Gyro.Y = BitConverter.ToSingle(data, offset + 4);
            droneData.Gyro.Z = BitConverter.ToSingle(data, offset + 8);
            offset += 12;

            // Parse inputs (4 floats - 16 bytes)
            droneData.Inputs.Throttle = BitConverter.ToSingle(data, offset);
            droneData.Inputs.Yaw = BitConverter.ToSingle(data, offset + 4);
            droneData.Inputs.Pitch = BitConverter.ToSingle(data, offset + 8);
            droneData.Inputs.Roll = BitConverter.ToSingle(data, offset + 12);
            offset += 16;

            // Parse battery (2 floats - 8 bytes)
            droneData.Battery.Voltage = BitConverter.ToSingle(data, offset);
            droneData.Battery.Percentage = BitConverter.ToSingle(data, offset + 4);
            offset += 8;

            // Parse motor RPMs
            byte motorCount = data[offset++];
            droneData.MotorRPMs = new float[motorCount];
            for (int i = 0; i < motorCount && i < 4; i++)
            {
                droneData.MotorRPMs[i] = BitConverter.ToSingle(data, offset);
                offset += 4;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error parsing telemetry: {ex.Message}");
            return new DroneData(); // Return empty data on error
        }

        return droneData;
    }

    public void Stop()
    {
        isRunning = false;
        udpClient.Close();
        unityPublisher.Close();
    }

    public static async Task Main(string[] args)
    {
        var middleware = new Middleware();
        
        // Handle Ctrl+C gracefully
        Console.CancelKeyPress += (s, e) =>
        {
            e.Cancel = true;
            middleware.Stop();
        };

        try
        {
            await middleware.StartAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Fatal error: {ex.Message}");
        }
    }
}