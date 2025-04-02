using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadManager : MonoBehaviour
{
    public void LoadMenu()
    {
        Debug.Log("Loading Starting Scene...");
        SceneManager.LoadScene("StartingScene");
    }

    public void SignUp()
    {
        Debug.Log("Loading SignUp Scene...");
        SceneManager.LoadScene("SignUp");
    }

    public void ForgotPassword()
    {
        Debug.Log("Loading Forgot Password Scene...");
        SceneManager.LoadScene("ForgotPW");
    }

    public void SignIn()
    {
        Debug.Log("Loading SignIn Scene...");
        SceneManager.LoadScene("SignIn");
    }
}
