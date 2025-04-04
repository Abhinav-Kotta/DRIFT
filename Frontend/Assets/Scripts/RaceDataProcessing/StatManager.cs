using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.CodeDom.Compiler;

public class StatManager : MonoBehaviour
{
    //Fake data toggle 
    [SerializeField] public bool needFakeData = false;
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
    
    private TMP_Text textBox;
    
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
        textBox = canvas.GetComponentInChildren<TMP_Text>();
        if (textBox == null)
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
        if(needFakeData)
        {
            Debug.Log("making fake data check");
            fakeData();
            
        }
        
            DroneMover selectedDrone = DataManager.Instance.GetSelectedDrone();
            if (selectedDrone != null)
            {
                if (showStickInput && stickCircleTY != null && stickCirclePR != null)
                {
                    // Update stick input visualization
                    yawInput = selectedDrone.YawInput;
                    throttleInput = selectedDrone.ThrottleInput;
                    rollInput = selectedDrone.RollInput * -1f; // Invert roll input for correct visualization
                    pitchInput = selectedDrone.PitchInput;
                    UpdateStickInput();
                }
            }
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

    private void fakeData()
    {
        
        // Debug.Log("Forming fake data");
        // Create a consistent drone ID
        string droneId = "1.1.1.1";

        // Generate position values based on sine and cosine functions
        float time = Time.time;
        float x = 0;
        float y = (Mathf.Sin(time) * 10f) + 20f;
        float z = Mathf.Cos(time) * 10f; // Keep z constant for simplicity

        // Create a new DroneData instance
        DroneData droneData = new DroneData
        {
            drone_id = droneId,
            timestamp = time,
            position = new Position { x = x, y = y, z = z },
            attitude = new Attitude { x = 0, y = 0, z = 0, w = 1 },
            velocity = new Vector3Data { x = 0, y = 0, z = 0 },
            gyro = new GyroData { pitch = 0, roll = 0, yaw = 0 },
            inputs = new Inputs { throttle = 0, yaw = 0, pitch = 0, roll = 0 },
            battery = new Battery { percentage = 100, voltage = 12.6f },
            motor_count = 4,
            motor_rpms = new float[4] { 1000, 1000, 1000, 1000 }
        };
        // Debug.Log(droneData.drone_id);
        // Debug.Log(droneData.position.x);    
        DataManager.Instance.SetSelectedDrone(droneData.drone_id.ToString());
        DataManager.Instance.UpdateDroneData(droneData);
    }
}
