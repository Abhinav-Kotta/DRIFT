using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;

public class CreateAccountScript : MonoBehaviour
{
    private Button submitButton;
    private TMP_InputField usernameField;
    private TMP_InputField passwordField;
    private TMP_InputField securityQuestionField;
    private TMP_InputField securityAnswerField;
    private string apiUrl;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        submitButton = GetComponent<Button>();
        submitButton.onClick.AddListener(OnSubmitClicked);
        apiUrl = ConfigLoader.GetApiUrl();
        Debug.Log(apiUrl);

        usernameField = GameObject.Find("Username").GetComponent<TMP_InputField>();
        passwordField = GameObject.Find("Password").GetComponent<TMP_InputField>();
        securityQuestionField = GameObject.Find("SecurityQDropdown").GetComponent<TMP_InputField>();
        securityAnswerField = GameObject.Find("SecurityQInput").GetComponent<TMP_InputField>();

        if (usernameField == null || passwordField == null || securityQuestionField == null || securityAnswerField == null || apiUrl == null)
        {
            Debug.LogError("apiUrl, Username, password, security question, or security answer field not found");
        }
        
    }

    // Update is called once per frame
    void OnSubmitClicked()
    {
        StartCoroutine(CreateAccount());
    }

    IEnumerator CreateAccount()
    {
        string jsonPayload = $"{{\"username\":\"{usernameField.text}\",\"password\":\"{passwordField.text}\",\"security_question\":\"{securityQuestionField.text}\",\"security_answer\":\"{securityAnswerField.text}\"}}";

        UnityWebRequest www = new UnityWebRequest($"{apiUrl}/create_user", "POST");
        
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
