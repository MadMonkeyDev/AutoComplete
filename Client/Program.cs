using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using Client.Properties;
using System.IO;

namespace Client
{
    class Program
    {
        static AsyncSocketClient _Client = new AsyncSocketClient();
        public static bool ExitStatus = false;
        static void Main(string[] args)
        {            
            try
            {
                // Подключаемся к указанному в параметрах серверу
                _Client.Connect(args[0], int.Parse(args[1]));
                Console.Clear();
            }
            catch(Exception e)
            {
                Console.WriteLine($"Ошибка! {e.Message}");
            }

            while (!ExitStatus)
            {
                Console.Write("> ");
                string Request = ReadLineWithExit();

                if(Request.Length > 0)
                {
                    _Client.Send($"get {Request}");

                    string Responce = _Client.RecieveResponce();
                    if (Responce != "")
                    {
                        Console.Write(Responce);
                    }
                } else
                {
                    ExitStatus = true;
                }              
            }

            Console.WriteLine("Для завершения программы нажмите любую клавишу...");
            Console.ReadKey();
        }

        private static string ReadLineWithExit()
        {
            string Result = "";
            StringBuilder Buffer = new StringBuilder();

            ConsoleKeyInfo KeyInfo = Console.ReadKey(true);

            while (KeyInfo.Key != ConsoleKey.Enter && KeyInfo.Key != ConsoleKey.Escape)
            {
                Console.Write(KeyInfo.KeyChar);
                Buffer.Append(KeyInfo.KeyChar);
                KeyInfo = Console.ReadKey(true);

            }

            if (KeyInfo.Key == ConsoleKey.Escape) ExitStatus = true;

            if (KeyInfo.Key == ConsoleKey.Enter) Result = Buffer.ToString();

            Console.Write("\r\n");

            return Result;
        }
        
    }
}
