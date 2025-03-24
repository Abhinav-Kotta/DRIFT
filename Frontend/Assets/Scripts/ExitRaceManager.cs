using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using System.Collections;
using UnityEngine.Networking;
using System;
using System.Text;

public class SimpleExitRaceController : MonoBehaviour
{
    [SerializeField] private Button exitButton;
    [SerializeField] private GameObject popupPanel;
    [SerializeField] private TextMeshProUGUI popupText;
    
    // References to existing managers
    private DataManager dataManager;
    private StatManager statManager;
    private UserManager userManager;
    

    [SerializeField] private string apiServerIP; 
    [SerializeField] private int apiPort = 8000;
    [SerializeField] private string raceId;
    
    private int raceCreatorId = -1;
    private bool isCheckingCreator = false;
    
    void Awake()
    {
        // Initialize apiServerIP here instead of at declaration
        apiServerIP = ConfigLoader.GetApiUrl();
    }
    
    void Start()
    {
        // Get references to managers
        dataManager = DataManager.Instance;
        if (dataManager == null)
        {
            Debug.LogWarning("DataManager instance not found!");
        }
        
        statManager = FindObjectOfType<StatManager>();
        if (statManager == null)
        {
            Debug.LogWarning("StatManager not found in scene!");
        }

        // Get reference to UserManager
        userManager = UserManager.Instance;
        if (userManager == null)
        {
            Debug.LogWarning("UserManager instance not found!");
        }
        
        // Ensure EventSystem exists
        if (FindObjectOfType<EventSystem>() == null)
        {
            GameObject eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<EventSystem>();
            eventSystem.AddComponent<StandaloneInputModule>();
            Debug.Log("Created EventSystem");
        }
        
        // Set up button (initially hidden)
        if (exitButton != null)
        {
            exitButton.gameObject.SetActive(false);
            
            exitButton.interactable = true;
            
            Image buttonImage = exitButton.GetComponent<Image>();
            if (buttonImage != null)
            {
                buttonImage.raycastTarget = true;
            }
            
            exitButton.onClick.RemoveAllListeners();
            exitButton.onClick.AddListener(OnButtonClick);
            
            Debug.Log("Button setup complete. Initially hidden.");
        }
        else
        {
            Debug.LogError("Exit button reference is missing!");
        }
        
        // Hide popup initially
        if (popupPanel != null)
        {
            popupPanel.SetActive(false);
        }

        // Get selected drone ID if raceId is not set and we have a dataManager
        if (string.IsNullOrEmpty(raceId) && dataManager != null)
        {
            DroneMover selectedDrone = dataManager.GetSelectedDrone();
            if (selectedDrone != null)
            {
                raceId = selectedDrone.DID;
                Debug.Log($"Using drone ID as race ID: {raceId}");
            }
        }

        if (!string.IsNullOrEmpty(raceId) && userManager != null && userManager.IsLoggedIn)
        {
            StartCoroutine(CheckRaceCreator());
        }
        else
        {
            Debug.LogWarning("Cannot check race creator: Missing race ID or user not logged in");
        }
    }

