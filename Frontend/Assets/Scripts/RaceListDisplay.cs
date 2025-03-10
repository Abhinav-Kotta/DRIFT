using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RaceListDisplay : MonoBehaviour
{
    public TextMeshProUGUI raceListText;  // Assign in the Inspector
    private RaceClient raceClient;

    void Start()
    {
        raceClient = FindObjectOfType<RaceClient>(); // Find the existing RaceClient
        StartCoroutine(LoadRaces());
    }

    IEnumerator LoadRaces()
    {
        List<RaceResponse> races = new List<RaceResponse>(); // Initialize list

        yield return raceClient.ListRaces().ContinueWith(task => 
        {
            if (task.Result != null) 
            {
                races.Add(task.Result);
            }
        });

        if (races.Count == 0)
        {
            raceListText.text = "No races available.";
            yield break;
        }

        raceListText.text = "Available Races:\n";
        foreach (var race in races)
        {
            raceListText.text += $"- Race ID: {race.race_id}, Status: {race.status}\n";
        }
    }
}
