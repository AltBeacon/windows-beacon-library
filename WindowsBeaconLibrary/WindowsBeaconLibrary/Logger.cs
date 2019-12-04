using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Altbeacon
{
    public class Logger
    {
        public static String LogFilePath = @"C:\Users\Public\logfile.log";
        private static String BufferedLines = "";
        private static DateTime LastFileWriteTime = DateTime.Now;
        public static void SetLogFilePath(String path)
        {
            LogFilePath = path;
        }
        public static void ClearLogfile()
        {
            System.IO.File.Delete(LogFilePath);
        }
        public static void Flush()
        {
            LastFileWriteTime = DateTime.Now;
            String linesToWrite = BufferedLines;
            BufferedLines = "";
            using (System.IO.StreamWriter file =
                new System.IO.StreamWriter(LogFilePath, true))
            {
                file.WriteLine(linesToWrite);
            }
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public static void Log(String level, String line)
        {
            String context = getContext();
            String logLine = String.Format("{0} {1} {2} {3}", DateTime.Now.ToString("HH:mm:ss.FFF"), context, level, line);
            Console.WriteLine(logLine);
            BufferedLines += (logLine +"\n");
            var startTime = DateTime.Now;
            if ((DateTime.Now - LastFileWriteTime).TotalSeconds > 10 || BufferedLines.Length > 10000)
            {
                Flush();
            }
            var endTime = DateTime.Now;
            var elapsed = (endTime - startTime).TotalMilliseconds;
            if (elapsed > 100)
            {
               BufferedLines +=("******************* Log line write took " + elapsed + " milliseconds\n");
            }
        }

        public static void Debug(String line)
        {
            Log("DEBUG", line);
        }

        public static void Warn(String line)
        {
            Log("WARN", line);
        }

        public static void Error(String line)
        {
            Log("ERROR", line);
        }
        private static String getContext()
        {
            String source = "UNKNOWN";
            try
            {
                throw new Exception();
            }
            catch (Exception)
            {
                // Create a StackTrace that captures
                // filename, line number, and column
                // information for the current thread.
                StackTrace st = new StackTrace(true);
                if (st.FrameCount >= 4) {
                    StackFrame sf = st.GetFrame(3);
                    String[] fileParts = sf.GetFileName().Split('\\');
                    String file = fileParts[fileParts.Length - 1];
                    String[] fileParts2 = file.Split('.');
                    String fileWithoutExt = fileParts2.Length > 1 ? fileParts2[fileParts2.Length - 2] : file;

                    source = fileWithoutExt +":"+sf.GetFileLineNumber();
                }
            }
            return source+":T"+Thread.CurrentThread.ManagedThreadId;
        }
    }
}
