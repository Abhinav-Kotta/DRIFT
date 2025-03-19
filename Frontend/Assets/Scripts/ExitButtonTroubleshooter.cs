using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class ExitButtonTroubleshooter : MonoBehaviour
{
    [SerializeField] private Button exitButton;
    
    void Start()
    {
        // Check if the button exists
        if (exitButton == null)
        {
            Debug.LogError("Exit button reference is missing! Please assign it in the inspector.");
            return;
        }
        
        // Ensure the button is interactable
        exitButton.interactable = true;
        
        // Check if the button has a proper target graphic
        if (exitButton.targetGraphic == null)
        {
            Debug.LogWarning("Button has no target graphic assigned. Adding one now...");
            Image image = exitButton.GetComponent<Image>();
            if (image != null)
            {
                exitButton.targetGraphic = image;
            }
        }
        
        // Ensure the button's image has Raycast Target enabled
        Image buttonImage = exitButton.GetComponent<Image>();
        if (buttonImage != null)
        {
            if (!buttonImage.raycastTarget)
            {
                Debug.LogWarning("Button's Image raycastTarget was disabled. Enabling it now...");
                buttonImage.raycastTarget = true;
            }
        }
        
        // Check if EventSystem exists
        if (FindObjectOfType<EventSystem>() == null)
        {
            Debug.LogWarning("No EventSystem found in the scene. Creating one now...");
            GameObject eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<EventSystem>();
            eventSystem.AddComponent<StandaloneInputModule>();
        }
        
        // Log button's state for debugging
        Debug.Log($"Button state: Interactable={exitButton.interactable}, " +
                 $"GameObject Active={exitButton.gameObject.activeInHierarchy}, " +
                 $"Has OnClick Listeners={exitButton.onClick.GetPersistentEventCount() > 0}");
        
        // Add a click listener for testing
        exitButton.onClick.AddListener(ButtonClickTest);
    }
    
    void ButtonClickTest()
    {
        Debug.Log("Exit button was clicked successfully!");
    }
    
    void Update()
    {
        // Check for mouse clicks for debugging purposes
        if (Input.GetMouseButtonDown(0))
        {
            Debug.Log("Mouse clicked at: " + Input.mousePosition);
            
            // Check if our button is under the pointer
            if (RectTransformUtility.RectangleContainsScreenPoint(
                exitButton.GetComponent<RectTransform>(), 
                Input.mousePosition, 
                null))
            {
                Debug.Log("Mouse position is over our button!");
            }
        }
    }
}