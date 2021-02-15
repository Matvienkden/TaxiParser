using System;
using System.Linq;
using TaxiParser.Core.Enums;
using TaxiParser.Core.Parsers;

namespace TaxiParser.App
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {

                if (args.Contains("volgograd"))
                {
                    Console.WriteLine("Парсинг такси для Волгоградской области");
                    var parser = new VolgogradParser(new ConsoleLogger(), SaveTo.File);
                    parser.Start();
                }
                else if (args.Contains("voronezh"))
                {
                    Console.WriteLine("Парсинг такси для Воронежской области");
                    var parser = new VoronezhParser(new ConsoleLogger(), SaveTo.File);
                    parser.Start();
                }
                else
                {
                    Console.WriteLine("volgograd - парсинг такси для Волгоградской области");
                    Console.WriteLine("voronezh - парсинг такси для Воронежской области");
                }

            }
            catch (Exception e)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(e.Message);
                Console.WriteLine(e.Source);
                Console.WriteLine(e.StackTrace);
                Console.ForegroundColor = ConsoleColor.White;
            }

            Console.ReadLine();
        }
    }
}
