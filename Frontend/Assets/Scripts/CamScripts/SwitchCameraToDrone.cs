using UnityEngine;
using UnityEngine.UI;
public class SwitchCameraToDrone : MonoBehaviour
{
    public Button spectateButton;

    public DroneMover drone;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        spectateButton = GetComponentInChildren<Button>();
        spectateButton.onClick.AddListener(SwitchCamera);
    }

    void SwitchCamera()
    {
        // TODO: Figure out where the camMover script should go
        // call it and switch the camera to the drone, shouldnt be to bad
        // inshallah...

        // on click call the camMover script with the drone ref to switch to 
        Debug.Log("Switching camera to drone...");

        // sum like this 
        
        // camMover.SwitchCameraToDrone(drone);+
    
    }
}
