using BepInEx.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DustDevilFramework
{
    /// <summary>
    /// Redirects log output to save to personalized files (based off of GUID
    /// and are saved in the BepInEx/logs folder)
    /// </summary>
    public class LogListener : ILogListener
    {
        public static LogListener Instance { get; private set; }
        //private static Dictionary<string, System.IO.StreamWriter> streams;
        private const string LOG_PATH = @"BepInEx\logs\";
        private const string extension = ".txt";
        private static void EnsureDirectory(string guid)
        {
            if (!System.IO.Directory.Exists(LOG_PATH))
            {
                System.IO.Directory.CreateDirectory(LOG_PATH);
            }
            if (!System.IO.File.Exists(LOG_PATH + guid + extension))
            {
                System.IO.File.Create(LOG_PATH + guid + extension);
            }
        }
        private static void ClearLogs()
        {
            if (!System.IO.Directory.Exists(LOG_PATH))
            {
                return;
            }
            foreach (string f in System.IO.Directory.GetFiles(LOG_PATH))
            {
                if (f.EndsWith(extension))
                {
                    try
                    {
                        System.IO.File.Delete(f);
                    } catch (System.IO.IOException)
                    {
                        // We just don't delete the file. Oh well.
                    }
                }
            }
        }
        public static void Log(string guid, object message)
        {
            EnsureDirectory(guid);

            //if (streams == null)
            //{
            //    streams = new Dictionary<string, System.IO.StreamWriter>();
            //}
            //if (!streams.ContainsKey(guid))
            //{
            //    streams.Add(guid, System.IO.File.AppendText(LOG_PATH + guid + extension));
            //    streams[guid].AutoFlush = true;
            //    streams[guid].NewLine = "\n\r";
            //}
            //streams[guid].WriteLine(message);
            //streams[guid].Flush();

            try
            {
                System.IO.File.AppendAllText(LOG_PATH + guid + extension, message + Environment.NewLine);
            } catch (System.IO.IOException)
            {
                // We can't write at the same time here...
                // Skip the log event.
            }
        }
        internal static void Create()
        {
            if (Instance == null)
            {
                Instance = new LogListener();
            }
        }
        private LogListener()
        {
            // Add ourselves as a listener to all events when we are constructed.
            Logger.Listeners.Add(this);
            ClearLogs();
        }

        public void LogEvent(object sender, LogEventArgs eventArgs)
        {
            Log(eventArgs.Source.SourceName, eventArgs.Data);
        }

        public void Dispose()
        {
            //foreach (System.IO.StreamWriter w in streams.Values)
            //{
            //    w.Close();
            //    w.Dispose();
            //}
        }
    }
}
