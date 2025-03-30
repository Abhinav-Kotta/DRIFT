using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class DeviceListDisplay : MonoBehaviour
{
    public Text deviceText; // Assign in the Inspector to the DeviceText UI element

    void Start()
    {
        // Initialize the device list when the menu is loaded
        UpdateDeviceList();
    }

    public void UpdateDeviceList()
    {
        deviceText.text = "Connected Devices:\n";
        foreach (var device in InputSystem.devices)
        {
            deviceText.text += device.displayName + "\n";
        }
    }
}
