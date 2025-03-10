using UnityEngine;
using System.Collections;

public class RadioCompass : MonoBehaviour {
    public GUIStyle RCStyle;
    public Texture2D RadioCompTexture, RCArrowTex, RCFondTex;
    public float RCValue;
    public float posWX, posWY;
    public float k, nx, ny, qx, qy;
    public float relativeX, relativeY;
    public Vector2[] targPos;
    public RCPointer[] RCPtrs;//
    public float DelayValue;
    public float x, y;
    private float Xrel, Yrel;//h

	// Use this for initialization
	void Start () {
        //
        targPos = new Vector2[4];
        targPos[0] = new Vector2(410, 392);
        targPos[1] = new Vector2(542, 270);
        targPos[2] = new Vector2(731, 183);
        targPos[3] = new Vector2(960, 121);
        //
	    x = 206;
	    y = 393;
	}
    //
    private float relDragX, relDragY, lastRelX, lastRelY;
	// Update is called once per frame
	void Update () {
        relativeX = (x + 25 - (Screen.width / 2 + posWX + 128 + 32)) / 1023;
        relativeY = ((Screen.height/2 + posWY + 128 + 32) - y - 22) / 1023;
        if (DelayValue <= 0)
        {
            Debug.Log("enter the variable is greater than zero");
            return;
        }
        else
        {
            Xrel = lastRelX + (relativeX - relDragX) * Time.deltaTime / DelayValue;
            lastRelX = Xrel;
            relDragX = Xrel;
            Yrel = lastRelY + (relativeY - relDragY) * Time.deltaTime / DelayValue;
            lastRelY = Yrel;
            relDragY = Yrel;
        }
	    RCValue = AngCalc(Xrel, Yrel);
        if (RCValue < 0)
        {
            RCValue = 360 + RCValue;
        }
        GameObject goRC = GameObject.Find("GaugesPack");
        RCPtrs = goRC.GetComponents<RCPointer>();
        //
        for (int i = 0; i < RCPtrs.Length; i++)
        {
            RCPtrs[i].trgPos = targPos[i];
            RCPtrs[i].kp = k;
        }
        k = Mathf.Clamp(k, 0.5f, 2);//
        //ny = (157.5f + 9 * k) / 1.5f;//correction coordinate position of the arrow when scaling
        qx = (16.5f - 6 * k) / 1.5f;//adjustment of the position coordinates of the background when scaling
    }
    
    void OnGUI()
    {
        GUI.BeginGroup(new Rect(Screen.width / 2 + posWX, Screen.height / 2 + posWY, 256, 256));
        GUI.Label(new Rect(128 - ((150 - qx) / 2) * k, 128 - ((150 - qy) / 2) * k,
            (150 + qx) * k, (150 + qy) * k), RCFondTex);
        GUI.Label(new Rect(128 - 64 * k + 2.5f, 128 - 64 * k,
            128 * k, 128 * k), RadioCompTexture);//
        Matrix4x4 mtrxVSM = GUI.matrix;
        GUIUtility.RotateAroundPivot(RCValue, new Vector2(128, 128));
        GUI.Label(new Rect(128 - ((128 - nx) / 2) * k, 128 - ((128 - ny) / 2) * k,
            64 * k, 64 * k), RCArrowTex);
        GUI.matrix = mtrxVSM;
        GUI.EndGroup();
    }
    //
    public float AngCalc(float cx, float cy)
    {
        float RCVal;
        if (cy < 0)
        {
            RCVal = (Mathf.Atan(cx / cy) + Mathf.PI) * Mathf.Rad2Deg;
        }
        else
        {
            RCVal = Mathf.Atan(cx / cy) * Mathf.Rad2Deg;
        }
        return RCVal;
    }
}
