using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;


public class RaceCardUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI raceNameText;
    [SerializeField] private TextMeshProUGUI raceStatusText;
    [SerializeField] private TextMeshProUGUI udpPortText;
    [SerializeField] private TextMeshProUGUI wsPortText;

    private string raceID;

    public void SetRaceInfo(string id, string status, int udpPort, int wsPort)
    {
        raceID = id;
        raceNameText.text = $"Race ID: {id.Substring(0, 8)}...";
        udpPortText.text = $"UDP Port: {udpPort}";
        wsPortText.text = $"WS Port: {wsPort}";
        raceStatusText.text = $"Status: {status}";
    }

    // TO ADD: Will handle button behavior to load race
    public void LoadRace()
    {
        Debug.Log("[RaceCardUI] LoadRace called.");

        // Extract the string values from the TMP text fields
        string udpPort = udpPortText.text.Replace("UDP Port: ", "");
        string status = raceStatusText.text.Replace("Status: ", "");

        Debug.Log($"[RaceCardUI] Parsed values:");
        Debug.Log($"  - UDP Port: {udpPort}");
        Debug.Log($"  - Race ID: {raceID}");
        Debug.Log($"  - Status: {status}");

        // Store values in PlayerPrefs
        PlayerPrefs.SetString("udpPort", udpPort);
        PlayerPrefs.SetString("raceID", raceID);
        PlayerPrefs.SetString("status", status);
        PlayerPrefs.Save();

        Debug.Log("[RaceCardUI] PlayerPrefs saved. Loading SampleScene...");

        // Load the Sample Scene
        SceneManager.LoadScene("SampleScene");
    }

}