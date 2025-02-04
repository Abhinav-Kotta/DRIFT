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
public class RaceList
{
    public RaceResponse[] races;
}

public class RaceClient : MonoBehaviour
{
    [SerializeField]
    private string baseApiUrl = "http://34.68.252.128:8000";
    
    private RaceResponse currentRace;
    private WebSocket websocket;
    private bool isConnected = false;

    async void Start()
    {
        Debug.Log("Starting...");
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
                Debug.Log("Connected to race websocket!");
                isConnected = true;
            };

            websocket.OnMessage += (bytes) =>
            {
                // Parse the received JSON message
                string message = System.Text.Encoding.UTF8.GetString(bytes);
                HandleRaceData(message);
            };

            websocket.OnError += (e) =>
            {
                Debug.LogError($"WebSocket Error: {e}");
            };

            websocket.OnClose += (e) =>
            {
                Debug.Log("Connection closed");
                isConnected = false;
            };

            // Connecting
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
            Debug.Log($"Raw data received: {jsonData}"); // Log raw data for debugging
            
            // Parse position data
            var data = JsonUtility.FromJson<PositionData>(jsonData);
            if (data == null)
            {
                Debug.LogError($"Failed to parse position data from: {jsonData}");
                return;
            }

            // Log parsed position data
            Debug.Log($"Parsed position - X: {data.x:F2}, Y: {data.y:F2}, Z: {data.z:F2}");
            
            // If this GameObject has a Transform (it should), update its position
            transform.position = new Vector3(data.x, data.y, data.z);
        }
        catch (Exception e)
        {
            Debug.LogError($"Error parsing race data: {e.Message}\nRaw data: {jsonData}");
        }
    }

    public async Task<RaceResponse> CreateRace()
    {
        try
        {
            string createUrl = $"{baseApiUrl}/create_race";
            using (UnityWebRequest request = UnityWebRequest.PostWwwForm(createUrl, ""))
            {
                Debug.Log($"Creating new race at {createUrl}");
                var operation = request.SendWebRequest();
                while (!operation.isDone)
                    await Task.Yield();

                if (request.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError($"Failed to create race: {request.error}");
                    return null;
                }

                string jsonResponse = request.downloadHandler.text;
                Debug.Log($"Received response: {jsonResponse}");
                
                currentRace = JsonUtility.FromJson<RaceResponse>(jsonResponse);
                Debug.Log($"Race created with ID: {currentRace.race_id}");
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
            // Keep the connection alive
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

[Serializable]
public class PositionData
{
    public float x;
    public float y;
    public float z;
}