    private IEnumerator CheckRaceCreator()
    {
        if (isCheckingCreator)
        {
            yield break;
        }

        isCheckingCreator = true;
        Debug.Log("Checking if current user is race creator...");

        string apiUrl = $"http://{apiServerIP}:{apiPort}/watch_race/{raceId}";
        
        UnityWebRequest request = UnityWebRequest.Get(apiUrl);
        request.SetRequestHeader("Content-Type", "application/json");
        
        yield return request.SendWebRequest();
        
        if (request.result == UnityWebRequest.Result.Success)
        {
            string responseJson = request.downloadHandler.text;
            Debug.Log($"Race info response: {responseJson}");
            
            try
            {
                string responseText = Encoding.UTF8.GetString(request.downloadHandler.data);
                
                if (responseText.Contains("\"user_id\":"))
                {
                    int userIdIndex = responseText.IndexOf("\"user_id\":") + "\"user_id\":".Length;
                    int commaIndex = responseText.IndexOf(",", userIdIndex);
                    if (commaIndex == -1)
                    {
                        commaIndex = responseText.IndexOf("}", userIdIndex);
                    }
                    
                    string userIdValue = responseText.Substring(userIdIndex, commaIndex - userIdIndex).Trim();
                    userIdValue = userIdValue.Replace("\"", "").Trim();
                    
                    if (int.TryParse(userIdValue, out int parsedId))
                    {
                        raceCreatorId = parsedId;
                        Debug.Log($"Extracted race creator ID: {raceCreatorId}");
                        
                        if (userManager != null)
                        {
                            bool isCreator = userManager.UserId == raceCreatorId;
                            Debug.Log($"Current user ID: {userManager.UserId}, Is creator: {isCreator}");
                            
                            if (exitButton != null)
                            {
                                exitButton.gameObject.SetActive(isCreator);
                                Debug.Log($"Exit button visibility set to: {isCreator}");
                            }
                        }
                    }
                    else
                    {
                        Debug.LogError($"Failed to parse user ID '{userIdValue}' as an integer");
                    }
                }
                else
                {
                    Debug.LogWarning("Could not find user_id in response");
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Error parsing race info: {e.Message}");
            }
        }
        else
        {
            Debug.LogError($"Error fetching race info: {request.error}");
        }
        
        isCheckingCreator = false;
    }
    
    public void OnButtonClick()
    {
        Debug.Log("Exit button clicked!");
        
        if (exitButton != null)
        {
            exitButton.interactable = false;
        }
        
        if (string.IsNullOrEmpty(raceId))
        {
            ShowPopup("Error: No race ID found. Please set a race ID.", false);
            if (exitButton != null)
            {
                exitButton.interactable = true;
            }
            return;
        }
        
        StartCoroutine(EndRace());
    }
    
    private IEnumerator EndRace()
    {
        ShowPopup("Ending race...", true);
        
        string apiUrl = $"http://{apiServerIP}:{apiPort}/end_race/{raceId}";
        Debug.Log($"Calling API: {apiUrl}");
        
        UnityWebRequest request = UnityWebRequest.Delete(apiUrl);
        request.SetRequestHeader("Content-Type", "application/json");
        
        yield return request.SendWebRequest();
        
        if (request.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("Race ended successfully: " + request.downloadHandler.text);
            ShowPopup("Race ended successfully!", true);
            
            if (statManager != null && statManager.needFakeData)
            {
                statManager.needFakeData = false;
                Debug.Log("Disabled fake data generation in StatManager");
            }
            
            yield return new WaitForSeconds(2.0f);
            

        }
        else
        {
            Debug.LogError("Error calling API: " + request.error);
            ShowPopup("Failed to end race. Please try again.", false);
            
            if (exitButton != null)
            {
                exitButton.interactable = true;
            }
        }
    }
    
    private void ShowPopup(string message, bool success)
    {
        Debug.Log($"ShowPopup called with message: {message}");
        
        if (popupPanel == null || popupText == null)
        {
            Debug.LogError("Popup panel or text is null!");
            return;
        }
        
        popupText.rectTransform.localRotation = Quaternion.identity;
        
        popupText.enableWordWrapping = true;
        popupText.alignment = TextAlignmentOptions.Center;
        
        popupText.text = message;
        popupText.color = success ? Color.white : new Color(1f, 0.5f, 0.5f);
        
        RectTransform panelRect = popupPanel.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.pivot = new Vector2(0.5f, 0.5f);
        panelRect.anchoredPosition = Vector2.zero;
        panelRect.sizeDelta = new Vector2(400, 200);
        
        popupPanel.SetActive(true);
        
        if (success)
        {
            Invoke("HidePopup", 5f);
        }
    }
    
    private void HidePopup()
    {
        if (popupPanel != null)
        {
            Debug.Log("Hiding popup panel");
            popupPanel.SetActive(false);
        }
    }
    
    public void SetRaceId(string id)
    {
        raceId = id;
        Debug.Log($"Race ID set to: {raceId}");
        
        if (!string.IsNullOrEmpty(raceId) && userManager != null && userManager.IsLoggedIn)
        {
            StartCoroutine(CheckRaceCreator());
        }
    }
    
    public IEnumerator CheckServerConnection(Action<bool> callback)
    {
        if (string.IsNullOrEmpty(apiServerIP))
        {
            Debug.LogError("API Server IP not set!");
            callback(false);
            yield break;
        }
        
        string testUrl = $"http://{apiServerIP}:{apiPort}/list_races";
        UnityWebRequest request = UnityWebRequest.Get(testUrl);
        
        yield return request.SendWebRequest();
        
        bool isConnected = request.result == UnityWebRequest.Result.Success;
        Debug.Log($"API server connection test: {(isConnected ? "Success" : "Failed")}");
        
        callback(isConnected);
    }
}