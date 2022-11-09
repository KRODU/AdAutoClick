using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdAutoClick.Util
{
    class Logger
    {
        private readonly bool writeLog;
        private readonly TextWriter logWriteStream;

        public Logger(bool writeLog)
        {
            this.writeLog = writeLog;

            logWriteStream = TextWriter.Synchronized(new StreamWriter(new BufferedStream(new FileStream("AdAutoClick.log", FileMode.Create, FileAccess.Write))));
        }

        public void WriteLog(string logMsg)
        {
            if (!writeLog)
                return;

            string logging = $"[{DateTime.Now:yy-MM-dd HH:mm:ss}] {logMsg}";
            Console.WriteLine(logging);
            logWriteStream.WriteLine(logging);
        }

        public void WriteLog(string logMsg, Exception ex)
        {
            if (!writeLog)
                return;

            WriteLog($"[ERROR] {logMsg}: {ex}");
        }

        public async Task FlushAsync()
        {
            if (!writeLog)
                return;

            await logWriteStream.FlushAsync();
        }

        public void Close()
        {
            logWriteStream.Close();
        }
    }
}
