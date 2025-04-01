using UnityEngine;

public class FuelMeter : MonoBehaviour {
    public Texture2D BatteryMeterTexture, ArrowTex, BMFondTex;
    public Font mFont;
    
    public float posWX = 300f, posWY = 180f;
    public float textSizeX = 60f, textSizeY = 30f;
    public float k = 0.7f, nx = 0f, ny = 0f, qx = 0f, qy = 0f, tx = 0f, ty = 0f;
    
    private DataManager dataManager;
    private DroneMover currentDrone;
    
    private DroneViewCam cameraController;
    [SerializeField] private bool showInFirstPersonOnly = true;
    [SerializeField] private KeyCode toggleKey = KeyCode.B;
    private bool showGauge = true;
    
    private float batteryPercentage = 100f; 
    private float batteryVoltage = 0f;

    void Start() {
        dataManager = DataManager.Instance;
        cameraController = FindObjectOfType<DroneViewCam>();
        
        k = Mathf.Clamp(k, 0.5f, 2f);
        qx = (16.5f - 6f * k) / 1.5f;
        ty = -(6.9375f - 2.53f*k)/1.05f;
    }
    
    void Update() {
        if (Input.GetKeyDown(toggleKey)) {
            showGauge = !showGauge;
        }
        
        if (!ShouldShowGauge()) return;
        
        currentDrone = dataManager.GetSelectedDrone();
        if (currentDrone != null) {
            batteryPercentage = currentDrone.BatteryPercentage * 100;
            batteryVoltage = currentDrone.BatteryVoltage;
            //Debug.Log($"Battery percentage: {batteryPercentage}%");
            //Debug.Log($"Battery voltage: {batteryVoltage}V");
        }
        
        k = Mathf.Clamp(k, 0.5f, 2f);
        ty = -(6.9375f - 2.53f*k)/1.05f;
        qx = (16.5f - 6f * k) / 1.5f;
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
        
        if (BMFondTex == null || BatteryMeterTexture == null) return;
        
        GUIStyle BMStyle = new GUIStyle();
        Color color = new Color(0.2f, 0.8f, 0.75f, 1f);
        BMStyle.name = "BatteryMeter";
        BMStyle.fontSize = (int)(12*k);
        BMStyle.font = mFont;
        BMStyle.normal.textColor = color;
        
        GUI.BeginGroup(new Rect(Screen.width / 2 + posWX, Screen.height / 2 + posWY, 256, 256));
        
        // Background
        GUI.Label(new Rect(128 - ((150 - qx) / 2) * k, 128 - ((150 - qy) / 2) * k,
            (150 + qx) * k, (150 + qy) * k), BMFondTex);
            
        // Battery percentage text
        GUI.Label(new Rect(128 - ((textSizeX - tx) / 2) * k, 128 - ((textSizeY - ty) / 2) * k,
            textSizeX * k, textSizeY * k), batteryPercentage.ToString("F0") + "%", BMStyle);
            
        // Gauge face
        GUI.Label(new Rect(128 - 64 * k + 2.5f, 128 - 64 * k,
            128 * k, 128 * k), BatteryMeterTexture);
            
        float arrowRotation = (batteryPercentage / 100f * 7.2f) - 3.6f;
        arrowRotation *= 36f;
            
        // Arrow rotation
        Matrix4x4 mtrxBM = GUI.matrix;
        GUIUtility.RotateAroundPivot(arrowRotation, new Vector2(128, 128));
        GUI.Label(new Rect(128 - ((128 - nx) / 2) * k, 128 - ((128 - ny) / 2) * k,
            64 * k, 64 * k), ArrowTex);
        GUI.matrix = mtrxBM;
        
        GUI.EndGroup();
    }
}