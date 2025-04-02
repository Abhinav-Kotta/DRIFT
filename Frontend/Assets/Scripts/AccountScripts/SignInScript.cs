using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Networking;
using System.Collections;
using UnityEngine.SceneManagement;

public class SignInScript : MonoBehaviour
{
    [SerializeField] private Button signInButton;
    [SerializeField] private TMP_InputField usernameField;
    [SerializeField] private TMP_InputField passwordField;
    [SerializeField] private TMP_Text ErrorMessage;

    private string apiUrl;

    void Start()
    {
        apiUrl = ConfigLoader.GetApiUrl();
        if (string.IsNullOrEmpty(apiUrl))
        {
            Debug.LogError("API URL not found in configuration");
            SetError("Configuration error. Please contact support.");
            return;
        }

        if (signInButton == null)
            signInButton = GetComponent<Button>();
        signInButton.onClick.AddListener(OnSignInClicked);

        if (usernameField == null)
            usernameField = GameObject.Find("UsernameField")?.GetComponent<TMP_InputField>();

        if (passwordField == null)
            passwordField = GameObject.Find("PasswordField")?.GetComponent<TMP_InputField>();

        if (ErrorMessage == null)
        {
            GameObject go = GameObject.Find("ErrorMessage");
            if (go != null)
                ErrorMessage = go.GetComponent<TMP_Text>();
            else
                Debug.LogError("Could not find 'ErrorMessage' GameObject in scene.");
        }

        if (usernameField == null || passwordField == null)
        {
            Debug.LogError("Username or password field not found");
            SetError("UI error. Please contact support.");
        }

        SetError(""); // clear
    }

    void OnSignInClicked()
    {
        if (string.IsNullOrEmpty(usernameField.text) || string.IsNullOrEmpty(passwordField.text))
        {
            SetError("Please enter both username and password.");
            return;
        }

        SetError("Signing in...", Color.yellow);
        signInButton.interactable = false;
        StartCoroutine(SignIn());
    }

    IEnumerator SignIn()
    {
        string jsonPayload = JsonUtility.ToJson(new LoginData(usernameField.text, passwordField.text));
        Debug.Log($"Connecting to: {apiUrl}/login");
        Debug.Log($"Payload: {jsonPayload}");

        using (UnityWebRequest www = new UnityWebRequest($"{apiUrl}/login", "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonPayload);
            www.uploadHandler = new UploadHandlerRaw(bodyRaw);
            www.downloadHandler = new DownloadHandlerBuffer();
            www.SetRequestHeader("Content-Type", "application/json");
            www.timeout = 5;

            yield return www.SendWebRequest();
            signInButton.interactable = true;

            string result = www.downloadHandler.text;

            if (www.result == UnityWebRequest.Result.Success)
            {
                Debug.Log($"Login successful: {result}");
                LoginResponse response = JsonUtility.FromJson<LoginResponse>(result);

                SetError("Login successful!", Color.green);
                Debug.Log($"Extracted user ID: {response.user_id}");

                if (UserManager.Instance != null)
                {
                    UserManager.Instance.SetUserInfo(response.user_id, usernameField.text);
                    Debug.Log("User info set in UserManager");
                    SceneManager.LoadScene("StartingScene");
                }
                else
                {
                    Debug.LogError("UserManager instance not found");
                    SetError("Internal error. Please restart the application.");
                }
            }
            else
            {
                Debug.LogError($"Network error: {www.error}");
                Debug.LogError($"Server response: {result}");

                string detailMessage = "Network or server error.";
                if (!string.IsNullOrEmpty(result) && result.Contains("detail"))
                {
                    int startIndex = result.IndexOf(":") + 2;
                    int endIndex = result.LastIndexOf("\"");
                    if (startIndex > 0 && endIndex > startIndex)
                        detailMessage = result.Substring(startIndex, endIndex - startIndex);
                }

                SetError(detailMessage, Color.red);
            }
        }
    }

    void SetError(string msg, Color? color = null)
    {
        if (ErrorMessage == null)
        {
            Debug.LogError("ErrorMessage is null.");
            return;
        }

        ErrorMessage.text = msg;
        ErrorMessage.color = color ?? Color.red;
    }
}

[System.Serializable]
public class LoginData
{
    public string username;
    public string password;

    public LoginData(string username, string password)
    {
        this.username = username;
        this.password = password;
    }
}

[System.Serializable]
public class LoginResponse
{
    public string status;
    public string message;
    public int user_id;
}
