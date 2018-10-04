using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TFI.BusinessLogic.Interfaces
{
    public interface IErrorHandler : IBaseBL
    {
        void LogInfo(Exception ex);
        void LogInfo(string sMessage);
        void LogInfo(string sMessage, StackTrace errTrace);

    }
}
