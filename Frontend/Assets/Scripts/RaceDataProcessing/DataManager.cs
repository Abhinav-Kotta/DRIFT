using System;
using System.Collections.Generic;
using UnityEngine;

public class DataManager : MonoBehaviour
{
    private static DataManager instance;
    public static DataManager Instance => instance;

    private Dictionary<int, DroneMover> droneIdToGameObjectMap;
    private DroneMover selectedDrone;

    void Awake()
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

    void Start()
    {
        // Initialize the mapping dictionary
        droneIdToGameObjectMap = new Dictionary<int, DroneMover>();

        // Find the JsonDataGen component in the scene
    }

    public void UpdateDroneData(DroneData droneData)
    {
        // Ensure drone_id is an integer
        int droneId = Convert.ToInt32(droneData.drone_id);

        if (droneIdToGameObjectMap.TryGetValue(droneId, out DroneMover drone))
        {
            drone.UpdateDroneData(droneData.position, droneData.attitude);
        }
    }

    public DroneMover GetSelectedDrone()
    {
        return selectedDrone;
    }

    public void SetSelectedDrone(int droneId)
    {
        if (droneIdToGameObjectMap.TryGetValue(droneId, out DroneMover drone))
        {
            selectedDrone = drone;
        }
    }

    void GenerateAndLogJsonDataForAllDrones()
    {
        foreach (var kvp in droneIdToGameObjectMap)
        {
            int droneID = kvp.Key;
            DroneMover droneObject = kvp.Value;

            if (droneObject != null)
            {

            }
            else
            {
                Debug.LogError($"GameObject for drone ID {droneID} not found.");
            }
        }
    }

    void UpdateDroneObjectWithJsonData(DroneMover droneObject, string jsonData)
    {

    }
}
