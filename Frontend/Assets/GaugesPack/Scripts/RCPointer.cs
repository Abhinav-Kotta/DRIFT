using UnityEngine;
using System.Collections;

public class RCPointer : MonoBehaviour {
    public Texture2D pTex;
    public Vector2 trgPos;
    public float posWinX, posWinY;
    public float Pang;
    public float pntrSizeX, pntrSizeY, corrX, corrY;
    public float kp;
    public bool tgtLock;

	// Use this for initialization
	void Start ()
	{
	    pntrSizeX = 40;
	    pntrSizeY = 40;
	}
	
	// Update is called once per frame
	void Update ()
	{
        //calc pointer angle
	    float mkrPntrrelX = (trgPos.x + 25 - (Screen.width/2 + posWinX + 128 + 32))/1023;
	    float mkrPntrrelY = ((Screen.height/2 + posWinY + 128 + 32) - trgPos.y - 22)/1023;
        if (mkrPntrrelY < 0)
        {
            Pang = (Mathf.Atan(mkrPntrrelX / mkrPntrrelY) + Mathf.PI) * Mathf.Rad2Deg;
        }
        else
        {
            Pang = Mathf.Atan(mkrPntrrelX / mkrPntrrelY) * Mathf.Rad2Deg;
            if (Pang < 0)
            {
                Pang = 360 + Pang;
            }
        }
        //
        if (tgtLock)
        {
            AnnunciatorPnl ap;
            GameObject goip = GameObject.Find("GaugesPack");
            ap = goip.GetComponent("AnnunciatorPnl") as AnnunciatorPnl;
            float distX = trgPos.x + 25 - (Screen.width/2 + posWinX + 128 + 32);
            float distY = (Screen.height/2 + posWinY + 128 + 32) - trgPos.y - 22;
            ap.dist = Mathf.Sqrt(distX*distX + distY*distY);
        }
	}
    //
    public void OnGUI()
    {
        if (tgtLock)
        {
            GUI.color = Color.yellow;
        }
        else
        {
            GUI.color = Color.red;
        }
        GUI.BeginGroup(new Rect(Screen.width / 2 + posWinX,
            Screen.height / 2 + posWinY, 192, 192));
        Matrix4x4 matrixPntr = GUI.matrix;
        Vector2 pos = new Vector2(128, 128);
        GUIUtility.RotateAroundPivot(Pang, pos);
        GUI.Label(new Rect(128 - ((40 - 26) / 2) * kp, 128 - ((40 + 79.81f) / 2) * kp,
           pntrSizeX / 2 * kp, pntrSizeY / 2 * kp), pTex);
        GUI.matrix = matrixPntr;
        GUI.EndGroup();
    }
}
