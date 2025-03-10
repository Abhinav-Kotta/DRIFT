using UnityEngine;
using System.Collections;

public class EngineSpeedMeter : MonoBehaviour {
    public Texture2D SpeedMeterTexture, ArrowTex, QuadTex;
    public float RpmValue;
    public float DelayValue;
    public float posWX, posWY;
    public float k, nx, ny, qx, qy;
    private float valueRPM ;

	// Use this for initialization
	void Start () {
	
	}

    private float lastRpmVal, rpmValDrag;
	// Update is called once per frame
	void Update ()
	{
        //get value from control script (example):
        //PlaneControl pl;
        //GameObject plane = GameObject.Find("Plane");
        //pl = plane.GetComponent("PlaneControl") as PlaneControl;
        //RpmValue=pl.EnginePower;
        //
        if (DelayValue <= 0)
        {
            Debug.Log("enter the variable is greater than zero");
            return;
        }
        else
        {
            //readings slowdown
            valueRPM = lastRpmVal + (RpmValue - rpmValDrag) * Time.deltaTime / DelayValue;
            lastRpmVal = valueRPM;
            rpmValDrag = valueRPM;
        }
        //
        k = Mathf.Clamp(k, 0.5f, 2);//scaling factor
        qx = (16.5f - 6 * k) / 1.5f;//adjustment of the position coordinates of the background when scaling
	}
    //
    void OnGUI()
    {
        GUI.BeginGroup(new Rect(Screen.width / 2 + posWX, Screen.height / 2 + posWY, 256, 256));
        GUI.Label(new Rect(128 - ((150 - qx) / 2) * k, 128 - ((150 - qy) / 2) * k,
            (150 + qx) * k, (150 + qy) * k), QuadTex);
        GUI.Label(new Rect(128 - 64 * k + 2.5f, 128 - 64 * k, 128 * k, 128 * k), SpeedMeterTexture);
        Matrix4x4 mtrxVSM = GUI.matrix;
        GUIUtility.RotateAroundPivot(valueRPM * 300, new Vector2(128, 128));
        GUI.Label(new Rect(128 - ((128 - nx) / 2) * k, 128 - ((128 - ny) / 2) * k,
            64 * k, 64 * k), ArrowTex);
        GUI.matrix = mtrxVSM;
        GUI.EndGroup();
    }
}
