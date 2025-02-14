using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;

public class SignInScript : MonoBehaviour
{
    private Button signInButton;
    private TMP_InputField usernameField;
    private TMP_InputField passwordField;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

        signInButton = GetComponent<Button>();
        signInButton.onClick.AddListener(OnSignInClicked);

        usernameField = GameObject.Find("UsernameField").GetComponent<TMP_InputField>();
        passwordField = GameObject.Find("PasswordField").GetComponent<TMP_InputField>();

        if (usernameField == null || passwordField == null)
        {
            Debug.LogError("Username or password field not found");
        }
       
    }

    // Update is called once per frame
    void OnSignInClicked()
    {
        StartCoroutine(SignIn());
    }

    IEnumerator SignIn()
    {
        string jsonPayload = $"{{\"username\":\"{usernameField.text}\",\"password\":\"{passwordField.text}\"}}";

        UnityWebRequest www = new UnityWebRequest("http://34.68.252.128:8000/login", "POST");
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonPayload);
        www.uploadHandler = new UploadHandlerRaw(bodyRaw);
        www.downloadHandler = new DownloadHandlerBuffer();
        www.SetRequestHeader("Content-Type", "application/json");

        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError(www.error);
        }
        else
        {
            string result = www.downloadHandler.text;
            Debug.Log(result);
        }
    }
}
