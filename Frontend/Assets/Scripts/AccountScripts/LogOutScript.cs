using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro; // For TextMeshPro

public class LogOutScript : MonoBehaviour
{
    [SerializeField] private Button logoutButton;
    [SerializeField] private GameObject signinButton;

    void Start()
    {
        if (logoutButton == null)
            logoutButton = GetComponent<Button>();

        logoutButton.onClick.AddListener(OnLogoutClicked);

        if (signinButton == null)
        {
            Debug.LogWarning("Signin button reference is not set in the inspector!");
            // Optionally try to find it
            signinButton = GameObject.Find("SignIn")?.gameObject;
        }
    }

    void OnLogoutClicked()
    {
        
        if (UserManager.Instance.UserId != -1)
        {
            // SceneManager.LoadScene("StartingScene");
            logoutButton.interactable = false;
            gameObject.SetActive(false);
            UserManager.Instance.Logout();
            Debug.Log("User logged out.");

            signinButton.gameObject.SetActive(true);

        }
        else
        {
            Debug.LogWarning("UserManager not found.");
        }
    }

    void OnDestroy()
    {
        if (logoutButton != null)
            logoutButton.onClick.RemoveListener(OnLogoutClicked);
    }
}