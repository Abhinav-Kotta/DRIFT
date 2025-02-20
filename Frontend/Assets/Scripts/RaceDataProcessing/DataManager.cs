using System.Collections.Generic;
using UnityEngine;

public class DataManager : MonoBehaviour
{
    private JsonDataGen jsonDataGen;
    private Dictionary<int, GameObject> droneIdToGameObjectMap;

    void Start()
    {
        // Initialize the mapping dictionary
        droneIdToGameObjectMap = new Dictionary<int, GameObject>();

        // Find the JsonDataGen component in the scene
        jsonDataGen = FindFirstObjectByType<JsonDataGen>();
        //replace with json inputer

        if (jsonDataGen != null)
        {
            // Example: Map drone IDs to specific GameObjects in the scene
            droneIdToGameObjectMap[0] = GameObject.Find("mini-drone0");
            droneIdToGameObjectMap[1] = GameObject.Find("mini-drone1");
            droneIdToGameObjectMap[2] = GameObject.Find("mini-drone2");

            // Start repeating the data generation and updating process
            InvokeRepeating("GenerateAndLogJsonDataForAllDrones", 0f, 1f); // Repeat every 1 second
        }
        else
        {
            Debug.LogError("JsonDataGen component not found in the scene.");
        }
    }

    void GenerateAndLogJsonDataForAllDrones()
    {
        foreach (var kvp in droneIdToGameObjectMap)
        {
            int droneID = kvp.Key;
            GameObject droneObject = kvp.Value;

            if (droneObject != null)
            {
                string jsonData = jsonDataGen.GenerateAndLogJsonData(droneID);
                // Update the corresponding GameObject with the generated data
                UpdateDroneObjectWithJsonData(droneObject, jsonData);
            }
            else
            {
                Debug.LogError($"GameObject for drone ID {droneID} not found.");
            }
        }
    }

    void UpdateDroneObjectWithJsonData(GameObject droneObject, string jsonData)
    {
        // Parse the position from the JSON data
        Vector3 position = ParsePositionFromJson(jsonData);
        // Update the GameObject's position
        droneObject.transform.position = position;
        Debug.Log($"Updated {droneObject.name} position to: {position}");
    }

    Vector3 ParsePositionFromJson(string jsonData)
    {
        JsonData data = JsonUtility.FromJson<JsonData>(jsonData);
        Position position = data.StreamFormat.Position;
        return new Vector3(position.PositionX, position.PositionY, position.PositionZ);
    }
}
