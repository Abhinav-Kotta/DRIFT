using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Collections;
using System.Text.RegularExpressions;
using TMPro;
using System.Collections.Generic;

[System.Serializable]
public class RootResponse
{
    public string status;
    public Race race;
}

[System.Serializable]
public class Race
{
    public string race_id;
    public string race_name;
    public string drift_map;
    public string flight_packet;
}

public class RaceReplayResponse
{
    public string raceID;
    public string raceName;
    public string driftMap;
    public DroneData[] packets;
}

public static class JsonHelper
{
    public static T[] FromJson<T>(string json)
    {
        Wrapper<T> wrapper = JsonUtility.FromJson<Wrapper<T>>(json);
        return wrapper.Items;
    }

    public static string FixJsonArray(string raw)
    {
        return "{\"Items\":" + raw + "}";
    }

    [System.Serializable]
    private class Wrapper<T>
    {
        public T[] Items;
    }
}

public class RaceReplayer : MonoBehaviour
{
    private string baseApiUrl;
    public string raceID;
    [SerializeField] public bool isPaused = true;

    private DroneData[] replayPackets;
    private Coroutine replayCoroutine;
    private int currentIndex = 0;
    private float replayStartTime;

    public Slider replaySlider;
    public float replayDuration; // Total duration in seconds
    private bool isSliderBeingDragged = false;
    private bool wasPlayingBeforeDrag = false;
    public TMPro.TextMeshProUGUI timestampDisplay;
    private bool replayFinished = false;

    public void OnSliderDragStart()
    {
        if (!isPaused) // Only pause if it was moving
        {
            wasPlayingBeforeDrag = true;
            TogglePause(); // Pause
        }
        else
        {
            wasPlayingBeforeDrag = false;
        }

        isSliderBeingDragged = true;
    }


    public void OnSliderDragEnd()
    {
        isSliderBeingDragged = false;

        if (replaySlider == null || replayPackets == null) return;

        JumpToTime(replaySlider.value);

        if (wasPlayingBeforeDrag)
        {
            TogglePause(); // Resume
        }
    }

    public void OnSliderChanged(float value)
    {
        if (isSliderBeingDragged)
        {
            JumpToTime(value);
        }
    }


    public void TogglePause()
    {
        isPaused = !isPaused;
        Debug.Log("Toggling pause state: " + isPaused);

        if (!isPaused && replayCoroutine == null)
        {
            float flightStartTime = replayPackets[0].timestamp;
            float currentPacketTime = replayPackets[currentIndex].timestamp;
            replayStartTime = Time.realtimeSinceStartup - (currentPacketTime - flightStartTime);
            replayCoroutine = StartCoroutine(PlayReplay());
        }
    }

    public void JumpToTime(float targetTime)
    {
        if (replayPackets == null || replayPackets.Length == 0) return;

        int index = FindClosestPacketIndex(replayPackets, targetTime);
        currentIndex = index;

        float flightStartTime = replayPackets[0].timestamp;
        float currentPacketTime = replayPackets[currentIndex].timestamp;
        replayStartTime = Time.realtimeSinceStartup - (currentPacketTime - flightStartTime);

        if (!isPaused && replayCoroutine == null)
            replayCoroutine = StartCoroutine(PlayReplay());

        // Move the drone to that timestamp
        if (isPaused && DataManager.Instance != null)
            DataManager.Instance.UpdateDroneData(replayPackets[currentIndex]);

        if (timestampDisplay != null)
        {
            int curMin = Mathf.FloorToInt(currentPacketTime / 60f);
            int curSec = Mathf.FloorToInt(currentPacketTime % 60f);

            int totalMin = Mathf.FloorToInt(replayDuration / 60f);
            int totalSec = Mathf.FloorToInt(replayDuration % 60f);

            timestampDisplay.text = $"{curMin:D2}:{curSec:D2} / {totalMin:D2}:{totalSec:D2}";
        }

        if (!isPaused || replayFinished)
        {
            replayFinished = false;
            replayCoroutine = StartCoroutine(PlayReplay());
        }
    }

    int FindClosestPacketIndex(DroneData[] packets, float targetTime)
    {
        int left = 0, right = packets.Length - 1;
        while (left < right)
        {
            int mid = (left + right) / 2;
            if (packets[mid].timestamp < targetTime)
                left = mid + 1;
            else
                right = mid;
        }
        return left;
    }

    void Start()
    {
        Debug.Log("Replay data loading....");
        StartCoroutine(LoadReplayData(raceID));
    }

