using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaxiParser.Core;

namespace TaxiParser.App
{
    class Program
    {
        static void Main(string[] args)
        {
            var parser = new ParserService(new ConsoleLogger());
            parser.Start();
            Console.ReadLine();
        }
    }
}
