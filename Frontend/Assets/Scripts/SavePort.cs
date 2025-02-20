using UnityEngine;
using TMPro;

public class SavePort : MonoBehaviour
{
    // Three canvas ui option on-screen
    public TMP_InputField iField;
    public TMP_Text displayText;
    public UnityEngine.UI.Button submitBtn;

    // Port number input from user
    public string portNum;

    void Start()
    {
        submitBtn.onClick.AddListener(GetPort);
    }

    void GetPort()
    {
        portNum = iField.text;

        // Perform checks to see if port number is valid (idk what parameters for validity are)
        if (portNum.Length != 5)
        {
            displayText.text = "Invalid input detected! Please retry";
        }
        else
        {
            displayText.text = "Port not found! Please retry";
        }
    }
}
