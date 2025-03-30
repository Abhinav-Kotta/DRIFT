using UnityEngine;
using TMPro; // Required for TextMeshPro components
using System.Collections.Generic;

public class LeaderBoardTesting : MonoBehaviour
{
    // Reference to the TextMeshProUGUI element for displaying the leaderboard
    private TextMeshProUGUI leaderboardText;

    // Reference to the DataManager instance
    private DataManager dataManager;

    void Start()
    {
        // Get the DataManager instance
        dataManager = DataManager.Instance;

        if (dataManager == null)
        {
            Debug.LogError("DataManager instance is not found in the scene!");
        }

        // Find the TextMeshProUGUI component dynamically if it's not on the same GameObject
        if (leaderboardText == null)
        {
            leaderboardText = GetComponentInChildren<TextMeshProUGUI>(); // Searches child objects
            if (leaderboardText == null)
            {
                leaderboardText = GameObject.Find("LeaderboardText")?.GetComponent<TextMeshProUGUI>(); // Searches by name
            }

            if (leaderboardText == null)
            {
                Debug.LogError("TextMeshProUGUI component is not attached to the same GameObject or could not be found!");
            }
        }
    }

    void Update()
    {
        // Update the leaderboard every frame (or you can trigger this based on an event)
        UpdateLeaderBoardUI();
    }

    private void UpdateLeaderBoardUI()
    {
        if (dataManager == null || leaderboardText == null)
        {
            return;
        }

        // Get the leaderboard string from DataManager
        string leaderboardString = dataManager.GetLeaderBoardString();

        // Update the TextMeshProUGUI component with the leaderboard string
        leaderboardText.text = leaderboardString;
    }
}
