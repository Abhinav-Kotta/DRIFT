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
        // Get references
        baseApiUrl = ConfigLoader.GetApiUrl();
        raceClient = FindObjectOfType<RaceClient>();
        userManager = UserManager.Instance;

        // Get reference to button if not assigned
        if (exitRaceButton == null)
            exitRaceButton = GetComponent<Button>();

        // Initially hide the button by making it non-interactable and transparent
        exitRaceButton.interactable = false;
        
        // Make the button visually hidden but keep GameObject active
        CanvasGroup canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        canvasGroup.alpha = 0; // Make it invisible

        // Add listener to the button click event
        exitRaceButton.onClick.AddListener(OnExitRaceClicked);
        
        // Start checking if current user is race creator
        StartCoroutine(CheckRaceCreator());
    }

    IEnumerator CheckRaceCreator()
    {
        // Initial delay to allow other components to initialize
        yield return new WaitForSeconds(initialCheckDelay);

        // Wait until race client has race information
        while (raceClient == null || raceClient.currentRace == null || string.IsNullOrEmpty(raceClient.currentRace.race_id))
        {
            Debug.Log("Waiting for race information to be available...");
            yield return new WaitForSeconds(retryDelay);
            
            // If raceClient is still null, try to find it again
            if (raceClient == null)
                raceClient = FindObjectOfType<RaceClient>();
        }

        // Race information is now available
        string raceId = raceClient.currentRace.race_id;
        Debug.Log($"Race ID found: {raceId}. Checking if current user is the creator...");
        
        // Call the watch_race endpoint to get creator info
        yield return StartCoroutine(GetRaceCreatorInfo(raceId));
        
        // Update button visibility based on creator status
        CanvasGroup canvasGroup = GetComponent<CanvasGroup>();
        if (isCreator)
        {
            exitRaceButton.interactable = true;
            canvasGroup.alpha = 1; // Make it visible
        }
        else
        {
            exitRaceButton.interactable = false;
            canvasGroup.alpha = 0; // Keep it invisible
        }
        Debug.Log($"Exit race button visibility set to: {isCreator}");
    }

    IEnumerator GetRaceCreatorInfo(string raceId)
    {
        string watchRaceUrl = $"{baseApiUrl}/watch_race/{raceId}";
        
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
                    
                    // Parse the response to get race_creator
                    RaceCreatorResponse response = JsonUtility.FromJson<RaceCreatorResponse>(jsonResponse);
                    Debug.Log($"JSON converted response: {response}");
                    Debug.Log("RaceCreatorResponse Contents:");
                    Debug.Log($"  race_id: {response.race_id}");
                    Debug.Log($"  race_creator: {response.race_creator}");
                    Debug.Log($"  udp_port: {response.udp_port}");
                    Debug.Log($"  ws_port: {response.ws_port}");
                    Debug.Log($"  status: {response.status}");
                    // Check if current user is the race creator
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
                // Show error message to user
            }
            else
            {
                Debug.Log($"Race ended successfully: {www.downloadHandler.text}");
                exitRaceButton.interactable = false;
                
                // Handle race end - maybe return to menu or lobby scene
                SceneManager.LoadScene("StartingScene");
            }
        }
    }
}

// Helper class for deserializing the race creator information
[Serializable]
public class RaceCreatorResponse
{
    public string race_id;
    public int race_creator;
    public int udp_port;
    public int ws_port;
    public string status;
}