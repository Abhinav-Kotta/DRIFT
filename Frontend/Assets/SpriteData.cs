using UnityEngine;

[System.Serializable]
public class SpriteData
{
    public GameObject targetObject; // Assign manually in the Inspector
    public Texture2D texture;  // Texture for the sprite
    public Vector3 scale = new Vector3(1, 1, 1); // Default scale
}
