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
    private List<DroneMover> droneMovers;
    private DroneMover selectedDrone;

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
        // Initialize the mapping dictionary
        droneIdToGameObjectMap = new Dictionary<string, DroneMover>();
        droneMovers = new List<DroneMover>(FindObjectsOfType<DroneMover>());
    }

    public void UpdateDroneData(DroneData droneData)
    {
        var droneId = droneData.drone_id;

        if (!droneIdToGameObjectMap.TryGetValue(droneId, out DroneMover drone))
        {
            // Assign a DroneMover object to the new drone ID
            if (droneMovers.Count > 0)
            {
                drone = droneMovers[0];
                droneMovers.RemoveAt(0);
                drone.Name = $"Drone {droneIdToGameObjectMap.Count + 1}";
                if (onDroneAdded == null)
                {
                    Debug.LogWarning("No listeners subscribed to onDroneAdded event!");
                }
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
            selectedDrone = drone;
        }
    }
}
