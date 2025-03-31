using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using Unity.Android.Gradle.Manifest;

public class CreateRaceScript : MonoBehaviour
{
    private string apiUrl;
    void Start()
    {
        apiUrl = ConfigLoader.GetApiUrl();

        Button thisButton = GetComponent<Button>();

        if (thisButton != null && !UserManager.Instance.IsLoggedIn)
        {
            thisButton.interactable = false;
        }
    }

    public void CreateRace()
    {
        Debug.Log("schlobert");
        Debug.Log(UserManager.Instance.UserId);
        StartCoroutine(CreateRaceCoroutine());
    }

    IEnumerator CreateRaceCoroutine()
    {
        int userId = UserManager.Instance.UserId;
        string url = $"{apiUrl}/create_race?user_id={userId}";

        using (UnityWebRequest www = new UnityWebRequest(url, "POST"))
        {
            www.uploadHandler = new UploadHandlerRaw(new byte[0]);
            www.downloadHandler = new DownloadHandlerBuffer();
            www.SetRequestHeader("Content-Type", "application/json");

            yield return www.SendWebRequest();

            Debug.Log("Response Code: " + www.responseCode);
            Debug.Log("Response Text: " + www.downloadHandler.text);

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError(www.error);
            }
            else
            {
                Debug.Log(www.downloadHandler.text);
                SceneManager.LoadScene("Import");
            }
        }
    }
}
