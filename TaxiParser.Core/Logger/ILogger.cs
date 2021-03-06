﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaxiParser.Core.Logger
{
    public interface ILogger
    {
        void Log(string message, LogLevel LogLevel = LogLevel.INFO);
    }

    public enum LogLevel
    {
        INFO,
        WARNING,
        ERROR,
        SUCCSESS
    }
}
