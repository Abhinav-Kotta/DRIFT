using UnityEngine;
using UnityEngine.Networking;
using System.Threading.Tasks;
using System;

[Serializable]
public class RaceResponse
{
    public int udp_port;
    public int ws_port;
    public string status;
}

public class RaceClient : MonoBehaviour
{
    [SerializeField]
    private string apiUrl = "http://34.68.252.128:8000/create_race";

    private RaceResponse currentRace;

    async void Start()
    {
        Debug.Log("starting");
        Debug.Log("Starting race creation...");
        RaceResponse race = await CreateRace();
        
        if (race != null)
        {
            Debug.Log($"Race created successfully!");
            Debug.Log($"UDP Port: {race.udp_port}");
            Debug.Log($"WebSocket Port: {race.ws_port}");
            Debug.Log($"Status: {race.status}");
        }
        else
        {
            Debug.LogError("Failed to create race!");
        }
    }

    public async Task<RaceResponse> CreateRace()
    {
        try
        {
            using (UnityWebRequest request = UnityWebRequest.PostWwwForm(apiUrl, ""))
            {
                Debug.Log($"Sending request to {apiUrl}");
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
                return currentRace;
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Error creating race: {e.Message}");
            return null;
        }
    }
}