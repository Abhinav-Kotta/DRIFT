using UnityEngine;

public class raceGate : MonoBehaviour
{
    // Collider attached to this game object
    private Collider collider;

    void Start()
    {
        // Get the collider component attached to this game object
        collider = GetComponent<Collider>();
        // Set the collider to be a trigger
        collider.isTrigger = true;
    }

    void OnTriggerExit(Collider other)
    {
        Debug.Log("Trigger Entered by: " + other.gameObject.name);
        // Check if the object that exited the trigger has a DroneMover component
        DroneMover droneMover = other.gameObject.GetComponent<DroneMover>();
        if (droneMover != null)
        {
            // Call the PassGate method on the DroneMover component
            droneMover.PassGate();
        }
    }
}
