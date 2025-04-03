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

    public void SignIn()
    {
        if(UserManager.Instance.UserId == -1)
            SceneManager.LoadScene("SignIn");
        else
            UserManager.Instance.Logout();
        
    }

    public void Replay()
    {
        if(UserManager.Instance.UserId != -1)
            SceneManager.LoadScene("ListReplays");
    }
}
