using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;

public class ForgotPassScript : MonoBehaviour
{
    private Button submitButton;
    private TMP_InputField usernameField;
    private TMP_InputField securityAnswerField;
    private TMP_InputField newPasswordField;
    private string apiUrl;

    void Start()
    {

        submitButton = GetComponent<Button>();
        submitButton.onClick.AddListener(OnSubmitClicked);
        apiUrl = ConfigLoader.GetApiUrl();

        usernameField = GameObject.Find("Username").GetComponent<TMP_InputField>();
        securityAnswerField = GameObject.Find("SecurityA").GetComponent<TMP_InputField>();
        newPasswordField = GameObject.Find("NewPassword").GetComponent<TMP_InputField>();

        if (usernameField == null || securityAnswerField == null || newPasswordField == null || apiUrl == null)
        {
            Debug.LogError("apiUrl, Username, security answer, or new password field not found");
        }
    }

    void OnSubmitClicked()
    {
        StartCoroutine(ForgotPass());
    }

    IEnumerator ForgotPass()
    {
        string jsonPayload = $"{{\"username\":\"{usernameField.text}\",\"security_answer\":\"{securityAnswerField.text}\",\"new_password\":\"{newPasswordField.text}\"}}";

        UnityWebRequest www = new UnityWebRequest($"{apiUrl}/reset_password", "POST");
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
