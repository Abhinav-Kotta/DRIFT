using UnityEngine;
using System.Collections;

public class AviaHorizont : MonoBehaviour
{
    public GUIStyle AHStyle;
    public Texture2D AviaHorTexture, scale, lml, AHFond;
    //public float fondXsize, fondYsize;
    public float posWX, posWY, scaleXsize, scaleYsize;
    public float scMove, scCorr, scaleK;
    public float vTv;
    public float k, qx, qy, nx, ny, tsX, tsY;
    
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update ()
	{
	    k = Mathf.Clamp(k, .5f, 1.8f);
	    scCorr = (57.7f + 9.4f*k)/1.42f;//.3f;//scale correction
	    scMove = 325 * scaleK + 127.6f;//scale move
	}
    //
    void OnGUI()
    {
        GUI.BeginGroup(new Rect(Screen.width/2 + posWX, Screen.height/2 + posWY, 256, 256));
        GUI.BeginGroup(new Rect(128 - 64 * k, 128 - 64 * k, 128 * k, 128 * k));
        GUI.DrawTexture(new Rect(((scaleXsize + scCorr) / 2) * k, - ((scaleYsize - scMove) / 2) * k,
            scaleXsize * k, scaleYsize * k), scale);
        GUI.EndGroup();
        GUI.Label(new Rect(128 - ((150 - qx) / 2) * k, 128 - ((150 - qy) / 2) * k,
            (150 + qx) * k, (150 + qy) * k), AHFond);
        GUI.Label(new Rect(128 - 64 * k + 2.5f, 128 - 64 * k, 128 * k, 128 * k), AviaHorTexture);//
        //
        Matrix4x4 mtrx1 = GUI.matrix;
        GUIUtility.RotateAroundPivot(vTv * 90, new Vector2(128, 128));
        GUI.Label(new Rect(128 - ((128 - nx) / 2) * k, (128 - ((128 - ny) / 2) * k),
            (128 - tsX) * k, (128 - tsY) * k), lml);//
        GUI.matrix = mtrx1;
        GUI.EndGroup();
    }
}
