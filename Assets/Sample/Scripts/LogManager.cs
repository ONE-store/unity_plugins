using System;
using UnityEngine;
using UnityEngine.UI;

public class LogManager : MonoBehaviour
{
    public GameObject scrollLog;
    public GameObject logView;

    Text logText;
    bool isShowing;
    
    void Start()
    {
        logText = logView.GetComponent<Text>();
        isShowing = scrollLog.activeSelf;
    }

    public void Log(LogType type, string format, params object[] args)
    {
        string result = String.Format(format, args);

        Debug.unityLogger.LogFormat(type, format, args);

        if (logText != null)
            logText.text += "\n[" + System.DateTime.Now.ToString("hh:mm:ss") + "]::" + result;
    }

    public void ShowHideLog()
    {
        isShowing = !isShowing;
        scrollLog.SetActive(isShowing);
    }

    public void ClearLog()
    {
        if (logText != null)
            logText.text = "";
    }

}
