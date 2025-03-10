using UnityEngine;
using System.Collections;

public class AnnunciatorPnl : MonoBehaviour
{
    public GUIStyle APStyle;
    public Font DigFont;
    public float pnlPosX, pnlPosY, bcnPosX, bcnPosY, tgPnlPosX, tgPnlPosY, modePnlPosX, modePnlPosY;
    public float deltaTimeON, deltaTimeOFF;
    public bool on, ledOFF, ledON;
    public bool fire, fuelLow, lowAlt, hightT, engStop, oilLowPress, hydrFail;
    public bool[] bc;
    public bool apOn, rdrOn, terrfollowOn, tgtLocked;
    public int[] tgtNum;
    public int targetNum;
    public float dist;
    public float[] beaconDist;
	// Use this for initialization
	void Start () {
	    tgtNum = new int[4];
	    tgtNum[0] = 1;
	    tgtNum[1] = 2;
	    tgtNum[2] = 3;
	    tgtNum[3] = 4;
        //
        bc=new bool[5];
        beaconDist=new float[5];
	}

    private bool ap, rdr, terrFlv;
    private float lastTime;
	// Update is called once per frame
	void Update () {
        //LED on/off
	    if (on)
	    {
	        if (ledOFF)
	        {
	            if (Time.time > (lastTime + deltaTimeOFF))
	            {
	                ledON = true;
	                ledOFF = false;
	                lastTime = Time.time;
	            }
	        }
	        else if (Time.time > (lastTime + deltaTimeON))
	        {
	            ledON = false;
	            ledOFF = true;
	            lastTime = Time.time;
	        }
	    }
        //system control
        if (!ap)
        {
            //autopilot on
            if (Input.GetKeyDown("w"))
            {
                apOn = true;
                ap = true;
            }
        }
        else
        {
            //autopilot off
            if (Input.GetKeyDown("w"))
            {
                apOn = false;
                ap = false;
            }
        }
        //
        if (!rdr)
        {
            //radar on
            if (Input.GetKeyDown("r"))
            {
                rdrOn = true;
                rdr = true;
            }
        }
        else
        {
            //radar off
            if (Input.GetKeyDown("r"))
            {
                rdrOn = false;
                rdr = false;
            }
        }
        //
        if (!terrFlv)
        {
            if (Input.GetKeyDown("e"))
            {
                terrfollowOn = true;
                terrFlv = true;
            }
        }
        else
        {
            if (Input.GetKeyDown("e"))
            {
                terrfollowOn = false;
                terrFlv = false;
            }
        }
	}
    //
    public bool panelOnOff(bool led, bool onn)
    {
        if (onn)
        {
            return led;
        }
        return false;
    }

