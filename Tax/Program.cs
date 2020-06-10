using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Tax
{
    class Program
    {
        static void Main(string[] args)
        {
            Start();
        }

        public static void Start()
        {            
            Console.WriteLine("Запуск");

            int stream = int.Parse(ConfigurationManager.AppSettings["stream"]);

            for (int i = 1; i <= stream; i++)
            {
                new Task(() => new Loading().Load()).Start();
            }

            if (Console.ReadKey().Key != ConsoleKey.Escape)
                Console.WriteLine("Операция прервана");            

        }
    }
}
