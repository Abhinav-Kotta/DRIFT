using UnityEngine;
using System.Collections;

public class VertSpeedMeter : MonoBehaviour {
    public Texture2D VertSMeterTexture, ArrowTex, VSFondTex;
    public float VertSpeedValue, valueVS;
    public float DelayValue;
    public float posWX, posWY;
    public float k;
    public float nx, ny;
    private float qx;
	// Use this for initialization
	void Start () {
	
	}
    private float lastVSVal, VSValDrag;
	// Update is called once per frame
	void Update ()
	{
        if (DelayValue <= 0)
        {
            Debug.Log("enter the variable is greater than zero");
            return;
        }
        else
        {
            //readings slowdown
            valueVS = lastVSVal + (VertSpeedValue - VSValDrag) * Time.deltaTime / DelayValue;
            lastVSVal = valueVS;
            VSValDrag = valueVS;
        }
	    k = Mathf.Clamp(k, 0.5f, 2);//scaling factor
        ny = (157.5f + 9 * k) / 1.5f;//adjustment of the position coordinates of the arrow
        qx = (16.5f - 6 * k) / 1.5f;//adjustment of the position coordinates of the background when scaling
	}
    //
    void OnGUI()
    {
        GUI.BeginGroup(new Rect(Screen.width / 2 + posWX, Screen.height / 2 + posWY, 256, 256));
        GUI.Label(new Rect(128 - ((135 - qx) / 2) * k, 128 - (135 / 2) * k, (135 + qx) * k, 135 * k),
            VSFondTex);
        GUI.Label(new Rect(128 - (128 / 2) * k + 2.5f, 128 - (128 / 2) * k, 128 * k, 128 * k),
            VertSMeterTexture);
        Matrix4x4 mtrxVSM = GUI.matrix;
        GUIUtility.RotateAroundPivot(valueVS * 3, new Vector2(128, 128));
        GUI.Label(new Rect(128 - ((128 - nx) / 2) * k, 128 - ((128 - ny) / 2) * k, 64 * k, 128 * k), ArrowTex);
        GUI.matrix = mtrxVSM;
        GUI.EndGroup();
    }
}
