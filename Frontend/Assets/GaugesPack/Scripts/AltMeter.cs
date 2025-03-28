using UnityEngine;
using System.Collections;

public class AltMeter : MonoBehaviour {
    public GUIStyle AMStyle;
    public Texture2D AltMeterTexture, Arrow1Tex, Arrow2Tex, AMPntr, AMFondTex;
    public Component AP;
    
    [SerializeField] private float altitudeScale = 0.1f;

    public float posWX = 300f, posWY = -180f;
    public float k = 0.7f, nx = 0f, ny = 0f, qx = 0f, qy = 0f, px = 0f, py = 0f;
    public float pntrVal = 180f, delayValue = 0.5f;

    private float Hvalue, drag, lastAlt;
    public float AltMValue;
    private bool hasAnnunciator = false;
    private DataManager dataManager;
    private DroneMover currentDrone;
    
    private DroneViewCam cameraController;
    [SerializeField] private bool showInFirstPersonOnly = true;
    [SerializeField] private KeyCode toggleKey = KeyCode.G;
    private bool showGauge = true;

    void Start() {
        // Get references to your data management system
        dataManager = DataManager.Instance;
        cameraController = FindObjectOfType<DroneViewCam>();
        
        // Initialize values properly
        k = Mathf.Clamp(k, 0.5f, 2f);
        qx = (16.5f - 6f * k) / 1.5f;
        
        hasAnnunciator = (AP != null && AP.GetComponent<AnnunciatorPnl>() != null);
    }

    void Update() {
        if (Input.GetKeyDown(toggleKey)) {
            showGauge = !showGauge;
        }
        
        if (!ShouldShowGauge()) return;
        
        currentDrone = dataManager.GetSelectedDrone();
        if (currentDrone == null) {
            // Debug.LogError("No drone selected or data manager not initialized.");
            return;
        }
        if (currentDrone != null) {
            AltMValue = currentDrone.getPosition().y * altitudeScale;
            Debug.Log(AltMValue);
        }
        
        k = Mathf.Clamp(k, 0.5f, 2f);
        qx = (16.5f - 6f * k) / 1.5f;
        
        if (Input.GetKey("home")) {
            pntrVal += 1;
        } else if (Input.GetKey("end")) {
            pntrVal -= 1;
        }
        
        pntrVal = Mathf.Repeat(pntrVal, 360f);
        
        delayValue = Mathf.Max(0.01f, delayValue);
        
        float tempValue = (AltMValue - drag) * Time.deltaTime / delayValue;
        if (!float.IsNaN(tempValue) && !float.IsInfinity(tempValue)) {
            Hvalue = lastAlt + tempValue;
            lastAlt = Hvalue;
            drag = Hvalue;
        }
        
        if (hasAnnunciator) {
            try {
                AP.GetComponent<AnnunciatorPnl>().lowAlt = (Hvalue * 10 < pntrVal / 36);
            } catch (System.Exception) {
                hasAnnunciator = false;
            }
        }
    }
    
    private bool ShouldShowGauge() {
        if (!showGauge) return false;
        
        if (showInFirstPersonOnly && cameraController != null) {
            int mode = cameraController.GetCurrentMode();
            return mode == 0; // First-person mode
        }
        
        return true;
    }
    
    void OnGUI() {
        if (!ShouldShowGauge()) return;
        
        if (AMFondTex == null || AltMeterTexture == null) return;
        
        GUI.BeginGroup(new Rect(Screen.width / 2 + posWX, Screen.height / 2 + posWY, 256, 256));
        
        // Background and main dial
        GUI.Label(new Rect(128 - ((150 - qx) / 2) * k, 128 - ((150 - qy) / 2) * k,
            (150 + qx) * k, (150 + qy) * k), AMFondTex);
        GUI.Label(new Rect(128 - 64 * k + 2.5f, 128 - 64 * k, 128 * k, 128 * k), AltMeterTexture);
        
        // Only render arrows if textures are assigned
        if (Arrow1Tex != null) {
            try {
                // Large arrow (full rotation)
                Matrix4x4 mtrxAM1 = GUI.matrix;
                GUIUtility.RotateAroundPivot(Mathf.Repeat(Hvalue * 360f, 360f), new Vector2(128, 128));
                GUI.Label(new Rect(128 - ((128 - nx) / 2) * k, 128 - ((128 - ny) / 2) * k,
                    64 * k, 64 * k), Arrow1Tex);
                GUI.matrix = mtrxAM1;
            } catch (System.Exception) {
                // Restore matrix if rotation fails
                GUI.matrix = GUI.matrix;
            }
        }
        
        if (Arrow2Tex != null) {
            try {
                // Small arrow (faster rotation)
                Matrix4x4 mtrxAM2 = GUI.matrix;
                GUIUtility.RotateAroundPivot(Mathf.Repeat(Hvalue * 36f, 360f), new Vector2(128, 128));
                GUI.Label(new Rect(128 - ((128 - nx) / 2) * k, 128 - ((128 - ny) / 2) * k,
                    128 / 3f * k, 128 / 3f * k), Arrow2Tex);
                GUI.matrix = mtrxAM2;
            } catch (System.Exception) {
                // Restore matrix if rotation fails
                GUI.matrix = GUI.matrix;
            }
        }
        
        if (AMPntr != null) {
            try {
                // Warning pointer
                Matrix4x4 mtrxAMPntr = GUI.matrix;
                GUIUtility.RotateAroundPivot(pntrVal, new Vector2(128, 128));
                GUI.Label(new Rect(128 - ((40 - px) / 2) * k, 128 - ((40 - py) / 2) * k,
                    20 * k, 20 * k), AMPntr);
                GUI.matrix = mtrxAMPntr;
            } catch (System.Exception) {
                // Restore matrix if rotation fails
                GUI.matrix = GUI.matrix;
            }
        }
        
        GUI.EndGroup();
    }
}
