using UnityEngine;
using TMPro;

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
        Debug.Log($"Loading race: {raceID}");
        // TO ADD
    }
}
