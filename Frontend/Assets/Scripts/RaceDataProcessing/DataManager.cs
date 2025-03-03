using System;
using System.Collections.Generic;
using UnityEngine;

public class DataManager : MonoBehaviour
{
    private static DataManager instance;
    public static DataManager Instance => instance;

    private Dictionary<String, DroneMover> droneIdToGameObjectMap;
    
    private List<DroneMover> droneMovers;
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
        droneIdToGameObjectMap = new Dictionary<String, DroneMover>();
        droneMovers = new List<DroneMover>(FindObjectsOfType<DroneMover>());
            //FindObjectsOfType<DroneMover>());
    }

    public void UpdateDroneData(DroneData droneData)
    {
        //Debug.Log(droneData.drone_id);
        String droneId = droneData.drone_id.ToString();
        //Debug.Log(droneId);

        if (!droneIdToGameObjectMap.TryGetValue(droneId, out DroneMover drone))
        {
            // Assign a DroneMover object to the new drone ID
            if (droneMovers.Count > 0)
            {
                drone = droneMovers[0];
                droneMovers.RemoveAt(0);
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

    public void SetSelectedDrone(String droneId)
    {
        if (droneIdToGameObjectMap.TryGetValue(droneId, out DroneMover drone))
        {
            selectedDrone = drone;
        }
    }

    // void GenerateAndLogJsonDataForAllDrones()
    // {
    //     foreach (var kvp in droneIdToGameObjectMap)
    //     {
    //         String droneID = kvp.Key;
    //         DroneMover droneObject = kvp.Value;

    //         if (droneObject != null)
    //         {

    //         }
    //         else
    //         {
    //             Debug.LogError($"GameObject for drone ID {droneID} not found.");
    //         }
    //     }
    // }

    // void UpdateDroneObjectWithJsonData(DroneMover droneObject, string jsonData)
    // {

    // }
}
