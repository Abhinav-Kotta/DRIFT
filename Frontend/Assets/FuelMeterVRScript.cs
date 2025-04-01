using System;
using UnityEngine;

[ExecuteAlways] // Ensures the script runs in edit mode
public class FuelMeterVRScript : MonoBehaviour
{
    [System.Serializable] // Keeps it serializable in Inspector
    public class SpriteData
    {
        public GameObject targetObject; // Assign in Inspector
        public Texture2D texture;  
        public Vector3 scale = new Vector3(1, 1, 1); // Default scale
    }

    public SpriteData background, fuel, pointer; // Serialized fields
    private DataManager dataManager;
    private DroneMover currentDrone;
    public float battery;
    private DroneViewCam cameraController;

    void Start()
    {
        UpdateSprites();
        dataManager = DataManager.Instance;
        cameraController = FindObjectOfType<DroneViewCam>();
    }

    void Update()
    {
        if (!Application.isPlaying) return; // Prevents execution in Edit Mode

        if (dataManager == null)
        {
            Debug.LogError("DataManager instance is null. Ensure DataManager is initialized before accessing it.");
            return;
        }

        currentDrone = dataManager.GetSelectedDrone();
        if (currentDrone == null)
        {
            Debug.Log("No drone selected or data manager not initialized.");
            return;
        }

        // Update altitude value
        battery = currentDrone.BatteryPercentage;

        // Move the pointer based on AltMValue
        if (pointer.targetObject != null)
        {
            // float normalizedBattery = battery * 100;
            Debug.Log($"battery percentage: {battery}");
            
            float clamped = Mathf.Clamp(battery, 0f, 1f);
            
            // 100% 235
            // float angle = Mathf.Lerp(0f, 235f, clamped);

            // 50% 0, 45% 45, 10% 200, 0% 240
            float angle = (Mathf.Lerp(0f, 245f, battery) - 128) * -1;
            // print($"angle: {angle}, {clamped}");
    
            // if (clamped <= 0.2f) {
            //     angle = Mathf.Lerp(-180f, -270f, clamped / 0.2f);
            // } else {
            //     angle = Mathf.Lerp(90f, 180f, (clamped - 0.2f) / 0.8f);
            // }
            
            pointer.targetObject.transform.rotation = Quaternion.Euler(0, 0, angle);
        }
    }


    void UpdateSprites()
    {
        ApplySprite("Background", background, 0);
        ApplySprite("Numbers", fuel, 0);
        //ApplySprite("Pointer", pointer, 1); // Pointer on top
    }

    void ApplySprite(string name, SpriteData data, int sortingOrder)
    {
        if (data.texture == null || data.targetObject == null) return;

        SpriteRenderer renderer = data.targetObject.GetComponent<SpriteRenderer>();
        if (renderer == null)
            renderer = data.targetObject.AddComponent<SpriteRenderer>();

        // Assign texture and sorting order
        renderer.sprite = TextureToSprite(data.texture);
        renderer.sortingOrder = sortingOrder;

        // Preserve manual scale changes made in the Inspector
        if (!Application.isPlaying) 
        {
            data.scale = data.targetObject.transform.localScale; // Store the new scale
        }
        else 
        {
            data.targetObject.transform.localScale = data.scale; // Apply scale in Play mode
        }
    }


    Sprite TextureToSprite(Texture2D tex)
    {
        return Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
    }

    void OnValidate() // Runs in editor when values change
    {
        if (!Application.isPlaying)
        {
            UpdateSprites();
        }
    }
}
