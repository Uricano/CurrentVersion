using System;
using System.Text;

namespace Framework.Core.Log
{
    public class AppLoggerUtil
    {
        public static string ParseSessionContextContainer(object scc)
        {
            return scc.ToString();
        }

        public static string ParseExceptionForLog(Exception ex)
        {
            var sb = new StringBuilder();
            while (ex != null)
            {
                sb.AppendLine(ex.Message);
                sb.Append(ex.StackTrace);
                sb.AppendLine("------------");
                ex = ex.InnerException;
            }
            return sb.ToString();
        }

        public static string ParseStringFormessage(string message)
        {
            var sb = new StringBuilder();
            sb.AppendLine(message);
            return sb.ToString();
        }
    }
}
