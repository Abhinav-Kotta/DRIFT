using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class StatManager : MonoBehaviour
{
    // Toggle stats visibility
    [SerializeField] public bool showStats = true;

    // Individual stat toggles
    [SerializeField] public bool showVelocity = true;
    [SerializeField] public bool showPitch = true;
    [SerializeField] public bool showRoll = true;
    [SerializeField] public bool showYaw = true;
    [SerializeField] public bool showAcceleration = true;
    [SerializeField] public bool showStickInput = true;

    // Simulated stick input values (-1 to 1 range) DELETE REMOVE REVISE TEMPORARY
    [SerializeField]
    public float yawInput = 0;  // Controls StickCircleX (Yaw)

    [SerializeField]
    public float throttleInput = 0;  // Controls StickCircleX (Throttle)

    [SerializeField]
    public float rollInput = 0;  // Controls StickCircleY (Roll)
    
    [SerializeField]
    public float pitchInput = 0;  // Controls StickCircleY (Pitch)
    
    private TMP_Text text;
    
    // Stick input visualization
    private RectTransform stickCircleTY;
    private RectTransform stickCirclePR;

    void Start()
    {
        // Get Text element from the Canvas
        Canvas canvas = GameObject.Find("Overlay").GetComponent<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("Canvas (Overlay) not found.");
            return;
        }

        // Get the TextMeshPro component
        text = canvas.GetComponentInChildren<TMP_Text>();
        if (text == null)
        {
            Debug.LogError("Text component not found in Overlay.");
        }

        // Locate Stick Input UI elements within their respective panels
        Transform throttleYawPanel = canvas.transform.Find("ThrottleYaw");
        Transform pitchRollPanel = canvas.transform.Find("PitchRoll");

        if (throttleYawPanel != null)
        {
            Transform circleXObj = throttleYawPanel.Find("stickCircleTY");
            if (circleXObj != null)
                stickCircleTY = circleXObj.GetComponent<RectTransform>();
        }
        
        if (pitchRollPanel != null)
        {
            Transform circleYObj = pitchRollPanel.Find("stickCirclePR");
            if (circleYObj != null)
                stickCirclePR = circleYObj.GetComponent<RectTransform>();
        }

        if (stickCircleTY == null || stickCirclePR == null)
        {
            Debug.LogError("Stick input circles not found. Make sure they are correctly named under their respective panels.");
        }
    }

    //Get text field from canvas attached to UI cam and update that text based on what kind of data we want to see.
    //Also update stick input wiht input from controller. Will do after text is working.
    void Update()
    {
        DroneMover selectedDrone = DataManager.Instance.GetSelectedDrone();
        if (selectedDrone != null)
        {
            // Update the text with the selected drone's data
            text.text = UpdateText(selectedDrone);
        }

        if (showStickInput && stickCircleTY != null && stickCirclePR != null)
        {
            UpdateStickInput();
        }
    }

    private string UpdateText(DroneMover drone)
    {
        string text = "";
        text += "Drone Id: " + drone.DID + "\n";
        if (!showStats)
        {
            return text;
        }

        if (showVelocity)
        {
            text += " Velocity: " + drone.Speed + "\n";
        }
        if (showPitch)
        {
            text += "     Pitch: " + drone.Pitch + "\n";
        }
        if (showRoll)
        {
            text += "       Roll: " + drone.Roll + "\n";
        }
        if (showYaw)
        {
            text += "       Yaw: " + drone.Yaw + "\n";
        }
        return text;
    }

    private void UpdateStickInput()
    {
        // Adjust range to UI coordinates (assume 100x100 box for each indicator)
        //clamp Inputs before position calculation for stick input
        yawInput = Mathf.Clamp(yawInput, -1f, 1f);
        throttleInput = Mathf.Clamp(throttleInput, -1f, 1f);
        rollInput = Mathf.Clamp(rollInput, -1f, 1f);
        pitchInput = Mathf.Clamp(pitchInput, -1f, 1f);

        //incorrect values for range of extremes was Yaw and Roll (-0.7 to 0.7) and Throttle and Pitch (-0.45 to 0.45). when using *50 mult for x and y
        //Based on the incorrect range of extremes with *50 , we can calculate the correct to get the correct range of extremes for the stick input
        float xThrottleYaw = yawInput * 35f;   //50 * 0.7 = 35
        float yThrottleYaw = throttleInput * 22.5f;   //50 * 0.45 = 22.5
        float xPitchRoll = rollInput * 35f;
        float yPitchRoll = pitchInput * 22.5f;

        // Update positions within their respective panels
        stickCircleTY.anchoredPosition = new Vector2(xThrottleYaw, yThrottleYaw);
        stickCirclePR.anchoredPosition = new Vector2(xPitchRoll, yPitchRoll);
    }
}
