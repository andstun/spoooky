using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

// Attempts to write all Debug lines 

public class InGameLogger : MonoBehaviour
{
    public TMP_Text logText;           // assign in inspector (inside the scrollview content)
    public ScrollRect scrollRect;  // assign in inspector (scrollview)

    private Queue<string> logLines = new Queue<string>();
    private const int maxLines = 100;

    void OnEnable()
    {
        Application.logMessageReceived += HandleLog;
    }

    void OnDisable()
    {
        Application.logMessageReceived -= HandleLog;
    }

    void HandleLog(string logString, string stackTrace, LogType type)
    {
        string line = logString;

        if (type == LogType.Error || type == LogType.Exception)
            line += "\n" + stackTrace;

        logLines.Enqueue(line);
        if (logLines.Count > maxLines)
            logLines.Dequeue();

        logText.text = string.Join("\n", logLines);
        Canvas.ForceUpdateCanvases(); // ensure layout updates
        scrollRect.verticalNormalizedPosition = 0f; // scroll to bottom
    }
}
