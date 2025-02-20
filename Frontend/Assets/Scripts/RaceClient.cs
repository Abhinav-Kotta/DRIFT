using UnityEngine;
using UnityEngine.Networking;
using System.Threading.Tasks;
using System;
using NativeWebSocket;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Linq;


[Serializable]
public class RaceResponse
{
    public string race_id;
    public int udp_port;
    public int ws_port;
    public string status;
}

[Serializable]
public class RaceListResponse
{
    public RaceEntry[] races;
}

[Serializable]
public class RaceEntry
{
    public string race_id;
    public RaceResponse value;
}

[Serializable]
public class DroneData
{
    public object drone_id;
    public float timestamp;
    public Position position;
    public Attitude attitude;
    public Vector3Data velocity;
    public GyroData gyro;
    public Inputs inputs;
    public Battery battery;
    public int motor_count;
    public float[] motor_rpms;
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
public class Inputs
{
    public float throttle;
    public float yaw;
    public float pitch;
    public float roll;
}

[Serializable]
public class Battery
{
    public float percentage;
    public float voltage;
}

public class RaceClient : MonoBehaviour
{
    [SerializeField]
    private string baseApiUrl = "http://104.197.175.117:8000";
    private WebSocket websocket;
    private RaceResponse currentRace;
    private bool isListening = false;

    async void Start()
    {
        Debug.Log("RaceClient Start() called");
        await InitializeRaceConnection();
    }

    async Task InitializeRaceConnection()
    {
        try
        {
            Debug.Log("Starting race connection initialization...");

            currentRace = await ListRaces();

            if (currentRace == null)
            {
                Debug.Log("No races found, creating a new race...");
                await CreateRace();  // Create a race but don't store the response

                // Now get the list of races again and use the first one
                currentRace = await ListRaces();
                if (currentRace == null)
                {
                    Debug.LogError("Failed to get race after creation");
                    return;
                }
            }

            Debug.Log($"Using race with ID: {currentRace.race_id}");

            // Connect to the WebSocket for this race
            await ConnectToRaceWebSocket(currentRace.race_id);
        }
        catch (Exception e)
        {
            Debug.LogError($"Critical error in InitializeRaceConnection: {e.Message}\nStack trace: {e.StackTrace}");
        }
    }

