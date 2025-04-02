using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LogOutScript : MonoBehaviour
{
    [SerializeField] private Button logoutButton;

    void Start()
    {
        if (logoutButton == null)
            logoutButton = GetComponent<Button>();

        logoutButton.onClick.AddListener(OnLogoutClicked);
    }

    void OnLogoutClicked()
    {
        if (UserManager.Instance != null)
        {
            UserManager.Instance.Logout();
            Debug.Log("User logged out.");

            // Optionally return to login screen
            SceneManager.LoadScene("StartingScene");
        }
        else
        {
            Debug.LogWarning("UserManager not found.");
        }
    }
}