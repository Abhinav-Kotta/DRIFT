using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using UnityEngine;
using TMPro;
using System.Linq;
using Newtonsoft.Json.Linq;

public class ReplayListDisplay : MonoBehaviour
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
        string uri = $"{apiUrl}/user_races/{UserManager.Instance.UserId}";
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

            Debug.Log("Response Code: " + uwr.responseCode);
            Debug.Log("Response Text: " + uwr.downloadHandler.text);
            List<ReplayResponse> races = ParseRaceData(uwr.downloadHandler.text);

            if (races == null || races.Count == 0)
            {
                noRacesText.gameObject.SetActive(true);
                yield break;
            }

            noRacesText.gameObject.SetActive(false);
            foreach (var race in races)
            {
                Debug.Log($"Creating card for race: {race.race_id}");
                GameObject newRaceCard = Instantiate(raceCardPrefab, raceListContainer);

                // Assign race details to the card
                RaceCardUI cardUI = newRaceCard.GetComponent<RaceCardUI>();
                if (cardUI != null)
                {
                cardUI.SetRaceInfo(race.race_id, "Completed", race.race_size_bytes, 0);

                // ✅ Hook up the Launch button to LoadRace() in code
                UnityEngine.UI.Button launchButton = newRaceCard.GetComponentInChildren<UnityEngine.UI.Button>();
                if (launchButton != null)
                {
                    launchButton.onClick.RemoveAllListeners(); // Clear previous listeners if any
                    launchButton.onClick.AddListener(cardUI.LoadReplay);
                }
                else
                {
                Debug.LogWarning("[RaceListDisplay] Launch button not found in RaceCard prefab.");
                }
            }
            else
            {
                Debug.LogError("RaceCardUI component missing on prefab");
            }
            }
        }
    }

    private List<ReplayResponse> ParseRaceData(string jsonData)
    {
        List<ReplayResponse> raceList = new List<ReplayResponse>();

        try
        {
            Debug.Log("Sclungart");
            JObject response = JObject.Parse(jsonData);
            JArray racesArray = (JArray)response["races"];
            Debug.Log("Sclungart2");

            if (racesArray != null && racesArray.Count > 0)
            {
                foreach (JObject raceData in racesArray)
                {
                    ReplayResponse race = new ReplayResponse
                    {
                        race_id = raceData["race_id"]?.ToString() ?? "",
                        race_name = raceData["race_name"]?.ToString() ?? "",
                        drift_map = raceData["drift_map"]?.ToString() ?? "",
                        created_at = raceData["created_at"]?.ToString() ?? "",
                        user_id = JArray.Parse(raceData["user_id"]?.ToString() ?? "[]").First?.ToString() ?? "",
                        race_size_bytes = raceData["race_size_bytes"]?.Value<int>() ?? 0
                    };

                    raceList.Add(race);
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
