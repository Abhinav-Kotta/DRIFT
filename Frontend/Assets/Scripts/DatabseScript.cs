using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class DatabaseScript : MonoBehaviour
{
    void Start()
    {
        StartCoroutine(GetDroneData());
    }

    IEnumerator GetDroneData()
    {
        string uri = "http://127.0.0.1:5000/telemetry";
        UnityWebRequest uwr = UnityWebRequest.Get(uri);
        yield return uwr.SendWebRequest();

        if (uwr.result == UnityWebRequest.Result.ConnectionError)
        {
            Debug.Log("Error: " + uwr.error);
        }
        else
        {
            Debug.Log("Data received: " + uwr.downloadHandler.text);
        }
    }
}