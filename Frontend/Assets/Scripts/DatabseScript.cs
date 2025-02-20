using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class DatabaseScript : MonoBehaviour
{
    private string apiUrl;
    void Start()
    {
        apiUrl = ConfigLoader.GetApiUrl();
        if (apiUrl == null)
        {
            Debug.LogError("apiUrl not found");
        }

        Debug.Log("Starting to fetch drone data...");
        StartCoroutine(GetDroneData());
    }

    IEnumerator GetDroneData()
    {
        string uri = $"{apiUrl}/telemetry";
        Debug.Log("Attempting to connect to: " + uri);

        using (UnityWebRequest uwr = UnityWebRequest.Get(uri))
        {
            // Add headers
            uwr.SetRequestHeader("Content-Type", "application/json");
            uwr.SetRequestHeader("Accept", "application/json");

            Debug.Log("Sending web request...");
            yield return uwr.SendWebRequest();
            Debug.Log("Request completed.");

            if (uwr.result == UnityWebRequest.Result.ConnectionError || 
                uwr.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError($"Error: {uwr.error}");
                Debug.LogError($"Response Code: {uwr.responseCode}");
                Debug.LogError($"Response Data: {uwr.downloadHandler.text}");  // Added to see error message
            }
            else
            {
                Debug.Log($"Response Code: {uwr.responseCode}");
                Debug.Log($"Data received: {uwr.downloadHandler.text}");
            }
        }
    }
}