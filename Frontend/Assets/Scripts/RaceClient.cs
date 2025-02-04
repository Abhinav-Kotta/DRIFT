using UnityEngine;
using UnityEngine.Networking;
using System.Threading.Tasks;
using System;
using NativeWebSocket;

[Serializable]
public class RaceResponse
{
    public string race_id;
    public int udp_port;
    public int ws_port;
    public string status;
}

[Serializable]
public class TelemetryData
{
    public float timestamp;
    public Position position;
    public Attitude attitude;
    public Vector3Data velocity;
    public GyroData gyro;
    public InputData input;
    public BatteryData battery;
    public float[] motor_rpm;
}

[Serializable]
public class Position
{
    public float x;
    public float y;
    public float z;
}

[Serializable]
public class Attitude
{
    public float x;
    public float y;
    public float z;
    public float w;
}

[Serializable]
public class Vector3Data
{
    public float x;
    public float y;
    public float z;
}

[Serializable]
public class GyroData
{
    public float pitch;
    public float roll;
    public float yaw;
}

[Serializable]
public class InputData
{
    public float throttle;
    public float yaw;
    public float pitch;
    public float roll;
}

[Serializable]
public class BatteryData
{
    public float voltage;
    public float percentage;
}

public class RaceClient : MonoBehaviour
{
    [SerializeField]
    private string baseApiUrl = "http://34.68.252.128:8000";
    
    private RaceResponse currentRace;
    private WebSocket websocket;
    private bool isConnected = false;

    [SerializeField]
    private TMPro.TextMeshProUGUI batteryText;
    [SerializeField]
    private TMPro.TextMeshProUGUI motorRpmText;

    async void Start()
    {
        RaceResponse newRace = await CreateRace();
        if (newRace != null)
        {
            await ConnectToRace(newRace);
        }
    }

    async Task ConnectToRace(RaceResponse race)
    {
        try
        {
            string wsUrl = $"ws://{new Uri(baseApiUrl).Host}:8000/ws/race/{race.race_id}";
            websocket = new WebSocket(wsUrl);

            websocket.OnOpen += () =>
            {
                isConnected = true;
            };

            websocket.OnMessage += (bytes) =>
            {
                string message = System.Text.Encoding.UTF8.GetString(bytes);
                HandleRaceData(message);
            };

            websocket.OnError += (e) =>
            {
                Debug.LogError($"WebSocket Error: {e}");
            };

            websocket.OnClose += (e) =>
            {
                isConnected = false;
            };

            await websocket.Connect();
        }
        catch (Exception e)
        {
            Debug.LogError($"Error connecting to websocket: {e.Message}");
        }
    }

    void HandleRaceData(string jsonData)
    {
        try
        {
            var telemetry = JsonUtility.FromJson<TelemetryData>(jsonData);
            if (telemetry == null)
            {
                return;
            }

            transform.position = new Vector3(
                telemetry.position.x,
                telemetry.position.y,
                telemetry.position.z
            );

            transform.rotation = new Quaternion(
                telemetry.attitude.x,
                telemetry.attitude.y,
                telemetry.attitude.z,
                telemetry.attitude.w
            );

            Rigidbody rb = GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.velocity = new Vector3(
                    telemetry.velocity.x,
                    telemetry.velocity.y,
                    telemetry.velocity.z
                );
            }

            if (batteryText != null)
            {
                batteryText.text = $"Battery: {telemetry.battery.percentage:F1}% ({telemetry.battery.voltage:F1}V)";
            }

            if (motorRpmText != null && telemetry.motor_rpm != null)
            {
                string rpmText = "Motor RPM:";
                for (int i = 0; i < telemetry.motor_rpm.Length; i++)
                {
                    rpmText += $"\nMotor {i + 1}: {telemetry.motor_rpm[i]:F0}";
                }
                motorRpmText.text = rpmText;
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Error parsing telemetry data: {e.Message}\nRaw data: {jsonData}");
        }
    }

    public async Task<RaceResponse> CreateRace()
    {
        try
        {
            string createUrl = $"{baseApiUrl}/create_race";
            using (UnityWebRequest request = UnityWebRequest.PostWwwForm(createUrl, ""))
            {
                var operation = request.SendWebRequest();
                while (!operation.isDone)
                    await Task.Yield();

                if (request.result != UnityWebRequest.Result.Success)
                {
                    return null;
                }

                string jsonResponse = request.downloadHandler.text;
                currentRace = JsonUtility.FromJson<RaceResponse>(jsonResponse);
                return currentRace;
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Error creating race: {e.Message}");
            return null;
        }
    }

    void Update()
    {
        if (websocket != null)
        {
            websocket.DispatchMessageQueue();
        }
    }

    private async void OnApplicationQuit()
    {
        if (websocket != null && isConnected)
        {
            await websocket.Close();
        }
    }
}