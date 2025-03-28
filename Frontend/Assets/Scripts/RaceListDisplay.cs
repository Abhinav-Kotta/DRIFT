using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using UnityEngine;
using TMPro;
using System.Linq;
using Newtonsoft.Json.Linq;

public class RaceListDisplay : MonoBehaviour
{
    [SerializeField] private GameObject raceCardPrefab;  // Prefab to create race cards
    [SerializeField] private Transform raceListContainer;  // Panel where cards will appear
    [SerializeField] private TextMeshProUGUI noRacesText;  // UI Text for "No Active Races"

    private string apiUrl;

    void Start()
    {
        apiUrl = ConfigLoader.GetApiUrl();
        if (apiUrl == null)
        {
            Debug.LogError("apiUrl not found");
            return;
        }

        Debug.Log("Starting to fetch race data...");
        StartCoroutine(LoadRaces());
    }

    IEnumerator LoadRaces()
{
    string uri = $"{apiUrl}/list_races";
    Debug.Log("Attempting to connect to: " + uri);

    using (UnityWebRequest uwr = UnityWebRequest.Get(uri))
    {
        uwr.SetRequestHeader("Content-Type", "application/json");
        uwr.SetRequestHeader("Accept", "application/json");

        Debug.Log("Sending web request...");
        yield return uwr.SendWebRequest();
        Debug.Log("Request completed.");

        if (uwr.result == UnityWebRequest.Result.ConnectionError || 
            uwr.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.LogError($"Error: {uwr.error}");
            noRacesText.gameObject.SetActive(true);
            yield break;
        }

        List<RaceResponse> races = ParseRaceData(uwr.downloadHandler.text);

        if (races == null || races.Count == 0)
        {
            noRacesText.gameObject.SetActive(true);
            yield break;
        }

        noRacesText.gameObject.SetActive(false);

        foreach (var race in races)
        {
            Debug.Log($"Creating card for race: {race.race_id}, Status: {race.status}, UDP: {race.udp_port}, WS: {race.ws_port}");
            GameObject newRaceCard = Instantiate(raceCardPrefab, raceListContainer);

            // Assign race details to the card
            RaceCardUI cardUI = newRaceCard.GetComponent<RaceCardUI>();
            if (cardUI != null)
            {
                cardUI.SetRaceInfo(race.race_id, race.status, race.udp_port, race.ws_port);
            }
            else
            {
                Debug.LogError("RaceCardUI component missing on prefab");
            }
        }
    }
}

    private List<RaceResponse> ParseRaceData(string jsonData)
{
    List<RaceResponse> raceList = new List<RaceResponse>();

    try
    {
        JObject response = JObject.Parse(jsonData);
        JArray racesArray = (JArray)response["races"];

        if (racesArray != null && racesArray.Count > 0)
        {
            foreach (JObject raceObj in racesArray)
            {
                JProperty raceProp = raceObj.Properties().FirstOrDefault();
                
                if (raceProp != null) // Ensure it's not null before using it
                {
                    JObject raceData = (JObject)raceProp.Value;

                    RaceResponse race = new RaceResponse
                    {
                        race_id = (string)raceData["race_id"],
                        udp_port = (int)raceData["udp_port"],
                        ws_port = (int)raceData["ws_port"],
                        status = (string)raceData["status"]
                    };

                    raceList.Add(race);
                }
            }
        }
    }
    catch (System.Exception e)
    {
        Debug.LogError($"Error parsing race data: {e.Message}");
    }

    return raceList;
}

}
