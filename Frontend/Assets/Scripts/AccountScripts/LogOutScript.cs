using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro; // For TextMeshPro

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
        
        if (UserManager.Instance.UserId != -1)
        {
            // SceneManager.LoadScene("StartingScene");
            logoutButton.interactable = false;
            gameObject.SetActive(false);
            UserManager.Instance.Logout();
            Debug.Log("User logged out.");
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