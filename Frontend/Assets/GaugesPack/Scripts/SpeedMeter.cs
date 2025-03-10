using UnityEngine;
using System.Collections;

public class SpeedMeter : MonoBehaviour {
    public Texture2D SpeedMeterTexture, ArrowTex, SMFondTex;
    public float SpeedValue, valueSM;
    public float SMDelayValue;
    public float posWX, posWY;
    public float k, nx, ny, qx, qy;

	// Use this for initialization
	void Start () {
	
	}

    private float lastSMVal, SMValDrag;
	// Update is called once per frame
    private void Update()
    {
        //readings slowdown
        valueSM = lastSMVal + (SpeedValue - SMValDrag) * Time.deltaTime / SMDelayValue;
        lastSMVal = valueSM;
        SMValDrag = valueSM;
        //
        k = Mathf.Clamp(k, 0.5f, 1.5f); //scaling factor
        qx = (16.5f - 6*k)/1.5f; //adjustment of the position coordinates of the background when scaling
    }

    //
    void OnGUI()
    {
        GUI.BeginGroup(new Rect(Screen.width / 2 + posWX, Screen.height / 2 + posWY,
            256, 256));
        GUI.Label(new Rect(128 - ((150 - qx) / 2) * k, 128 - ((150 - qy) / 2) * k,
            (150 + qx) * k, (150 + qy) * k), SMFondTex);
        GUI.Label(new Rect(128 - 64 * k + 2.5f, 128 - 64 * k, 128 * k, 128 * k), SpeedMeterTexture);
        Matrix4x4 mtrxVSM = GUI.matrix;
        GUIUtility.RotateAroundPivot(valueSM * 3.4f, new Vector2(128, 128));
        GUI.Label(new Rect(128 - ((128 - nx) / 2) * k, 128 - ((128 - ny) / 2) * k,
            64 * k, 64 * k), ArrowTex);
        GUI.matrix = mtrxVSM;
        GUI.EndGroup();
    }
}
