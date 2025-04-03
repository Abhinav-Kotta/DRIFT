using UnityEngine.SceneManagement;
using UnityEngine.Networking;
using UnityEngine;
using System.Collections;

public class showOptions : MonoBehaviour
{
    private string apiUrl;

    void Start()
    {
        apiUrl = ConfigLoader.GetApiUrl();
    }

    public void ExitRace()
    {
        SceneManager.UnloadScene(SceneManager.GetActiveScene().buildIndex);
        SceneManager.LoadScene("Import");
    }

    public void Delete()
    {
        StartCoroutine(DeleteRace());
    }

    IEnumerator DeleteRace()
    {
        string url = $"{apiUrl}/delete_race/{PlayerPrefs.GetString("raceID", string.Empty)}/{UserManager.Instance.UserId}";

        using (UnityWebRequest uwr = UnityWebRequest.Delete(url))
        {
            uwr.SetRequestHeader("Content-Type", "application/json");
            uwr.SetRequestHeader("Accept", "application/json");

            yield return uwr.SendWebRequest();

            if (uwr.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"Error: {uwr.error}");
                yield break;
            }
            else
            {
                SceneManager.UnloadScene(SceneManager.GetActiveScene().buildIndex);
                Debug.Log("Race deleted successfully.");
                SceneManager.LoadScene("ListReplays");
            }
        }
    }
}