    public async Task<RaceResponse> ListRaces()
    {
        try
        {
            string listUrl = $"{baseApiUrl}/list_races";
            using (UnityWebRequest request = UnityWebRequest.Get(listUrl))
            {
                var operation = request.SendWebRequest();
                while (!operation.isDone)
                    await Task.Yield();

                if (request.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError($"Error fetching races: {request.error}");
                    return null;
                }

                string jsonResponse = request.downloadHandler.text;
                Debug.Log($"Received races response: {jsonResponse}");

                try
                {
                    // Parse the outer response
                    JObject response = JObject.Parse(jsonResponse);
                    JArray races = (JArray)response["races"];

                    if (races != null && races.Count > 0)
                    {
                        // Get the first race object
                        JObject firstRaceContainer = (JObject)races[0];
                        // Get the first property of that object (the race ID and its data)
                        JProperty raceProp = firstRaceContainer.Properties().First();
                        // Get the race data
                        JObject raceData = (JObject)raceProp.Value;

                        return new RaceResponse
                        {
                            race_id = (string)raceData["race_id"],
                            udp_port = (int)raceData["udp_port"],
                            ws_port = (int)raceData["ws_port"],
                            status = (string)raceData["status"]
                        };
                    }
                    return null;
                }
                catch (Exception e)
                {
                    Debug.LogError($"Error parsing race list: {e.Message}");
                    Debug.LogError($"JSON response was: {jsonResponse}");
                    return null;
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Error listing races: {e.Message}");
            return null;
        }
    }

    async Task<RaceResponse> CreateRace()
    {
        string createUrl = $"{baseApiUrl}/create_race";
        Debug.Log($"Attempting to create race at: {createUrl}");

        try
        {
            using (UnityWebRequest request = new UnityWebRequest(createUrl, "POST"))
            {
                request.downloadHandler = new DownloadHandlerBuffer();
                request.uploadHandler = new UploadHandlerRaw(new byte[0]);
                request.SetRequestHeader("Content-Type", "application/json");

                var operation = request.SendWebRequest();
                while (!operation.isDone)
                {
                    await Task.Yield();
                }

                if (request.result != UnityWebRequest.Result.Success)
                {
                    return null;
                }

                string responseText = request.downloadHandler.text;
                Debug.Log($"Received response: {responseText}");

                try
                {
                    Debug.Log("in here");
                    var response = JsonUtility.FromJson<RaceResponse>(responseText);
                    Debug.Log($"Create race response: {JsonUtility.ToJson(response)}");


                    if (response == null)
                    {
                        Debug.LogError("Failed to parse JSON response");
                        Debug.LogError($"Raw response: {responseText}");
                    }
                    return response;
                }
                catch (Exception e)
                {
                    Debug.LogError($"JSON parsing error: {e.Message}");
                    Debug.LogError($"Raw response that failed to parse: {responseText}");
                    return null;
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Exception in CreateRace: {e.Message}");
            Debug.LogError($"Stack trace: {e.StackTrace}");
            return null;
        }
    }

    async Task ConnectToRaceWebSocket(string raceId)
    {
        try
        {
            string wsUrl = $"ws://104.197.175.117:8765/race/{currentRace.udp_port}";
            Debug.Log($"[WS] Connecting to WebSocket at: {wsUrl}");
            Debug.Log($"[WS] Race info:");
            Debug.Log($"    - Race ID: {currentRace.race_id}");
            Debug.Log($"    - UDP Port: {currentRace.udp_port}");
            Debug.Log($"    - Status: {currentRace.status}");

            websocket = new WebSocket(wsUrl);
            Debug.Log($"[WS] WebSocket instance created with State: {websocket.State}");

            websocket.OnOpen += () =>
            {
                Debug.Log("[WS] OnOpen event fired!");
                isListening = true;
            };

            websocket.OnError += (e) =>
            {
                Debug.LogError($"[WS] WebSocket Error: {e}");
                Debug.LogError($"[WS] Current State: {websocket.State}");
                isListening = false;
            };

            websocket.OnClose += (e) =>
            {
                Debug.Log($"[WS] WebSocket connection closed with State: {websocket.State}");
                isListening = false;
            };

            websocket.OnMessage += (bytes) =>
            {
                Debug.Log("[WS] OnMessage event fired");
                try
                {
                    var message = Encoding.UTF8.GetString(bytes);
                    Debug.Log($"[WS] Received message length: {bytes.Length}");
                    Debug.Log($"[WS] Decoded message: {message}");

                    var droneData = JsonConvert.DeserializeObject<DroneData>(message);
                    Debug.Log($"[WS] Position: x={droneData.position.x}, y={droneData.position.y}, z={droneData.position.z}");
                }
                catch (Exception e)
                {
                    Debug.LogError($"[WS] Error processing message: {e.Message}");
                }
            };

            Debug.Log("[WS] About to call Connect()...");
            await websocket.Connect();
            Debug.Log($"[WS] Connect() completed with State: {websocket.State}");
        }
        catch (Exception e)
        {
            Debug.LogError($"[WS] Failed to connect to WebSocket: {e.Message}");
            Debug.LogError($"[WS] Stack trace: {e.StackTrace}");
            if (websocket != null)
            {
                Debug.LogError($"[WS] Final WebSocket State: {websocket.State}");
            }
        }
    }

    void Update()
    {
        if (websocket != null)
        {
            websocket.DispatchMessageQueue();
        }
    }

    private async void OnDestroy()
    {
        if (websocket != null)
        {
            await websocket.Close();
        }
    }
}
