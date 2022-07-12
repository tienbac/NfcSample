using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace NfcSample.Settings
{
    internal class Utilities
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger("NFC");

        public static bool ActiveOperationLog = true;
        public static bool ActiveDebugLog = true;
        public static long ConvertToUnixTime(DateTime datetime)
        {
            DateTime sTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

            return (long)(datetime - sTime).TotalSeconds;
        }

        public static void StartLog(bool activeOperationLog, bool activeDebugLog)
        {
            ActiveOperationLog = activeOperationLog;
            ActiveDebugLog = activeDebugLog;
        }

        public static void CloseLog()
        {
            foreach (log4net.Appender.IAppender app in log.Logger.Repository.GetAppenders())
            {
                app.Close();
            }
        }

        public static void WriteErrorLog(string logtype, string logcontent)
        {
            try
            {
                log.Error($"{logtype} \t {logcontent}");
            }
            catch
            {
                // ignored
            }
        }

        public static void WriteOperationLog(string logtype, string logcontent)
        {
            if (!ActiveOperationLog)
                return;

            try
            {
                log.Info($"{logtype} \t {logcontent}");
            }
            catch
            {
                // ignored
            }
        }

        public static void WriteDebugLog(string logtype, string logcontent)
        {
            if (!ActiveDebugLog)
                return;

            try
            {
                log.Debug($"{logtype} \t {logcontent}");
            }
            catch
            {
                // ignored
            }
        }

        public static bool IsFileLocked(FileInfo file)
        {
            FileStream stream = null;

            try
            {
                stream = file.Open(FileMode.Open, FileAccess.Read, FileShare.None);
            }
            catch (IOException)
            {
                //the file is unavailable because it is:
                //still being written to
                //or being processed by another thread
                //or does not exist (has already been processed)
                return true;
            }
            finally
            {
                if (stream != null)
                    stream.Close();
            }

            //file is not locked
            return false;
        }

        public static bool ValidateIPv4(string ipString)
        {
            if (String.IsNullOrWhiteSpace(ipString))
            {
                return false;
            }

            string[] splitValues = ipString.Split('.');
            if (splitValues.Length != 4)
            {
                return false;
            }

            byte tempForParsing;

            return splitValues.All(r => byte.TryParse(r, out tempForParsing));
        }

        public static void WriteFile(string data, string path, string uuid)
        {
            var filePath = Path.Combine(path, $"{uuid}_data_{DateTime.Now:yyyyMMddHHmmss}.json");
            if (!File.Exists(filePath))
            {
                File.Create(filePath).Close();
            }

            File.WriteAllText(filePath, $"{data.ToString()}");
        }

        public static int GetCount()
        {
            var path = Directory.GetCurrentDirectory();
            var filePath = Path.Combine(path, "CountRun.txt");
            if (!File.Exists(filePath))
            {
                File.Create(filePath).Close();
            }

            var data = File.ReadAllText(filePath);

            return string.IsNullOrEmpty(data) ? 0 : Convert.ToInt32(data);
        }

        public static string GetLinks(string message)
        {
            try
            {
                List<string> list = new List<string>();
                Regex urlRx = new Regex(@"((https?|ftp|file)\://|www.)[A-Za-z0-9\.\-]+(/[A-Za-z0-9\?\&\=;\+!'\(\)\*\-\._~%]*)*", RegexOptions.IgnoreCase);

                MatchCollection matches = urlRx.Matches(message);
                foreach (Match match in matches)
                {
                    list.Add(match.Value);
                }
                return list[0];
            }
            catch
            {
                return string.Empty;
            }
        }
    }
}
