using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System;
using UnityEngine.SceneManagement;

public class ExitRaceButton : MonoBehaviour
{
    [SerializeField] private Button exitRaceButton;
    [SerializeField] private float initialCheckDelay = 1f; // Time to wait before checking race creator
    [SerializeField] private float retryDelay = 3f; // Time to wait before retrying if race client isn't ready

    private string baseApiUrl;
    private RaceClient raceClient;
    private UserManager userManager;
    private bool isCreator = false;

    void Start()
    {
        baseApiUrl = ConfigLoader.GetApiUrl();
        raceClient = FindObjectOfType<RaceClient>();
        userManager = UserManager.Instance;

        if (exitRaceButton == null)
            exitRaceButton = GetComponent<Button>();

        exitRaceButton.interactable = false;
        
        CanvasGroup canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        canvasGroup.alpha = 0;

        exitRaceButton.onClick.AddListener(OnExitRaceClicked);
        
        StartCoroutine(CheckRaceCreator());
    }

    IEnumerator CheckRaceCreator()
    {
        yield return new WaitForSeconds(initialCheckDelay);

        while (raceClient == null || raceClient.currentRace == null || string.IsNullOrEmpty(raceClient.currentRace.race_id))
        {
            Debug.Log("Waiting for race information to be available...");
            yield return new WaitForSeconds(retryDelay);
            
            if (raceClient == null)
                raceClient = FindObjectOfType<RaceClient>();
        }

        string raceId = raceClient.currentRace.race_id;
        Debug.Log($"Race ID found: {raceId}. Checking if current user is the creator...");
        
        yield return StartCoroutine(GetRaceCreatorInfo(raceId));
        
        CanvasGroup canvasGroup = GetComponent<CanvasGroup>();
        if (isCreator)
        {
            exitRaceButton.interactable = true;
            canvasGroup.alpha = 1;
        }
        else
        {
            exitRaceButton.interactable = false;
            canvasGroup.alpha = 0;
        }
        Debug.Log($"Exit race button visibility set to: {isCreator}");
    }

    IEnumerator GetRaceCreatorInfo(string raceId)
    {
        string watchRaceUrl = $"{baseApiUrl}/watch_race/{raceId}/{userManager.UserId}";
        
        using (UnityWebRequest www = UnityWebRequest.Get(watchRaceUrl))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"Error fetching race creator info: {www.error}");
                isCreator = false;
            }
            else
            {
                try
                {
                    string jsonResponse = www.downloadHandler.text;
                    Debug.Log($"Race info response: {jsonResponse}");
                    
                    RaceCreatorResponse response = JsonUtility.FromJson<RaceCreatorResponse>(jsonResponse);
                    Debug.Log($"JSON converted response: {response}");
                    Debug.Log("RaceCreatorResponse Contents:");
                    Debug.Log($"  race_id: {response.race_id}");
                    Debug.Log($"  race_creator: {response.race_creator}");
                    Debug.Log($"  udp_port: {response.udp_port}");
                    Debug.Log($"  ws_port: {response.ws_port}");
                    Debug.Log($"  status: {response.status}");
                    Debug.Log($"User manager: {userManager}");
                    if (userManager != null && userManager.IsRaceCreator(response.race_creator))
                    {
                        isCreator = true;
                        Debug.Log($"Current user (ID: {userManager.UserId}) is the race creator (ID: {response.race_creator})");
                    }
                    else
                    {
                        isCreator = false;
                        Debug.Log($"Current user (ID: {userManager.UserId}) is NOT the race creator (ID: {response.race_creator})");
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError($"Error parsing race creator info: {e.Message}");
                    isCreator = false;
                }
            }
        }
    }

    void OnExitRaceClicked()
    {
        if (isCreator && raceClient != null && raceClient.currentRace != null)
        {
            StartCoroutine(EndRace(raceClient.currentRace.race_id));
        }
    }

    IEnumerator EndRace(string raceId)
    {
        string endRaceUrl = $"{baseApiUrl}/end_race/{raceId}";
        Debug.Log($"end race url: {endRaceUrl}");
        using (UnityWebRequest www = UnityWebRequest.Delete(endRaceUrl))
        {
            www.downloadHandler = new DownloadHandlerBuffer();
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"Error ending race: {www.error}");
            }
            else
            {
                Debug.Log($"Race ended successfully: {www.downloadHandler.text}");
                exitRaceButton.interactable = false;
                
                SceneManager.LoadScene("StartingScene");
            }
        }
    }
}

[Serializable]
public class RaceCreatorResponse
{
    public string race_id;
    public int race_creator;
    public int udp_port;
    public int ws_port;
    public string status;
}