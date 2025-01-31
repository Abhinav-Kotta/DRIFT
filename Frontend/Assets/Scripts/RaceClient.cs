using UnityEngine;
using UnityEngine.Networking;
using System.Threading.Tasks;
using System;

[Serializable]
public class RaceResponse
{
    public string race_id;
    public int udp_port;
    public int ws_port;
    public string status;
}

public class RaceClient : MonoBehaviour
{
    [SerializeField]
    private string baseApiUrl = "http://34.68.252.128:8000";

    private RaceResponse currentRace;

    async void Start()
    {
        Debug.Log("Starting...");
    
        // First create a race to get a valid race ID
        RaceResponse newRace = await CreateRace();
        if (newRace != null)
        {
            Debug.Log($"Created race with ID: {newRace.race_id}");
            
            // Now test watching this race
            RaceResponse watchedRace = await WatchRace(newRace.race_id);
            if (watchedRace != null)
            {
                Debug.Log($"Successfully watching race!");
                Debug.Log($"Race ID: {watchedRace.race_id}");
                Debug.Log($"UDP Port: {watchedRace.udp_port}");
                Debug.Log($"WS Port: {watchedRace.ws_port}");
                Debug.Log($"Status: {watchedRace.status}");
            }
            else
            {
                Debug.LogError("Failed to watch race!");
            }
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

    public async Task<RaceResponse> WatchRace(string raceId)
    {
        try
        {
            string watchUrl = $"{baseApiUrl}/watch_race/{raceId}";
            using (UnityWebRequest request = UnityWebRequest.Get(watchUrl))
            {
                Debug.Log($"Querying race: {watchUrl}");
                var operation = request.SendWebRequest();

                while (!operation.isDone)
                    await Task.Yield();

                if (request.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError($"Failed to watch race: {request.error}");
                    return null;
                }

                string jsonResponse = request.downloadHandler.text;
                Debug.Log($"Received race data: {jsonResponse}");
                
                currentRace = JsonUtility.FromJson<RaceResponse>(jsonResponse);
                Debug.Log($"Connected to race with ID: {currentRace.race_id}");
                return currentRace;
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Error watching race: {e.Message}");
            return null;
        }
    }

    public async Task<RaceResponse[]> GetLiveRaces()
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
                    Debug.LogError($"Failed to get race list: {request.error}");
                    return null;
                }

                string jsonResponse = request.downloadHandler.text;
                RaceList raceList = JsonUtility.FromJson<RaceList>(jsonResponse);
                return raceList.races;
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Error getting race list: {e.Message}");
            return null;
        }
    }

    public RaceResponse GetCurrentRace()
    {
        return currentRace;
    }
}