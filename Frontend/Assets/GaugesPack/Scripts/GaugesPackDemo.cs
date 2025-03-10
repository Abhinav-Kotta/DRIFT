using System;
using UnityEngine;
using System.Collections;

public class GaugesPackDemo : MonoBehaviour
{
    public Component InP;
    public Font mFont;
    string txt;
    public float _k;
    public float VSMtextPosX, VSMtextPosY, TPX, TPY, vsmValue;
    public float SMtextPosX, SMtextPosY, TPX1, TPY1, smValue;
    public float AMtextPosX, AMtextPosY, TPX2, TPY2, amValue;
    public float AHtextPosX, AHtextPosY, TPX3, TPY3, ahValue1, ahValue2;
    public float RCtextPosX, RCtextPosY, TPX4, TPY4;
    public float EStextPosX, EStextPosY, TPX5, TPY5, esValue;
    public float FMtextPosX, FMtextPosY, TPX6, TPY6;
    public Texture2D rs0, rs1, rs2;
    public Texture2D tgA0, tgA1, tgA2;
    public Texture2D tgB0, tgB1, tgB2;
    public Texture2D tgC0, tgC1, tgC2;
    public Texture2D tgD0, tgD1, tgD2;
    public Component AnnPnl;
    public Vector2[] targPos;
    public float[] bcX, bcY;
    public RCPointer[] RCPtrs; //
    public float mousePosX, mousePosY, H;
	// Use this for initialization
	void Start ()
	{
        //beacon position array
        bcX = new float[5];
        bcX[0] = 206;
        bcX[1] = 311;
        bcX[2] = 485;
        bcX[3] = 711;
        bcX[4] = 950;
        bcY = new float[5];
        bcY[0] = 393;
        bcY[1] = 251;
        bcY[2] = 150;
        bcY[3] = 82;
        bcY[4] = 54;
	    H = Screen.height;
        targPos = new Vector2[4];
        //target position array
        targPos[0] = new Vector2(410, 392);
        targPos[1] = new Vector2(542, 270);
        targPos[2] = new Vector2(731, 183);
        targPos[3] = new Vector2(950, 121);
        //
        txt = "\n home/end - altitude marker value change \n w - autopilot on/off " +
              "\n e - terrain follow mode \n r - radar on/off";
	}

    public bool mouseOver, mouseOver1, mouseOver2, mouseOver3, mouseOver4;
    private bool mouseOverTg0, mouseOverTg1, mouseOverTg2, mouseOverTg3, drag;
	// Update is called once per frame
	void Update () {
        mousePosX = Input.mousePosition.x;
        mousePosY = Input.mousePosition.y;

        //_k = InP.GetComponent<VertSpeedMeter>().k;
        GameObject goRC = GameObject.Find("GaugesPack");
        RCPtrs = goRC.GetComponents<RCPointer>();
        //a pointer coordinates of targets
        for (int i = 0; i < RCPtrs.Length; i++)
        {
            RCPtrs[i].trgPos = targPos[i];
        }
        //check the left mouse button is pressed
        if (Input.GetMouseButtonDown(0))
        {
            drag = true;
        }
        if (Input.GetMouseButtonUp(0))
        {
            drag = false;
        }
	}
    
    private float lastTimeH, lastTimeF;
    public bool hightT(float esval)
    {
        bool b = false;
        if (esval > 0.97f)
        {
            if (Time.time - lastTimeH > 75)
            {
                b=true;
            }
        }
        else
        {
            lastTimeH = Time.time;
        }
        return b;
    }
    
    public bool fire(bool ht)
    {
        bool f = false;
        if (ht)
        {
            if (Time.time - lastTimeF > 35)
            {
                f = true;
            }
        }
        else
        {
            lastTimeF = Time.time;
        }
        return f;
    }
    
    public bool engStop(float esval)
    {
        bool h = false;
        if (esval == 0)
        {
            h = true;
        }
        else
        {
            h = false;
        }
        return h;
    }
    
    private string lastTooltip = "";
    
