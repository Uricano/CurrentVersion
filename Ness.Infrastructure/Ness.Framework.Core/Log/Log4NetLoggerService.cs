using System;
using System.Diagnostics;
using log4net;
using PostSharp.Aspects;

namespace Framework.Core.Log
{
    using Utils;
    using Interfaces.Log;

    public class Log4NetLoggerService : ILoggerService
    {
        private ILog _logger;
        private static Object thisLock = new Object();

        public Log4NetLoggerService()
        {
            _logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
            //log4net.GlobalContext.Properties["SessionTransactionId"] = SessionStateUtility.GetHttpSessionStateFromContext(HttpContext.Current).SessionID;
            //// log4net.GlobalContext.Properties["userID"] = ...sessionContextContainer.userID;
        }

        public void Info(string message)
        {
            lock (thisLock)
            {
                message = AppLoggerUtil.ParseStringFormessage(message);
                _logger.Info(message);
            }
        }

        public void Info(object value)
        {
            lock (thisLock)
            {
                _logger.Info(GetCallingMethod(new StackTrace(1)));

                _logger.Info(SerializeHelper.CreateXML(value));
            }
        }

        public void Info(string message, Arguments args)
        {
            lock (thisLock)
            {
                foreach (var argument in args)
                    message += SerializeHelper.CreateXML(argument);

                _logger.Info(message);
            }
        }

        public void Info(Arguments args)
        {
            lock (thisLock)
            {
                var message = "";

                _logger.Info(GetCallingMethod(new StackTrace(2)));

                foreach (var argument in args)
                    message += SerializeHelper.CreateXML(argument);

                _logger.Info(message);
            }
        }

        public void Warn(string message)
        {
            lock (thisLock)
            {
                message = AppLoggerUtil.ParseStringFormessage(message);
                _logger.Warn(message);
            }
        }

        public void Debug(string message)
        {
            lock (thisLock)
            {
                message = AppLoggerUtil.ParseStringFormessage(message);
                _logger.Debug(message);
            }
        }

        public void Error(string message)
        {
            lock (thisLock)
            {
                message = AppLoggerUtil.ParseStringFormessage(message);
                _logger.Error(message);
            }
        }

        public void Error(object value)
        {
            lock (thisLock)
            {
                _logger.Error(GetCallingMethod(new StackTrace(1)));

                _logger.Error(SerializeHelper.CreateXML(value));
            }
        }

        public void Error(Arguments args)
        {
            lock (thisLock)
            {
                var message = string.Empty;

                _logger.Error(GetCallingMethod(new StackTrace(2)));

                foreach (var argument in args)
                    message += SerializeHelper.CreateXML(argument);

                _logger.Error(message);
            }
        }

        public void Error(Exception ex)
        {
            lock (thisLock)
            {
                string message = AppLoggerUtil.ParseExceptionForLog(ex);
                _logger.Error(message);
            }
        }

        public void Fatal(string message)
        {
            lock (thisLock)
            {
                message = AppLoggerUtil.ParseStringFormessage(message);
                _logger.Fatal(message);
            }
        }

        public void Fatal(Exception ex)
        {
            lock (thisLock)
            {
                string message = AppLoggerUtil.ParseExceptionForLog(ex);
                _logger.Fatal(message);
            }
        }

        private static string GetCallingMethod(StackTrace stack)
        {
            StackFrame frame = stack.GetFrame(0);
            string className = frame.GetMethod().DeclaringType.ToString();
            string functionName = frame.GetMethod().Name;
            return string.Format("{0}.{1}", className, functionName);
        }
    }
}
