using UnityEngine;
using System.Collections;

public class FuelMeter : MonoBehaviour {
    public Texture2D FuelMeterTexture, ArrowTex, FMFondTex;
    public Component AP, ESM;
    public Font mFont;
    public float posWX, posWY;
    public float textSizeX, textSizeY;
    public float fuelMeterValue;
    public float k, nx, ny, qx, qy, tx, ty;

	// Use this for initialization
	void Start ()
	{
	    fuelMeterValue = 3.6f;
	}
	
	// Update is called once per frame
	void Update () {
        //
	    fuelMeterValue = fuelMeterValue - AP.GetComponent<GaugesPackDemo>().esValue/10000;
        if (fuelMeterValue <= -2.8)
        {
            AP.GetComponent<AnnunciatorPnl>().fuelLow = true;
        }
        if (fuelMeterValue <= -3.6f)
        {
            fuelMeterValue = -3.6f;
            ESM.GetComponent<GaugesPackDemo>().esValue = 0;
            ESM.GetComponent<AnnunciatorPnl>().engStop = true;
        }
        k = Mathf.Clamp(k, 0.5f, 2);//scalling factor 
        //ny = (157.5f + 9 * k) / 1.5f;//correction coordinate position of the arrow when scaling
	    ty = -(6.9375f - 2.53f*k)/1.05f;
        qx = (16.5f - 6 * k) / 1.5f;//adjustment of the position coordinates of the background when scaling
	}
    void OnGUI()
    {
        GUIStyle FMStyle2 = new GUIStyle();//
        Color color = new Color(0.2f, 0.8f, 0.75f, 1f);
        FMStyle2.name = "FuelMeter";
        FMStyle2.fontSize = (int) (12*k);
        FMStyle2.font = mFont;
        FMStyle2.normal.textColor = color;
        //
        GUI.BeginGroup(new Rect(Screen.width / 2 + posWX, Screen.height / 2 + posWY, 256, 256));
        GUI.Label(new Rect(128 - ((150 - qx) / 2) * k, 128 - ((150 - qy) / 2) * k,
            (150 + qx) * k, (150 + qy) * k), FMFondTex);
        GUI.Label(new Rect(128 - ((textSizeX - tx) / 2) * k, 128 - ((textSizeY - ty) / 2) * k,
            textSizeX * k, textSizeY * k), (((fuelMeterValue / 3.6f) + 1) * 1000).ToString("f0"), FMStyle2);
        GUI.Label(new Rect(128 - 64 * k + 2.5f, 128 - 64 * k,
            128 * k, 128 * k), FuelMeterTexture);//
        Matrix4x4 mtrxVSM = GUI.matrix;
        GUIUtility.RotateAroundPivot(fuelMeterValue * 36, new Vector2(128, 128));
        GUI.Label(new Rect(128 - ((128 - nx) / 2) * k, 128 - ((128 - ny) / 2) * k,
            64 * k, 64 * k), ArrowTex);
        GUI.matrix = mtrxVSM;
        GUI.EndGroup();
    }
}
