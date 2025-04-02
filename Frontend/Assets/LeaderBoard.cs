using UnityEngine;
using TMPro; // If using TextMeshPro

public class LeaderBoard : MonoBehaviour
{
    private DataManager dataManager;
    private TMP_Text leaderboardText; // Use TMP_Text for TextMeshPro, or Text for Unity's default UI Text

    void Start()
    {
        dataManager = DataManager.Instance;

        // Get the TMP_Text or Text component attached to this GameObject
        leaderboardText = GetComponent<TMP_Text>();
        if (leaderboardText == null)
        {
            Debug.LogError("No TMP_Text component found on the LeaderBoard GameObject!");
        }
    }

    void Update()
    {
        if (dataManager != null && leaderboardText != null)
        {
            // Update the leaderboard text
            leaderboardText.text = dataManager.GetLeaderBoardString();
        }
    }
}
