using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaxiParser.Core.Logger;

namespace TaxiParser
{
    public class ConsoleLogger : ILogger
    {

        public void Log(string message, LogLevel LogLevel = LogLevel.INFO)
        {
            switch (LogLevel)
            {
                case LogLevel.INFO: 
                    Console.ForegroundColor = ConsoleColor.White;
                    break;
                case LogLevel.WARNING:
                case LogLevel.ERROR: 
                    Console.ForegroundColor = ConsoleColor.Red;
                    break;
                case LogLevel.SUCCSESS:
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    break;
                default:
                    throw new ArgumentException(nameof(LogLevel));
            }

            Console.WriteLine(message);
        }
    }
}
