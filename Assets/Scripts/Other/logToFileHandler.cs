using System;
using UnityEngine;
using System.Collections;
using Object = UnityEngine.Object;
using System.IO;

public class logToFileHandler : ILogHandler {
    public string logPath = "";

    public logToFileHandler(string dateTimeString) {
        if (logPath == "") {
            string d = Path.Combine(System.Environment.GetFolderPath(
              System.Environment.SpecialFolder.Desktop), "CouzinAlgorithmsLogs");
            System.IO.Directory.CreateDirectory(d);
            logPath = Path.Combine(d, String.Format("{0}.txt", dateTimeString));
        }
        // logPath = Path.Combine(Application.dataPath, "Logs", String.Format("{0}.txt", dateTimeString));
        File.AppendAllText(logPath, String.Format("Logs ({0})\n", dateTimeString));
    }

    public void InsertSeparator() {
        File.AppendAllText(logPath, "-----------------------------------------------\n");
    }

    public void LogFormat(LogType logType, Object context, string format, params object[] args) {
        File.AppendAllText(logPath, String.Format(format, args));
    }
    
    public void LogException(Exception exception, Object context) {
        Debug.unityLogger.LogException(new Exception("Error: ", exception), context);
    }
}