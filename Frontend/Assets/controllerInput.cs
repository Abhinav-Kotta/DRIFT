using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.XR;

public class ControllerInput : MonoBehaviour
{
    bool rightPrimaryPressedLast = false;
    bool rightSecondaryPressedLast = false;
    bool leftPrimaryPressedLast = false;
    bool leftSecondaryPressedLast = false;
    DataManager dataManager = null;
    DroneViewCam droneViewCam = null;

    // Properties to store stick inputs
    public Vector2 LeftStickInput { get; private set; }
    public Vector2 RightStickInput { get; private set; }

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

            if (primary != null)
            {
                if (primary.isPressed && !leftPrimaryPressedLast)
                    Debug.Log("Left X Button Just Pressed");
            
                leftPrimaryPressedLast = primary.isPressed;
                droneViewCam.cycleMode();
            }

            if (secondary != null)
            {
                if (secondary.isPressed && !leftSecondaryPressedLast)
                    Debug.Log("Left Y Button Just Pressed");

                leftSecondaryPressedLast = secondary.isPressed;
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
