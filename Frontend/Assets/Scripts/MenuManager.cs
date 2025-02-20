using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    public void StartingGame()
    {
        SceneManager.LoadScene("Game");
    }

    public void ImportFile()
    {
        SceneManager.LoadScene("Import");
    }

    public void LinkDevice()
    {
        SceneManager.LoadScene("Link");
    }

    public void Close()
    {
        Application.Quit();
    }
}
