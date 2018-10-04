using System.Diagnostics;

namespace Framework.Core.Log
{
    using Interfaces.Log;

    public class Log4NetConfig : ILogConfig
    {
        public void Start()
        {
            log4net.GlobalContext.Properties["ProcName"] = Process.GetCurrentProcess().ProcessName;

            log4net.Config.XmlConfigurator.Configure();
        }
    }
}
