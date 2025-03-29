using System;
using System.Collections.Generic;
using UnityEngine;

public class DataManager : MonoBehaviour
{
    private static DataManager instance;
    public static DataManager Instance
    {
        get
        {
            if (instance == null)
            {
                Debug.LogError("DataManager instance is null! Make sure it's in the scene.");
            }
            return instance;
        }
    }

    private Dictionary<string, DroneMover> droneIdToGameObjectMap;
    

    //This is all drones not mapped to a input stream
    private List<DroneMover> unmappedDroneMovers;

    //Drone movers that are mapped to input streams from Liftoff
    private List<DroneMover> activeDroneMovers;

    //Currently selected drone for data viewing/drone mover and cam-mover strict
    private DroneMover selectedDrone;
    
    //Used for drone selection rotation/indexing
    public int selectedDroneIndex = 0;
    public event Action<DroneMover> onDroneAdded;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        activeDroneMovers = new List<DroneMover>();
        // Initialize the mapping dictionary
        droneIdToGameObjectMap = new Dictionary<string, DroneMover>();
        unmappedDroneMovers = new List<DroneMover>(FindObjectsOfType<DroneMover>());
    }
    public void UpdateDroneData(DroneData droneData)
    {
        var droneId = droneData.drone_id;

        if (!droneIdToGameObjectMap.TryGetValue(droneId, out DroneMover drone))
        {
            // Assign a DroneMover object to the new drone ID
            if (unmappedDroneMovers.Count > 0)
            {
                drone = unmappedDroneMovers[0];
                unmappedDroneMovers.RemoveAt(0);
                drone.Name = $"Drone {droneIdToGameObjectMap.Count + 1}";
                if (onDroneAdded == null)
                {
                    Debug.LogWarning("No listeners subscribed to onDroneAdded event!");
                }
                activeDroneMovers.Add(drone);
                //testing
                selectedDrone = activeDroneMovers[0];
                onDroneAdded?.Invoke(drone);
                droneIdToGameObjectMap.Add(droneId, drone);
            }
            else
            {
                Debug.LogError("No available DroneMover objects to assign.");
                return;
            }
        }

        if (drone != null)
        {
            Debug.Log("droneId recognized");
            drone.SetDroneData(droneData);
        }
    }

    public DroneMover GetSelectedDrone()
    {
        return selectedDrone;
    }

    public void SetSelectedDrone(string droneId)
    {
        if (droneIdToGameObjectMap.TryGetValue(droneId, out DroneMover drone))
        {
            Debug.Log($"Selected drone ID: {droneId}");
            selectedDrone = drone;
        }
    }
    public void NextDrone()
    {
        if(GetNumActiveDrones() == 0)
            return;
        Debug.Log(selectedDroneIndex + " + 1");
        selectedDroneIndex ++;
        if(selectedDroneIndex > activeDroneMovers.Count -1 )
        {
            selectedDroneIndex = 0;
        }
        selectedDrone = activeDroneMovers[selectedDroneIndex];
        Debug.Log(selectedDrone.DID);
    }
    public void PrevDrone()
    {
        if(GetNumActiveDrones() == 0)
            return;
        Debug.Log(selectedDroneIndex + " - 1");
        selectedDroneIndex --;
        if(selectedDroneIndex < 0)
        {
            selectedDroneIndex = activeDroneMovers.Count -1;
        }
        selectedDrone = activeDroneMovers[selectedDroneIndex];
    }
    //to find number of active drones for indexing purposes
    public int GetNumActiveDrones()
    {
        return activeDroneMovers.Count;
    }
}
