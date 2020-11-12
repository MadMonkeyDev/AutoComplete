using Client.Properties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Client
{
    public class AsyncSocketClient
    {
        private static readonly Socket _ClientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        private const int BUFFER_SIZE = 1024;

        private static Encoding _Encoder = Encoding.UTF8;
        /// <summary>
        /// Подключение к серверу
        /// </summary>
        /// <param name="HostName">Имя хоста</param>
        /// <param name="Port">Порт хоста</param>
        public void Connect(string HostName, int Port)
        {
            while (!_ClientSocket.Connected)
            {
                try
                {
                    IPAddress IPv4Address = Array.FindLast(Dns.GetHostEntry(HostName).AddressList, a => a.AddressFamily == AddressFamily.InterNetwork);
                    _ClientSocket.Connect(new IPEndPoint(IPv4Address, Port));
                }
                catch (Exception se)
                { 
                    Console.WriteLine($"Ошибка! {se.Message}");
                }
            }
        }

        /// <summary>
        /// Отправка сообщения серверу
        /// </summary>
        /// <param name="Message">Текст сообщения</param>
        public void Send(string Message)
        {
            try
            {
                byte[] Buffer = _Encoder.GetBytes(Message);
                _ClientSocket.Send(Buffer, 0, Buffer.Length, SocketFlags.None);
            }
            catch (SocketException e)
            {

            }
            catch (Exception e)
            {
                Console.WriteLine($"ERROR: {e.Message}");
            }
        }

        public string RecieveResponce()
        {
            string ResponcePrefix = "resp:";
            var Buffer = new byte[BUFFER_SIZE];
            int Recieved = _ClientSocket.Receive(Buffer, SocketFlags.None);

            if (Recieved == 0) return null;

            byte[] Data = new byte[Recieved];
            Array.Copy(Buffer, Data, Recieved);

            return (_Encoder.GetString(Data)).Remove(0, ResponcePrefix.Length);
        }

        public void Disconnect()
        {
            Send("!disconnect");
            _ClientSocket.Shutdown(SocketShutdown.Both);
            _ClientSocket.Close();
        }
    }
}