    void PrintDroneData(DroneData data)
    {
        Debug.Log($"\n--- Drone Packet ---\n" +
            $"ID: {data.drone_id}\n" +
            $"Timestamp: {data.timestamp}\n" +
            $"Position: x={data.position.x}, y={data.position.y}, z={data.position.z}\n" +
            $"Attitude: x={data.attitude.x}, y={data.attitude.y}, z={data.attitude.z}, w={data.attitude.w}\n" +
            $"Velocity: x={data.velocity.x}, y={data.velocity.y}, z={data.velocity.z}\n" +
            $"Gyro: pitch={data.gyro.pitch}, roll={data.gyro.roll}, yaw={data.gyro.yaw}\n" +
            $"Inputs: throttle={data.inputs.throttle}, yaw={data.inputs.yaw}, pitch={data.inputs.pitch}, roll={data.inputs.roll}\n" +
            $"Battery: percentage={data.battery.percentage}, voltage={data.battery.voltage}\n" +
            $"Motor Count: {data.motor_count}\n" +
            $"Motor RPMs: {(data.motor_rpms != null ? string.Join(", ", data.motor_rpms) : "null")}\n");
    }

    IEnumerator LoadReplayData(string raceID)
    {
        replayFinished = false;
        baseApiUrl = ConfigLoader.GetApiUrl();
        UnityWebRequest request = UnityWebRequest.Get(baseApiUrl + "/replay_race/" + raceID);
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.LogError("Error loading replay data: " + request.error);
        }
        else
        {
            string jsonData = request.downloadHandler.text;
            RootResponse root = JsonUtility.FromJson<RootResponse>(jsonData);
            string fixedRaw = JsonHelper.FixJsonArray(root.race.flight_packet);
            string[] rawPackets = JsonHelper.FromJson<string>(fixedRaw);

            replayPackets = new DroneData[rawPackets.Length];
            for (int i = 0; i < rawPackets.Length; i++)
            {
                string raw = rawPackets[i].Replace("\\n", "").Replace("\\\"", "\"").Trim();
                raw = Regex.Replace(raw, "\"drone_id\":\\s*\\[\\s*\"(.*?)\"\\s*,\\s*(\\d+)\\s*\\]", "\"drone_id\": \"$1:$2\"");
                replayPackets[i] = JsonUtility.FromJson<DroneData>(raw);
            }

            Debug.Log("Race name: " + root.race.race_name);
            Debug.Log("Packets count: " + replayPackets.Length);

            Dictionary<string, float> droneOffsets = new();
            float runningMaxTime = 0f;

            for (int i = 0; i < replayPackets.Length; i++)
            {
                var packet = replayPackets[i];

                if (!droneOffsets.ContainsKey(packet.drone_id))
                {
                    // Offset this drone's timeline to match when it first appears
                    droneOffsets[packet.drone_id] = runningMaxTime - packet.timestamp;
                }

                // Apply offset
                packet.timestamp += droneOffsets[packet.drone_id];

                // Update max seen timestamp so far
                runningMaxTime = Mathf.Max(runningMaxTime, packet.timestamp);
            }

            for (int i = 0; i < replayPackets.Length; i++)
            {
                PrintDroneData(replayPackets[i]);
            }


            if (DataManager.Instance == null)
            {
                Debug.LogError("DataManager.Instance is still null at this point in time!");
            }

            replayDuration = replayPackets[replayPackets.Length - 1].timestamp;
            if (replaySlider != null)
            {
                replaySlider.minValue = 0;
                replaySlider.maxValue = replayDuration;
                replaySlider.onValueChanged.AddListener(OnSliderChanged);
            }

            currentIndex = 0;
            DataManager.Instance.UpdateDroneData(replayPackets[0]);            
        }
    }

    IEnumerator PlayReplay()
    {
        if (replayPackets == null || replayPackets.Length == 0) yield break;

        float flightStartTime = replayPackets[0].timestamp;

        while (currentIndex < replayPackets.Length)
        {
            DroneData packet = replayPackets[currentIndex];
            float targetTime = replayStartTime + (packet.timestamp - flightStartTime);

            // Wait until the right time or until paused
            while (Time.realtimeSinceStartup < targetTime || isPaused)
            {
                if (isPaused)
                {
                    replayStartTime += Time.unscaledDeltaTime;
                }

                if (!isSliderBeingDragged && replaySlider != null)
                {
                    float currentTime = Time.realtimeSinceStartup - replayStartTime + replayPackets[0].timestamp;
                    replaySlider.value = currentTime;

                    if (timestampDisplay != null)
                    {
                        int curMin = Mathf.FloorToInt(currentTime / 60f);
                        int curSec = Mathf.FloorToInt(currentTime % 60f);

                        int totalMin = Mathf.FloorToInt(replayDuration / 60f);
                        int totalSec = Mathf.FloorToInt(replayDuration % 60f);

                        timestampDisplay.text = $"{curMin:D2}:{curSec:D2} / {totalMin:D2}:{totalSec:D2}";
                    }
                }

                yield return null;

                // Refresh packet in case JumpToTime() changed currentIndex
                packet = replayPackets[currentIndex];
                targetTime = replayStartTime + (packet.timestamp - flightStartTime);
            }

            DataManager.Instance?.UpdateDroneData(packet);

            if (!isSliderBeingDragged && replaySlider != null)
            {
                replaySlider.value = packet.timestamp;
            }

            currentIndex++;
        }

        replayFinished = true;
    }
}
