using UnityEngine;

public class SpeedMeter : MonoBehaviour {
    public Texture2D SpeedMeterTexture, ArrowTex, SMFondTex;
    public float posWX = 0f, posWY = 180f;
    public float k = 0.7f, nx = 0f, ny = 0f, qx = 0f, qy = 0f;
    public float SMDelayValue = 0.5f;
    
    public float SpeedValue = 0f;
    private float valueSM = 0f;
    private float lastSMVal = 0f, SMValDrag = 0f;
    
    [SerializeField] private float speedScale = 1f;
    
    private DataManager dataManager;
    private DroneMover currentDrone;
    
    private DroneViewCam cameraController;
    [SerializeField] private bool showInFirstPersonOnly = true;
    [SerializeField] private KeyCode toggleKey = KeyCode.S;
    private bool showGauge = true;

    void Start() {
        dataManager = DataManager.Instance;
        cameraController = FindObjectOfType<DroneViewCam>();
        
        SMDelayValue = Mathf.Max(0.01f, SMDelayValue);
    }

    void Update() {
        if (Input.GetKeyDown(toggleKey)) {
            showGauge = !showGauge;
        }
        
        if (!ShouldShowGauge()) return;
        
        currentDrone = dataManager.GetSelectedDrone();
        if (currentDrone != null) {
            SpeedValue = currentDrone.CalculateVelocityMagnitude() * speedScale;
            Debug.Log($"Speed: {SpeedValue}");
            
            Debug.Log($"Speed: {SpeedValue}");
            Debug.Log($"Position x: {currentDrone.getPosition().x}");
            Debug.Log($"Position y: {currentDrone.getPosition().y}");
            Debug.Log($"Position z: {currentDrone.getPosition().z}");
        }
        
        SMDelayValue = Mathf.Max(0.01f, SMDelayValue);
        float tempValue = (SpeedValue - SMValDrag) * Time.deltaTime / SMDelayValue;
        if (!float.IsNaN(tempValue) && !float.IsInfinity(tempValue)) {
            valueSM = lastSMVal + tempValue;
            lastSMVal = valueSM;
            SMValDrag = valueSM;
        }
        
        k = Mathf.Clamp(k, 0.5f, 1.5f);
        qx = (16.5f - 6 * k) / 1.5f;
    }
    
    private bool ShouldShowGauge() {
        if (!showGauge) return false;
        
        if (showInFirstPersonOnly && cameraController != null) {
            int mode = cameraController.GetCurrentMode();
            return mode == 0;
        }
        
        return true;
    }

    void OnGUI() {
        if (!ShouldShowGauge()) return;
        
        if (SMFondTex == null || SpeedMeterTexture == null) return;
        
        GUI.BeginGroup(new Rect(Screen.width / 2 + posWX, Screen.height / 2 + posWY, 256, 256));
        
        GUI.Label(new Rect(128 - ((150 - qx) / 2) * k, 128 - ((150 - qy) / 2) * k,
            (150 + qx) * k, (150 + qy) * k), SMFondTex);
            
        GUI.Label(new Rect(128 - 64 * k + 2.5f, 128 - 64 * k, 
            128 * k, 128 * k), SpeedMeterTexture);
            
        Matrix4x4 mtrxVSM = GUI.matrix;
        GUIUtility.RotateAroundPivot(valueSM * 3.4f, new Vector2(128, 128));
        GUI.Label(new Rect(128 - ((128 - nx) / 2) * k, 128 - ((128 - ny) / 2) * k,
            64 * k, 64 * k), ArrowTex);
        GUI.matrix = mtrxVSM;
        
        GUI.EndGroup();
    }
}