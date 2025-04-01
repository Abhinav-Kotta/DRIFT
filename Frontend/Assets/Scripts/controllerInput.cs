using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.XR;
using UnityEngine.SceneManagement;

public class ControllerInput : MonoBehaviour
{
    public static ControllerInput Instance { get; private set; } // Singleton instance

    bool rightPrimaryPressedLast = false;
    bool rightSecondaryPressedLast = false;
    bool leftPrimaryPressedLast = false;
    bool leftSecondaryPressedLast = false;
    bool leftOptionPressedLast = false;
    bool leftGripPressedLast = false;
    bool leftTriggerPressedLast = false; // Track the last state of the left trigger button

    DataManager dataManager = null;
    DroneViewCam droneViewCam = null;

    // Properties to store stick inputs
    public Vector2 LeftStickInput { get; private set; }
    public Vector2 RightStickInput { get; private set; }

    void Awake()
    {
        // Ensure only one instance of ControllerInput exists
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Debug.LogError("Multiple instances of ControllerInput detected. Destroying duplicate.");
            Destroy(gameObject);
        }
    }

    void Start()
    {
        droneViewCam = FindObjectOfType<DroneViewCam>();
        if (droneViewCam == null)
        {
            Debug.LogError("DroneViewCam not found in the scene.");
            return;
        }
        dataManager = DataManager.Instance;
    }

    void Update()
    {
        var leftHand = InputSystem.GetDevice<XRController>(CommonUsages.LeftHand);
        var rightHand = InputSystem.GetDevice<XRController>(CommonUsages.RightHand);

        // Handle button inputs
        if (rightHand != null)
        {
            var primary = rightHand["primaryButton"] as ButtonControl;
            var secondary = rightHand["secondaryButton"] as ButtonControl;

            if (primary != null)
            {
                if (primary.isPressed && !rightPrimaryPressedLast)
                {
                    Debug.Log("Right A Button Pressed -> Next Drone");
                    dataManager.NextDrone();
                }
                rightPrimaryPressedLast = primary.isPressed;
            }

            if (secondary != null)
            {
                if (secondary.isPressed && !rightSecondaryPressedLast)
                {
                    Debug.Log("Right B Button Pressed -> Prev Drone");
                    dataManager.PrevDrone();
                }
                rightSecondaryPressedLast = secondary.isPressed;
            }
        }

        if (leftHand != null)
        {
            var primary = leftHand["primaryButton"] as ButtonControl;
            var secondary = leftHand["secondaryButton"] as ButtonControl;
            var option = leftHand["menuButton"] as ButtonControl; // Add the menu/option button
            var grip = leftHand["gripButton"] as ButtonControl; // Add the grip button
            var trigger = leftHand["triggerButton"] as ButtonControl; // Add the trigger button

            if (primary != null)
            {
                if (primary.isPressed && !leftPrimaryPressedLast)
                {
                    Debug.Log("Left X Button Just Pressed - > Cycle Camera");
                    droneViewCam.cycleMode();
                }
                leftPrimaryPressedLast = primary.isPressed;
            }

            if (secondary != null)
            {
                if (secondary.isPressed && !leftSecondaryPressedLast)
                    Debug.Log("Left Y Button Just Pressed");

                leftSecondaryPressedLast = secondary.isPressed;
            }

            // Add debug print for the left option/menu button
            if (option != null)
            {
                // check if optionis pressed and grip is pressed after
                if (grip != null && grip.isPressed && !leftGripPressedLast)
                {
                    Debug.Log("Left Grip Button Just Pressed -> Exit Race");
                    SceneManager.LoadScene("Import");
                }
                if (trigger != null && trigger.isPressed && !leftTriggerPressedLast)
                {
                    Debug.Log("Left Trigger Button Just Pressed -> End Race");
                    SceneManager.LoadScene("StartingScene");
                }
                if (option.isPressed && !leftOptionPressedLast)
                {
                    Debug.Log("Left Option/Menu Button Just Pressed");
                    droneViewCam.popupPanel.enabled = !droneViewCam.popupPanel.enabled;
                }                
                leftOptionPressedLast = option.isPressed;
            }
        }

        // Handle stick inputs
        if (leftHand != null)
        {
            var leftStick = leftHand["thumbstick"] as StickControl;
            if (leftStick != null)
            {
                LeftStickInput = leftStick.ReadValue();
            }
        }

        if (rightHand != null)
        {
            var rightStick = rightHand["thumbstick"] as StickControl;
            if (rightStick != null)
            {
                RightStickInput = rightStick.ReadValue();
            }
        }
    }
}
