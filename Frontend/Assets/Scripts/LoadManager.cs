using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadManager : MonoBehaviour
{
    public void LoadMenu()
    {
        Debug.Log("Button pressed - loading SampleScene");
        SceneManager.LoadScene("StartingScene");
    }
}