    //
    void OnGUI()
    {
        GUI.BeginGroup(new Rect(Screen.width/2 + pnlPosX, Screen.height/2 + pnlPosY, 256, 256));
        GUI.skin.box.font = DigFont;
        GUI.skin.box.fontSize = 12;
        GUI.skin.box.contentOffset = new Vector2(0, 1.5f);
        Color colRed = Color.red;
        Color colGreen = Color.green;
        Color colYellow = Color.yellow;
        string la = "LOW ALT";
        string a1 = "FIRE";
        string a2 = "FUEL LOW";
        string a3 = "HIGHT T";
        string a4 = "ENG STOP";
        string a5 = "OIL LOW P";
        string a6 = "HYDR FAIL";
        colChange(panelOnOff(ledON, lowAlt), colYellow);
        GUI.Box(new Rect(10, 10, 145, 20), la);
        colChange(panelOnOff(ledON,fire),colRed);
        GUI.Box(new Rect(10, 35, 70, 20), a1);
        colChange(panelOnOff(ledON,fuelLow),colRed);
        GUI.Box(new Rect(85, 35, 70, 20), a2);
        colChange(panelOnOff(ledON,hightT),colRed);
        GUI.Box(new Rect(10, 60, 70, 20), a3);
        colChange(panelOnOff(ledON,engStop),colYellow);
        GUI.Box(new Rect(85, 60, 70, 20), a4);
        colChange(panelOnOff(ledON,oilLowPress),colRed);
        GUI.Box(new Rect(10, 85, 70, 20), a5);
        colChange(panelOnOff(ledON,hydrFail),colRed);
        GUI.Box(new Rect(85, 85, 70, 20), a6);
        GUI.EndGroup();
        //beacon group
        GUI.BeginGroup(new Rect(Screen.width/2+bcnPosX,Screen.height/2+bcnPosY,256,256));
        GUI.skin.box.font = DigFont;
        GUI.skin.box.fontSize = 9;
        GUI.skin.box.contentOffset = new Vector2(0, 1.5f);
        GUI.skin.box.normal.textColor = Color.green;
        string b1 = "BEACON";
        string b2 = "DIST";
        GUI.Box(new Rect(10, 10, 40, 20), b1);
        GUI.Box(new Rect(52, 10, 28, 20), b2);
        colChange(bc[0],colGreen);
        GUI.Box(new Rect(10, 32, 20, 20), "1");
        GUI.Box(new Rect(52, 32, 28, 20), beaconDist[0].ToString("F0"));
        colChange(bc[1],colGreen);
        GUI.Box(new Rect(10, 54, 20, 20), "2");
        GUI.Box(new Rect(52, 54, 28, 20), beaconDist[1].ToString("F0"));
        colChange(bc[2],colGreen);
        GUI.Box(new Rect(10, 76, 20, 20), "3");
        GUI.Box(new Rect(52, 76, 28, 20), beaconDist[2].ToString("F0"));
        colChange(bc[3],colGreen);
        GUI.Box(new Rect(10, 98, 20, 20), "4");
        GUI.Box(new Rect(52, 98, 28, 20), beaconDist[3].ToString("F0"));
        colChange(bc[4],colGreen);
        GUI.Box(new Rect(10, 120, 20, 20), "5");
        GUI.Box(new Rect(52, 120, 28, 20), beaconDist[4].ToString("F0"));
        GUI.EndGroup();
        //target info
        GUI.BeginGroup(new Rect(Screen.width/2+tgPnlPosX,Screen.height/2+tgPnlPosY,256,256));
        GUI.skin.box.font = DigFont;
        GUI.skin.box.fontSize = 9;
        GUI.skin.box.contentOffset = new Vector2(0, 0f);
        GUI.skin.box.normal.textColor = Color.yellow;
        string t1 = "TARGET";
        string t2 = "DIST";
        GUI.Box(new Rect(10, 10, 40, 15), t1);
        GUI.Box(new Rect(52, 10, 40, 15), t2);
        GUI.Box(new Rect(10, 27, 40, 15), tgtNum[targetNum].ToString());
        GUI.Box(new Rect(52, 27, 40, 15), dist.ToString("F2"));
        GUI.EndGroup();
        //
        GUI.BeginGroup(new Rect(Screen.width / 2 + modePnlPosX, Screen.height / 2 + modePnlPosY, 256, 256));
        GUI.skin.box.font = DigFont;
        GUI.skin.box.fontSize = 9;
        GUI.skin.box.contentOffset = new Vector2(0, 0f);
        GUI.skin.box.normal.textColor = Color.green;
        string m1 = "AUTOPILOT ON";
        string m2 = "RADAR ON";
        string m3 = "TERR FLV ON";
        string m4 = "TARGET LOCK";
        colChange(apOn,colGreen);
        GUI.Box(new Rect(10, 10, 71, 15), m1);
        colChange(rdrOn,colGreen);
        GUI.Box(new Rect(83, 10, 71, 15), m2);
        colChange(terrfollowOn,colGreen);
        GUI.Box(new Rect(10, 27, 71, 15), m3);
        colChange(tgtLocked,colGreen);
        GUI.Box(new Rect(83, 27, 71, 15), m4);
        GUI.EndGroup();
    }
    //
    void colChange(bool panN, Color c)
    {
        if (panN)
        {
            GUI.skin.box.normal.textColor = c;
        }
        else
        {
            GUI.skin.box.normal.textColor = Color.gray;
        }
    }
}
