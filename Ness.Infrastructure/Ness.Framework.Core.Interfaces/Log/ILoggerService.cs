using System;

namespace Framework.Core.Interfaces.Log
{
    using PostSharp.Aspects;

    public interface ILoggerService
    {
        void Info(string message);
        void Info(object value);
        void Info(string message, Arguments args);
        // void Info(string format, params object[] args);
        void Info(Arguments args);
        void Warn(string message);
        // void Warn(object value);
        // void Warn(string format, params object[] args);
        void Debug(string message);
        // void Debug(object value);
        // void Debug(string format, params object[] args);
        void Error(string message);
        void Error(Exception ex);
        void Error(object value);
        void Error(Arguments args);
        //void Error(string format, params object[] args);
        void Fatal(string message);
        void Fatal(Exception ex);
        //void Fatal(object value);
        //void Fatal(string format, params object[] args);
    }
}
