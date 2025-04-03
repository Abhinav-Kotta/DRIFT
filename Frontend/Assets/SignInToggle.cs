using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using NUnit.Framework.Internal; // For TextMeshPro

public class SignInToggle : MonoBehaviour
{
    [SerializeField] private Button signinButton;
    public GameObject RaceReplay;
    private bool moved;
    public GameObject WatchRace;
    public GameObject text;
    void Start()
    {
        moved = true;
        if (signinButton == null)
            signinButton = GetComponent<Button>();

        // else
        // {
        //     signinButton.interactable = true;
        //     signinButton.onClick.AddListener(OnSignInClicked);
        // }
    }
    void Update()
    {
        if (UserManager.Instance.UserId != -1)
        {
            if (!moved)
            {
                // Add an offset of -87 to the y position
                RectTransform rectTransform = WatchRace.GetComponent<RectTransform>();
                if (rectTransform != null)
                {
                    rectTransform.anchoredPosition = new Vector2(
                        rectTransform.anchoredPosition.x,
                        rectTransform.anchoredPosition.y + 87 // Offset by -87
                    );
                }

                moved = true;
            }

            RaceReplay.SetActive(true);
            text.GetComponent<TextMeshProUGUI>().text = "Sign Out";
            Debug.Log("User already logged in. Sign-in button disabled.");
        }
        else
        {
            if (moved)
            {
                // Add an offset of +87 to the y position
                RectTransform rectTransform = WatchRace.GetComponent<RectTransform>();
                if (rectTransform != null)
                {
                    rectTransform.anchoredPosition = new Vector2(
                        rectTransform.anchoredPosition.x,
                        rectTransform.anchoredPosition.y - 70 // Offset by +87
                    );
                }

                moved = false;
            }

            RaceReplay.SetActive(false);
            text.GetComponent<TextMeshProUGUI>().text = "Sign In";
        }
    }
}