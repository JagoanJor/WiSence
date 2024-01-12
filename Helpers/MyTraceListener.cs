using System;
using System.Diagnostics;
using System.IO;

namespace API.Helpers
{
    public class MyTraceListener : TraceListener
    {
        private InvokerOutputTraceMessage listener;

        public MyTraceListener() { this.listener = null; }
        public MyTraceListener(InvokerOutputTraceMessage listener) { this.listener = listener; }

        private void OutputToFile(string msg)
        {
            try
            {
                var path = string.Format("{0}traceLogs\\{1:yyyMM}\\"
                    , AppDomain.CurrentDomain.SetupInformation.ApplicationBase
                    , DateTime.Today);
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);

                using (var writer = new StreamWriter(path + DateTime.Now.AddMinutes(-2).ToString("yyyyMMdd") + ".txt", true))
                {
                    writer.WriteLine(string.Format("[{0:HH:mm:ss.ffff}] {1}", DateTime.Now.AddMinutes(-2), msg));
                    writer.Close();
                }
            }
            catch (Exception) { }
        }

        public override void Write(string msg, string cat)
        {
            Write(string.Format("{0}: {1}", cat, msg));
        }

        public override void Write(string msg)
        {
            this.OutputToFile(msg);
            Console.Write(msg);
            if (this.listener != null) this.listener(msg);
        }

        public override void WriteLine(string msg, string cat)
        {
            WriteLine(string.Format("{0}: {1}", cat, msg));
        }

        public override void WriteLine(string msg)
        {
            this.OutputToFile(msg);
            Console.WriteLine(msg);
            if (this.listener != null) this.listener(msg);
        }

        public delegate void InvokerOutputTraceMessage(string message);
    }
}

