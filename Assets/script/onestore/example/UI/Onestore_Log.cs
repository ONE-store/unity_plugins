using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Onestore_Log : MonoBehaviour
{
    //log가 표시될 GameObject
    public GameObject LogView;
    Text logText;


    public GameObject ScrollLog;
    bool showLog = false;
    public void ShowLog()
    {
        showLog = !showLog;
        ScrollLog.SetActive(showLog);
    }

    // Start is called before the first frame update
    void Start()
    {
        logText = LogView.GetComponent<Text>();
    }

    public void Log(string msg)
    {
        Log("Onestore_Log", msg);
    }

    public void Log(string tag, string msg)
    {      
        string logMsg = "[" + tag + "]" + msg;
        //Debug.Log(logMsg);

        if (logText != null)
        {
            logText.text += "\n" + System.DateTime.Now.ToString("hh:mm:ss") + ": " + logMsg;
        }
    }

    //log clear
    public void Clear()
    {
        if (logText != null)
        {
            logText.text = "";
        }
    }

    //log 줄바꿈
    public void AddEnter()
    {
        if (logText != null)
        {
            logText.text += "\n";
            ScrollLog.GetComponent<ScrollRect>().verticalNormalizedPosition = 0;
        }
    }
}
