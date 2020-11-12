using Server.Properties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    public class AsyncSocketListner
    {
        private static AsyncSocketListner _Instance { get; set; }
        public static AsyncSocketListner Instance => _Instance ?? (_Instance = new AsyncSocketListner());

        private static readonly Socket _ServerSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        private static readonly List<Socket> _ClientSockets = new List<Socket>();
        private const int BUFFER_SIZE = 1024;
        private const int DEFAULT_SERVER_PORT = 5555;
        private static readonly byte[] _Buffer = new byte[BUFFER_SIZE];

        private Encoding _Encoder = Encoding.UTF8;        

        /// <summary>
        /// Запуск TCP севрера
        /// </summary>
        public void Start(int Port = DEFAULT_SERVER_PORT)
        {
            try
            {
                _ServerSocket.Bind(new IPEndPoint(IPAddress.Any, Port));
                _ServerSocket.Listen(0);
                _ServerSocket.BeginAccept(AcceptCallback, null);
            }
            catch (Exception e)
            {
                Program.ConsoleWrite($"Ошибка! {e.Message}", 3);
            }
        }

        /// <summary>
        /// Подключение и регистрация нового клиента
        /// </summary>
        /// <param name="ar"></param>
        private void AcceptCallback(IAsyncResult ar)
        {
            Socket _Socket;

            try
            {
                _Socket = _ServerSocket.EndAccept(ar);
            }
            catch (ObjectDisposedException)
            {
                return;
            }

            _ClientSockets.Add(_Socket);
            _Socket.BeginReceive(_Buffer, 0, BUFFER_SIZE, SocketFlags.None, ReceiveCallBack, _Socket);
            _ServerSocket.BeginAccept(AcceptCallback, null);
        }

        /// <summary>
        /// Прием сообщений от клиентов с последующей отправкой ответа
        /// </summary>
        /// <param name="ar"></param>
        private void ReceiveCallBack(IAsyncResult ar)
        {
            Socket _CurrentSocket = (Socket)ar.AsyncState;
            int Recieved;

            try
            {
                Recieved = _CurrentSocket.EndReceive(ar);
            }
            catch (SocketException)
            {
                _CurrentSocket.Close();
                _ClientSockets.Remove(_CurrentSocket);

                return;
            }

            byte[] _RecievedBuffer = new byte[Recieved];
            Array.Copy(_Buffer, _RecievedBuffer, Recieved);
            string Message = _Encoder.GetString(_RecievedBuffer);

            MessageProcessing(_CurrentSocket, Message);

            _CurrentSocket.BeginReceive(_Buffer, 0, BUFFER_SIZE, SocketFlags.None, ReceiveCallBack, _CurrentSocket);
        }

        /// <summary>
        /// Обрабатываем входящий запрос от клиента
        /// </summary>
        /// <param name="CurrentSocket">Текущее клиент, выполняющий запрос серверу</param>
        /// <param name="Message">Отправленное сообщение клиентом</param>
        public void MessageProcessing(Socket CurrentSocket, string Message)
        {
            string[] ArrayOfMessage = Message.Split(' ');

            string Responce = "resp:";

            switch (ArrayOfMessage[0])
            {
                // обработка комманды get <префикс>. Возвращает слова по указанному префиксу.
                case "get":
                    Responce += SQLiteDB.Instance.GetWords(ArrayOfMessage[1]);
                    break;
            }

            CurrentSocket.Send(_Encoder.GetBytes(Responce));
        }

        /// <summary>
        /// Закрытие все покдлючений к серверу
        /// </summary>
        public void CloseAllSockets()
        {
            foreach(Socket ClientSocket in _ClientSockets)
            {
                ClientSocket.Send(_Encoder.GetBytes("Сервер выключен. Соединение разорванно!"));
                ClientSocket.Shutdown(SocketShutdown.Both);
                ClientSocket.Close();
            }
        }

        /// <summary>
        /// Возвращает текущий порт, который используется TCP сервером
        /// </summary>
        /// <returns></returns>
        public int GetSocketServerPort()
        {
            return (int)((IPEndPoint)_ServerSocket.LocalEndPoint).Port;
        }

    }
}
