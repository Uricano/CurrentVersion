using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cherries.Models.App
{
    public class Message
    {
        public LogLevel LogLevel { get; set; }

        public string Text { get; set; }
    }

    public enum LogLevel
    {
        Fatal = 0,
        Error = 1,
        Warning = 2,
        Info = 3
    }
}
