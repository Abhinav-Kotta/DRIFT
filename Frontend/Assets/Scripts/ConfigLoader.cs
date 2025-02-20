using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class Config
{
    public string apiUrl;
    public string apiKey;
}

public class ConfigLoader : MonoBehaviour
{
    private static Config config;

    static ConfigLoader()
    {
        LoadConfig();
    }

    private static void LoadConfig()
    {
        TextAsset jsonFile = Resources.Load<TextAsset>("config");
        if (jsonFile == null)
        {
            Debug.LogError("Config file not found!");
            return;
        }

        config = JsonUtility.FromJson<Config>(jsonFile.text);
    }

    public static string GetApiUrl() => config?.apiUrl;
}