    private void OnGUI()
    {
        GUIStyle InstrPackStyle = new GUIStyle();
        Color color = new Color(0.2f, 0.8f, 0.75f, 1f);
        InstrPackStyle.name = "InstrumentalPack";
        InstrPackStyle.font = mFont;
        InstrPackStyle.normal.textColor = color;
        //VerticalSpeedMeter
        VSMtextPosX = InP.GetComponent<VertSpeedMeter>().posWX;
        VSMtextPosY = InP.GetComponent<VertSpeedMeter>().posWY;
        _k = InP.GetComponent<VertSpeedMeter>().k;
        InstrPackStyle.fontSize = (int) (_k*15);
        GUI.Label(new Rect(Screen.width / 2 + VSMtextPosX + (128 - ((TPX) / 2) * _k),
            Screen.height/2 + VSMtextPosY + (128 - ((TPY) / 2) * _k),
            TPX / 2 * _k, TPY / 2 * _k),
            "VERTICALSPEEDMETER", InstrPackStyle);
        //SpeedMeter
        SMtextPosX = InP.GetComponent<SpeedMeter>().posWX;
        SMtextPosY = InP.GetComponent<SpeedMeter>().posWY;
        _k = InP.GetComponent<SpeedMeter>().k;
        InstrPackStyle.fontSize = (int)(_k * 15);
        GUI.Label(new Rect(Screen.width / 2 + SMtextPosX + (128 - ((TPX1) / 2) * _k),
            Screen.height / 2 + SMtextPosY + (128 - ((TPY1) / 2) * _k),
            TPX1 / 2 * _k, TPY1 / 2 * _k), "SPEEDMETER", InstrPackStyle);
        //AltMeter
        AMtextPosX = InP.GetComponent<AltMeter>().posWX;
        AMtextPosY = InP.GetComponent<AltMeter>().posWY;
        _k = InP.GetComponent<AltMeter>().k;
        InstrPackStyle.fontSize = (int)(_k * 15);
        GUI.Label(new Rect(Screen.width / 2 + AMtextPosX + (128 - ((TPX2) / 2) * _k),
            Screen.height / 2 + AMtextPosY + (128 - ((TPY2) / 2) * _k),
            TPX2 / 2 * _k, TPY2 / 2 * _k), "ALTMETER", InstrPackStyle);
        //AviaHorizont
        AHtextPosX = InP.GetComponent<AviaHorizont>().posWX;
        AHtextPosY = InP.GetComponent<AviaHorizont>().posWY;
        _k = InP.GetComponent<AviaHorizont>().k;
        InstrPackStyle.fontSize = (int)(_k * 15);
        GUI.Label(new Rect(Screen.width / 2 + AHtextPosX + (128 - ((TPX3) / 2) * _k),
            Screen.height / 2 + AHtextPosY + (128 - ((TPY3) / 2) * _k),
            TPX3 / 2 * _k, TPY3 / 2 * _k), "AVIAHORIZON", InstrPackStyle);
        //RadioCompass
        RCtextPosX = InP.GetComponent<RadioCompass>().posWX;
        RCtextPosY = InP.GetComponent<RadioCompass>().posWY;
        _k = InP.GetComponent<RadioCompass>().k;
        InstrPackStyle.fontSize = (int)(_k * 15);
        GUI.Label(new Rect(Screen.width / 2 + RCtextPosX + (128 - ((TPX4) / 2) * _k),
            Screen.height / 2 + RCtextPosY + (128 - ((TPY4) / 2) * _k),
            TPX4 / 2 * _k, TPY4 / 2 * _k), "RADIOCOMPASS", InstrPackStyle);        
        //EngineSpeedMeter
        EStextPosX = InP.GetComponent<EngineSpeedMeter>().posWX;
        EStextPosY = InP.GetComponent<EngineSpeedMeter>().posWY;
        _k = InP.GetComponent<EngineSpeedMeter>().k;
        InstrPackStyle.fontSize = (int)(_k * 15);
        GUI.Label(new Rect(Screen.width / 2 + EStextPosX + (128 - ((TPX5) / 2) * _k),
            Screen.height / 2 + EStextPosY + (128 - ((TPY5) / 2) * _k),
            TPX5 / 2 * _k, TPY5 / 2 * _k), "ENGINESPEEDMETER", InstrPackStyle);
        //FuelMeter
        FMtextPosX = InP.GetComponent<FuelMeter>().posWX;
        FMtextPosY = InP.GetComponent<FuelMeter>().posWY;
        _k = InP.GetComponent<FuelMeter>().k;
        InstrPackStyle.fontSize = (int)(_k * 15);
        GUI.Label(new Rect(Screen.width / 2 + FMtextPosX + (128 - ((TPX6) / 2) * _k),
            Screen.height / 2 + FMtextPosY + (128 - ((TPY6) / 2) * _k),
            TPX6 / 2 * _k, TPY6 / 2 * _k), "FUELMETER", InstrPackStyle);
        //group adjustments
        GUI.BeginGroup(new Rect(0, 0, 512, 256));
        //
        InstrPackStyle.fontSize = 15;
        //1
        GUI.Label(new Rect(20, 30, 20, 20), "SPEEDMETER", InstrPackStyle);
        smValue = GUI.HorizontalSlider(new Rect(130, 30, 100, 20), smValue, 0, 100);
        InP.GetComponent<SpeedMeter>().SpeedValue = smValue;
        //2
        GUI.Label(new Rect(20, 50, 20, 20), "ALTMETER", InstrPackStyle);
        amValue = GUI.HorizontalSlider(new Rect(130, 50, 100, 20), amValue, 0, 10);
        InP.GetComponent<AltMeter>().AltMValue = amValue;
        //3
        GUI.Label(new Rect(20, 70, 20, 20), "AVIAHORIZON", InstrPackStyle);
        GUI.Label(new Rect(120, 70, 20, 20), "TANGAGE", InstrPackStyle);
        GUI.Label(new Rect(120, 90, 20, 20), "BANK", InstrPackStyle);
        ahValue1 = GUI.HorizontalSlider(new Rect(180, 70, 100, 20), ahValue1, -1, 1);
        ahValue2 = GUI.HorizontalSlider(new Rect(180, 90, 100, 20), ahValue2, -1, 1);
        InP.GetComponent<AviaHorizont>().scaleK = ahValue1;
        InP.GetComponent<AviaHorizont>().vTv = ahValue2;
        //4
        GUI.Label(new Rect(20, 110, 20, 20), "VERTICALSPEEDMETER", InstrPackStyle);
        vsmValue = GUI.HorizontalSlider(new Rect(180, 110, 100, 20), vsmValue, -55, 55);
        InP.GetComponent<VertSpeedMeter>().VertSpeedValue = vsmValue;
        //5
        GUI.Label(new Rect(20, 130, 20, 20), "ENGINESPEEDMETER", InstrPackStyle);
        esValue = GUI.HorizontalSlider(new Rect(180, 130, 100, 20), esValue, 0, 1);
        InP.GetComponent<EngineSpeedMeter>().RpmValue = esValue;
        //6
        GUI.Label(new Rect(10, 160, 20, 20), txt, InstrPackStyle);
        GUI.EndGroup();
        //add beacons
        AddBeacon(0, mouseOver, rs1, rs0, rs2, "rsWin", InstrPackStyle);
        AddBeacon(1, mouseOver1, rs1, rs0, rs2, "rsWin1", InstrPackStyle);
        AddBeacon(2, mouseOver2, rs1, rs0, rs2, "rsWin2", InstrPackStyle);
        AddBeacon(3, mouseOver3, rs1, rs0, rs2, "rsWin3", InstrPackStyle);
        AddBeacon(4, mouseOver4, rs1, rs0, rs2, "rsWin4", InstrPackStyle);
        //add targets
        AddTarget(0, tgA1, tgA0, tgA2, mouseOverTg0, "rtWin0", InstrPackStyle);
        AddTarget(1, tgB1, tgB0, tgB2, mouseOverTg1, "rtWin1", InstrPackStyle);
        AddTarget(2, tgC1, tgC0, tgC2, mouseOverTg2, "rtWin2", InstrPackStyle);
        AddTarget(3, tgD1, tgD0, tgD2, mouseOverTg3, "rtWin3", InstrPackStyle);
        //
        if (Event.current.type == EventType.Repaint && GUI.tooltip != lastTooltip)
        {
            if (lastTooltip != "")
                SendMessage(lastTooltip + "OnMouseOut", SendMessageOptions.DontRequireReceiver);

            if (GUI.tooltip != "")
                SendMessage(GUI.tooltip + "OnMouseOver", SendMessageOptions.DontRequireReceiver);

            lastTooltip = GUI.tooltip;
        }
        //annunciator panel elements on/off
        InP.GetComponent<AnnunciatorPnl>().hightT = hightT(esValue);
        InP.GetComponent<AnnunciatorPnl>().fire = fire(hightT(esValue));
        InP.GetComponent<AnnunciatorPnl>().engStop = engStop(esValue);
    }
    //
    private void rsWinOnMouseOver()
    {
        Debug.Log("Got focus. Press left mouse button end drage");
        mouseOver = true;
        //beaconN = 0;
    }
    private void rsWinOnMouseOut()
    {
        Debug.Log("Focus lost");
        mouseOver = false;
        AnnPnl.GetComponent<AnnunciatorPnl>().bc[0] = false;
    }
    //
    private void rsWin1OnMouseOver()
    {
        Debug.Log("Got focus. Press left mouse button end drage");
        mouseOver1 = true;
        //beaconN = 1;
    }
    private void rsWin1OnMouseOut()
    {
        Debug.Log("Focus lost");
        mouseOver1 = false;
        AnnPnl.GetComponent<AnnunciatorPnl>().bc[1] = false;
    }

