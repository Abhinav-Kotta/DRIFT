using System;
using UnityEngine;

[ExecuteAlways] // Ensures the script runs in edit mode
public class SpeedMeterVR : MonoBehaviour
{
    [System.Serializable] // Keeps it serializable in Inspector
    public class SpriteData
    {
        public GameObject targetObject; // Assign in Inspector
        public Texture2D texture;  
        //public Vector3 scale = new Vector3(1, 1, 1); // Default scale
    }

    public SpriteData background, numbers, pointer;
    private DataManager dataManager;
    private DroneMover currentDrone;
    private float AltMValue;
    private DroneViewCam cameraController;
    public float velocity;

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
        velocity = currentDrone.CalculateVelocityMagnitude() * 1.94384f;
        Debug.Log($"velocityr: {velocity}");

        // Move the pointer based on AltMValue
        if (pointer.targetObject != null)
        {
            //Vector3 pointerPosition = pointer.targetObject.transform.localPosition;
            

            float clamped = Mathf.Clamp(velocity, 0, 90);
            float rotationZ = clamped * 3.6f; // Map 0-100 to 0-360 degrees

            //pointerPosition.y = AltMValue * 0.1f; // Scale factor for altitude (adjust as needed)

            //pointerPosition.y = Mathf.Clamp(pointerPosition.y, -5f, 5f);

            pointer.targetObject.transform.localRotation = Quaternion.Euler(0, 0, -rotationZ);
        }
    }


    void UpdateSprites()
    {
        ApplySprite("Background", background, 0);
        ApplySprite("Numbers", numbers, 0);
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
           // data.scale = data.targetObject.transform.localScale; // Store the new scale
        }
        else 
        {
            //data.targetObject.transform.localScale = data.scale; // Apply scale in Play mode
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
