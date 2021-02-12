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
            try
            {
                var parser = new ParserService(new ConsoleLogger(), SaveTo.File);
                parser.Start();
            }
            catch (Exception e)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(e.Message);
                Console.WriteLine(e.Source);
                Console.WriteLine(e.StackTrace);
                Console.ForegroundColor = ConsoleColor.White;
            }

            
        }
    }
}