    private void rsWin2OnMouseOver()
    {
        Debug.Log("Got focus. Press left mouse button end drage");
        mouseOver2 = true;
        //beaconN = 2;
    }
    private void rsWin2OnMouseOut()
    {
        Debug.Log("Focus lost");
        mouseOver2 = false;
        AnnPnl.GetComponent<AnnunciatorPnl>().bc[2] = false;
    }
    //
    private void rsWin3OnMouseOver()
    {
        Debug.Log("Got focus. Press left mouse button end drage");
        mouseOver3 = true;
        //beaconN = 3;
    }
    private void rsWin3OnMouseOut()
    {
        Debug.Log("Focus lost");
        mouseOver3 = false;
        AnnPnl.GetComponent<AnnunciatorPnl>().bc[3] = false;
    }
    //
    private void rsWin4OnMouseOver()
    {
        Debug.Log("Got focus. Press left mouse button end drage");
        mouseOver4 = true;
        //beaconN = 4;
    }
    private void rsWin4OnMouseOut()
    {
        Debug.Log("Focus lost");
        mouseOver4 = false;
        AnnPnl.GetComponent<AnnunciatorPnl>().bc[4] = false;
    }
    private void rtWin0OnMouseOver()
    {
        Debug.Log("Got focus. Press left mouse button end drage");
        mouseOverTg0 = true;
    }
    private void rtWin0OnMouseOut()
    {
        Debug.Log("Focus lost");
        mouseOverTg0 = false;
        AnnPnl.GetComponent<AnnunciatorPnl>().tgtLocked = false;
        RCPtrs[0].tgtLock = false;
    }
    //
    private void rtWin1OnMouseOver()
    {
        Debug.Log("Got focus. Press left mouse button end drage");
        mouseOverTg1 = true;
    }
    private void rtWin1OnMouseOut()
    {
        Debug.Log("Focus lost");
        mouseOverTg1 = false;
        AnnPnl.GetComponent<AnnunciatorPnl>().tgtLocked = false;
        RCPtrs[1].tgtLock = false;
    }
    //
    private void rtWin2OnMouseOver()
    {
        Debug.Log("Got focus. Press left mouse button end drage");
        mouseOverTg2 = true;
    }
    private void rtWin2OnMouseOut()
    {
        Debug.Log("Focus lost");
        mouseOverTg2 = false;
        AnnPnl.GetComponent<AnnunciatorPnl>().tgtLocked = false;
        RCPtrs[2].tgtLock = false;
    }
    //
    private void rtWin3OnMouseOver()
    {
        Debug.Log("Got focus. Press left mouse button end drage");
        mouseOverTg3 = true;
    }
    private void rtWin3OnMouseOut()
    {
        Debug.Log("Focus lost");
        mouseOverTg3 = false;
        AnnPnl.GetComponent<AnnunciatorPnl>().tgtLocked = false;
        RCPtrs[3].tgtLock = false;
    }
    //
    void AddBeacon(int bcN, bool mouseOvr, Texture2D texBcn1, Texture2D texBcn0, Texture2D texBcn2,
        string tltp, GUIStyle bgs)
    {
        GUI.Label(new Rect(bcX[bcN] - 25, bcY[bcN] - 37, 60, 50), "BEACON " + (bcN+1).ToString("f0"), bgs);
        if (mouseOvr)
        {
            GUI.Label(new Rect(bcX[bcN] - 25, bcY[bcN] - 22, 50, 50), new GUIContent(texBcn1, tltp));
        }
        else
        {
            GUI.Label(new Rect(bcX[bcN] - 25, bcY[bcN] - 22, 50, 50), new GUIContent(texBcn0, tltp));
        }
        if (mouseOvr && drag)
        {
            bcX[bcN] = mousePosX;
            bcY[bcN] = H - mousePosY;
            AnnPnl.GetComponent<RadioCompass>().x = bcX[bcN];
            AnnPnl.GetComponent<RadioCompass>().y = bcY[bcN];
            //RCValue = AngCalc(Xrel, Yrel);
            //if (RCValue < 0)
            //{
            //    RCValue = 360 + RCValue;
            //}
            float PosWX = AnnPnl.GetComponent<RadioCompass>().posWX;
            float PosWY = AnnPnl.GetComponent<RadioCompass>().posWY;
            float distX = bcX[bcN] + 25 - (Screen.width / 2 + PosWX + 128 + 32);
            float distY = (Screen.height / 2 + PosWY + 128 + 32) - bcY[bcN] - 22;
            GUI.Label(new Rect(bcX[bcN] - 25, bcY[bcN] - 22, 50, 50), new GUIContent(texBcn2, tltp));
            AnnPnl.GetComponent<AnnunciatorPnl>().bc[bcN] = true;
            AnnPnl.GetComponent<AnnunciatorPnl>().beaconDist[bcN] =
                Mathf.Sqrt(distX * distX + distY * distY);
        }
    }
    void AddTarget(int tgN, Texture2D tgTex1, Texture2D tgTex0, Texture2D tgTex2, 
        bool tgMouseOver, string tgTltp, GUIStyle tgs)
    {
        GUI.Label(new Rect(targPos[tgN].x - 25, targPos[tgN].y - 37, 60, 50), "TARGET " + (tgN + 1).ToString("f0"), tgs);
        if (tgMouseOver)
        {
            GUI.Label(new Rect(targPos[tgN].x - 25, targPos[tgN].y - 22, 50, 50),
                new GUIContent(tgTex1, tgTltp));
        }
        else
        {
            GUI.Label(new Rect(targPos[tgN].x - 25, targPos[tgN].y - 22, 50, 50),
                new GUIContent(tgTex0, tgTltp));
        }
        if (tgMouseOver && drag)
        {
            targPos[tgN].x = mousePosX;
            targPos[tgN].y = H - mousePosY;
            GUI.Label(new Rect(targPos[tgN].x - 25, targPos[tgN].y - 22, 50, 50),
                new GUIContent(tgTex2, tgTltp));
            AnnPnl.GetComponent<AnnunciatorPnl>().targetNum = tgN;
            AnnPnl.GetComponent<AnnunciatorPnl>().tgtLocked = true;
            RCPtrs[tgN].tgtLock = true;
        }
    }
}
