using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using System.Collections;
using UnityEngine.Networking;

public class SimpleExitRaceController : MonoBehaviour
{
    [SerializeField] private Button exitButton;
    [SerializeField] private GameObject popupPanel;
    [SerializeField] private TextMeshProUGUI popupText;
    
    void Start()
    {
        // Ensure EventSystem exists
        if (FindObjectOfType<EventSystem>() == null)
        {
            GameObject eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<EventSystem>();
            eventSystem.AddComponent<StandaloneInputModule>();
            Debug.Log("Created EventSystem");
        }
        
        // Set up button
        if (exitButton != null)
        {
            // Ensure button is interactable
            exitButton.interactable = true;
            
            // Make sure image has raycast target enabled
            Image buttonImage = exitButton.GetComponent<Image>();
            if (buttonImage != null)
            {
                buttonImage.raycastTarget = true;
            }
            
            // Clear existing listeners and add our own
            exitButton.onClick.RemoveAllListeners();
            exitButton.onClick.AddListener(OnButtonClick);
            
            Debug.Log("Button setup complete. Interactable: " + exitButton.interactable);
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
    }

    void Update()
    {
        // Test key to show popup (press T)
        if (Input.GetKeyDown(KeyCode.T))
        {
            Debug.Log("Test key pressed - showing popup");
            ShowPopup("Test popup message", true);
        }
    }
    
    public void OnButtonClick()
    {
        Debug.Log("Exit button clicked!");
        
        // Here you would normally call the API to end the race
        // For now, we'll just show a success popup
        ShowPopup("Race ended successfully!", true);
        
        // Hide the button to prevent multiple clicks
        if (exitButton != null)
        {
            exitButton.gameObject.SetActive(false);
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
        
        // Reset text rotation and alignment
        popupText.rectTransform.localRotation = Quaternion.identity;
        
        // For TextMeshPro text
        popupText.enableWordWrapping = true;
        popupText.alignment = TextAlignmentOptions.Center;
        
        // Set message
        popupText.text = message;
        popupText.color = success ? Color.white : new Color(1f, 0.5f, 0.5f);
        
        // Set panel position to center and make sure it's visible
        RectTransform panelRect = popupPanel.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.pivot = new Vector2(0.5f, 0.5f);
        panelRect.anchoredPosition = Vector2.zero;
        panelRect.sizeDelta = new Vector2(400, 200);
        
        // Show the panel
        popupPanel.SetActive(true);
        
        // Hide after delay
        Invoke("HidePopup", 5f);
    }
    
    private void HidePopup()
    {
        if (popupPanel != null)
        {
            Debug.Log("Hiding popup panel");
            popupPanel.SetActive(false);
        }
    }
}