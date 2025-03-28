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
    [SerializeField] private TextMeshProUGUI errorText;
    
    private string apiUrl;
    
    void Start()
    {
        // Get API URL from config
        apiUrl = ConfigLoader.GetApiUrl();
        if (string.IsNullOrEmpty(apiUrl))
        {
            Debug.LogError("API URL not found in configuration");
            if (errorText != null)
                errorText.text = "Configuration error. Please contact support.";
            return;
        }
        
        // If button not assigned via Inspector, try to find it
        if (signInButton == null)
            signInButton = GetComponent<Button>();
            
        signInButton.onClick.AddListener(OnSignInClicked);

        // If input fields not assigned via Inspector, try to find them
        if (usernameField == null)
            usernameField = GameObject.Find("UsernameField")?.GetComponent<TMP_InputField>();
            
        if (passwordField == null)
            passwordField = GameObject.Find("PasswordField")?.GetComponent<TMP_InputField>();

        if (usernameField == null || passwordField == null)
        {
            Debug.LogError("Username or password field not found");
            if (errorText != null)
                errorText.text = "UI error. Please contact support.";
        }
        
        // Clear any previous error message
        if (errorText != null)
            errorText.text = "";
    }

    void OnSignInClicked()
    {
        // Check if fields are empty
        if (string.IsNullOrEmpty(usernameField.text) || string.IsNullOrEmpty(passwordField.text))
        {
            if (errorText != null)
                errorText.text = "Please enter both username and password.";
            return;
        }
        
        // Show loading state
        if (errorText != null)
            errorText.text = "Signing in...";
            
        signInButton.interactable = false;
        
        StartCoroutine(SignIn());
    }

    IEnumerator SignIn()
    {
        // Prepare login data
        string jsonPayload = JsonUtility.ToJson(new LoginData(usernameField.text, passwordField.text));
        
        // Log the URL being used (for debugging purposes)
        Debug.Log($"Connecting to: {apiUrl}/login");
        Debug.Log($"Payload: {jsonPayload}");

        // Create web request
        using (UnityWebRequest www = new UnityWebRequest($"{apiUrl}/login", "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonPayload);
            www.uploadHandler = new UploadHandlerRaw(bodyRaw);
            www.downloadHandler = new DownloadHandlerBuffer();
            www.SetRequestHeader("Content-Type", "application/json");
            
            // Add timeout (5 seconds)
            www.timeout = 5;
            
            // Send request
            yield return www.SendWebRequest();

            // Re-enable button after response received
            signInButton.interactable = true;
            
            // Process response
            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"Login error: {www.error}");
                Debug.LogError($"Response code: {www.responseCode}");
                Debug.LogError($"Response: {www.downloadHandler.text}");
                
                // Provide more specific error messages based on the error type
                if (www.error.Contains("111") || www.error.Contains("Connect call failed"))
                {
                    if (errorText != null)
                        errorText.text = "Cannot connect to server. Please check your internet connection.";
                }
                else if (www.responseCode == 401)
                {
                    if (errorText != null)
                        errorText.text = "Invalid username or password.";
                }
                else
                {
                    if (errorText != null)
                        errorText.text = $"Login failed. Please try again.";
                }
            }
            else
            {
                string result = www.downloadHandler.text;
                Debug.Log($"Login successful: {result}");
                
                try
                {
                    // Parse the response
                    LoginResponse response = JsonUtility.FromJson<LoginResponse>(result);
                    
                    // Check if login was successful based on status field
                    if (response.status == "success")
                    {
                        // Extract the user ID (now directly in user_id field)
                        int userId = response.user_id;
                        Debug.Log($"Extracted user ID: {userId}");
                        
                        // Store user info
                        if (UserManager.Instance != null)
                        {
                            UserManager.Instance.SetUserInfo(userId, usernameField.text);
                            Debug.Log("User info set in UserManager");
                            
                            // Navigate to main screen or home page
                            SceneManager.LoadScene("Scenes/SampleScene");
                        }
                        else
                        {
                            Debug.LogError("UserManager instance not found");
                            if (errorText != null)
                                errorText.text = "Internal error. Please restart the application.";
                        }
                    }
                    else
                    {
                        Debug.LogError($"Login failed: {response.message}");
                        if (errorText != null)
                            errorText.text = response.message;
                    }
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"Error parsing response: {e.Message}");
                    Debug.LogError($"Raw response: {result}");
                    if (errorText != null)
                        errorText.text = "Server returned an invalid response.";
                }
            }
        }
    }
}

// Helper classes for serialization
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
    public int user_id;  // Changed from UserId object to direct int
}