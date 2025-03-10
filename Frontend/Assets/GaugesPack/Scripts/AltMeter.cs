using UnityEngine;
using System.Collections;

public class AltMeter : MonoBehaviour {
    public GUIStyle AMStyle;
    public Texture2D AltMeterTexture, Arrow1Tex, Arrow2Tex, AMPntr, AMFondTex;
    public Component AP;
    public float AltMValue;
    public float posWX, posWY;//poswx=-550 poswy=80
    public float k, nx, ny, qx, qy, px, py;
    public float pntrVal, delayValue;

	// Use this for initialization
	void Start () {
	
	}

    private float Hvalue, drag, lastAlt;
	// Update is called once per frame
	void Update () {
        k = Mathf.Clamp(k, 0.5f, 2);//scaling factor
        qx = (16.5f - 6 * k) / 1.5f; //adjustment of the position coordinates of the background when scaling
        //"dangerous height" pointer control
	    if (Input.GetKey("home"))
	    {
	        pntrVal += 1;
	    }
	    else
	    {
	        if (Input.GetKey("end"))
	        {
	            pntrVal -= 1;
	        }
	    }
        //set pointer value change border 0 - 360 deg.
        if (pntrVal > 360)
        {
            pntrVal = 0;
        }
        else if (pntrVal < 0)
        {
            pntrVal = 360;
        }
        //readings slowdown
        Hvalue = lastAlt + (AltMValue - drag) * Time.deltaTime / delayValue;
        lastAlt = Hvalue;
        drag = Hvalue;
        //
        if (Hvalue * 10 < pntrVal / 36)
        {
            AP.GetComponent<AnnunciatorPnl>().lowAlt = true;
        }
        else
        {
            AP.GetComponent<AnnunciatorPnl>().lowAlt = false;
        }
	}
    //
    void OnGUI()
    {
        GUI.BeginGroup(new Rect(Screen.width / 2 + posWX, Screen.height / 2 + posWY, 256, 256));
        GUI.Label(new Rect(128 - ((150 - qx) / 2) * k, 128 - ((150 - qy) / 2) * k,
            (150 + qx) * k, (150 + qy) * k), AMFondTex);
        GUI.Label(new Rect(128 - 64 * k + 2.5f, 128 - 64 * k, 128 * k, 128 * k), AltMeterTexture);//
        //
        Matrix4x4 mtrxAM1 = GUI.matrix;
        GUIUtility.RotateAroundPivot(Hvalue * 360, new Vector2(128, 128));
        GUI.Label(new Rect(128 - ((128 - nx) / 2) * k, 128 - ((128 - ny) / 2) * k,
            64 * k, 64 * k), Arrow1Tex);
        GUI.matrix = mtrxAM1;
        //
        Matrix4x4 mtrxAM2 = GUI.matrix;
        GUIUtility.RotateAroundPivot(Hvalue * 36, new Vector2(128, 128));
        GUI.Label(new Rect(128 - ((128 - nx) / 2) * k, 128 - ((128 - ny) / 2) * k,
            128 / 3f * k, 128 / 3f * k), Arrow2Tex);
        GUI.matrix = mtrxAM2;
        //
        Matrix4x4 mtrxAMPntr = GUI.matrix;
        GUIUtility.RotateAroundPivot(pntrVal, new Vector2(128, 128));
        GUI.Label(new Rect(128 - ((40 - px) / 2) * k, 128 - ((40 - py) / 2) * k,
            20 * k, 20 * k), AMPntr);
        GUI.matrix = mtrxAMPntr;
        GUI.EndGroup();
    }
}
