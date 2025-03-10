using UnityEngine;
using System.Collections;

public class RCAddDelPointer : MonoBehaviour {
    public Texture2D Tex;
    public int qtE;
    public int qtS;
    public bool s;
    //public GameObject[] goks;
    //public float winX, winY;
    private RadioCompass rc;

    // Use this for initialization
    private void Start()
    {

    }

    // Update is called once per frame
    private void Update()
    {
        GameObject rcGo = GameObject.Find("GaugesPack");
        rc = rcGo.GetComponent<RadioCompass>();
        s = false;
        //goks = GameObject.FindGameObjectsWithTag("enemy");
        //qtE = goks.Length;
        qtE = 4;
        if (qtS < qtE)
        {
            AddScr();
            qtS++;
        }
        if (qtS > qtE)
        {
            DelScr();
            s = true;
            qtS--;
        }
    }

    private void AddScr()
    {
        GameObject go = GameObject.Find("GaugesPack");
        RCPointer pt = go.AddComponent<RCPointer>();
        pt.pTex = Tex;
        pt.posWinX = rc.posWX;
        pt.posWinY = rc.posWY;
        //pt.kp = rc.k;
        RcEnable();
    }

    public void DelScr()
    {
        GameObject go = GameObject.Find("GaugesPack");
        Destroy(go.GetComponent<RCPointer>());
        s = false;
    }

    private void RcEnable()
    {
        GameObject gorc = GameObject.Find("GaugesPack");
        RadioCompass rc = gorc.GetComponent<RadioCompass>();
        rc.enabled = false;
        if (!rc.enabled)
        {
            rc.enabled = true;
        }
    }
}
