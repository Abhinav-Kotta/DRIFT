using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

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

    void Start()
    {
        Debug.Log("Replay data loading....");
        StartCoroutine(LoadReplayData(raceID));
    }

    IEnumerator LoadReplayData(string raceID)
    {
        baseApiUrl = ConfigLoader.GetApiUrl();
        Debug.Log("aaa: " + baseApiUrl + "/replay_race/" + raceID);
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

            // Parse string[] from flight_packet (it's a JSON array of escaped strings)
            string fixedRaw = JsonHelper.FixJsonArray(root.race.flight_packet);
            string[] rawPackets = JsonHelper.FromJson<string>(fixedRaw);

            DroneData[] packets = new DroneData[rawPackets.Length];
            for (int i = 0; i < rawPackets.Length; i++)
            {
                string cleanJson = rawPackets[i]
                    .Replace("\\n", "")
                    .Replace("\\\"", "\"")
                    .Trim();

                packets[i] = JsonUtility.FromJson<DroneData>(cleanJson);
            }

            RaceReplayResponse replayData = new RaceReplayResponse
            {
                raceID = root.race.race_id,
                raceName = root.race.race_name,
                driftMap = root.race.drift_map,
                packets = packets
            };

            Debug.Log("Race name: " + replayData.raceName);
            Debug.Log("Packets count: " + replayData.packets.Length);

            for (int i = 0; i < replayData.packets.Length; i++)
            {
                PrintDroneData(replayData.packets[i]);
                yield return new WaitForSecondsRealtime(0.05f);
            }
        }
    }
}
