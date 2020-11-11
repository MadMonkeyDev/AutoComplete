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
        static void Main(string[] args)
        {

            try
            {
                // Подключаемся к указанному в параметрах серверу
                _Client.Connect(args[0], int.Parse(args[1]));
            }
            catch(Exception e)
            {
                Console.WriteLine($"Ошибка! {e.Message}");
            }

            while (true)
            {
                Console.Write("> ");
                string Request = Console.ReadLine();

                if(Request.Length > 0)
                {
                    _Client.Send($"get {Request}");

                    string Responce = _Client.RecieveResponce();
                    if (Responce != "")
                    {
                        Console.Write(Responce);
                    }
                }              
            }
        }
        
    }
}
