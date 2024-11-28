using System;
using System.IO;
using UnityEngine;

namespace CustomLogger{
    public static class FileLogger
    {
        private static string LogPath => $"{Application.persistentDataPath}/nreal_debug_log.txt";

        public static void Log(string message)
            {
                try
                {
                    string timeStamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
                    string logMessage = $"[{timeStamp}] {message}\n";
                    File.AppendAllText(LogPath, logMessage);
                }
                catch (Exception e)
                {
                    Debug.LogError($"Logging failed: {e.Message}");
                }
            }

            public static void ClearLog()
            {
                try
                {
                    if (File.Exists(LogPath))
                        File.Delete(LogPath);
                }
                catch (Exception e)
                {
                Debug.LogError($"Clear log failed: {e.Message}");
            }
        }
    }
}

