using Server.Properties;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Server
{
    class Program
    {
        static AsyncSocketListner TCPServer = AsyncSocketListner.Instance;

        /// <summary>
        /// Форматированный вывод сообщений в консоль с указанием даты и времени
        /// </summary>
        /// <param name="Message">Сообщение для вывода</param>
        /// <param name="MessageType">
        /// Тип сообщения, в зависимости от типа дата и время выделяются другим цветом, отличное от сообщения.
        /// 0 - белый
        /// 1 - зеленый
        /// 2 - желтый
        /// 3 - красный
        /// </param>
        public static void ConsoleWrite(string Message, int MessageType = 0)
        {
            switch (MessageType)
            {
                case 1:
                    Console.ForegroundColor = ConsoleColor.Green;
                    break;
                case 2:
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    break;
                case 3:
                    Console.ForegroundColor = ConsoleColor.Red;
                    break;
                default:
                    Console.ForegroundColor = ConsoleColor.White;
                    break;
            }
            Console.Write($"[{DateTime.Now.ToShortDateString()} {DateTime.Now.ToLongTimeString()}] ");
            Console.ResetColor();
            Console.WriteLine(Message);
        }

        static void Main(string[] args)
        {
            bool ExitStatus = false;

            // Обработка параметров запуска
            try
            {
                // Подключение к базе
                SQLiteDB.Instance.Connect(args[0]);
                // Запуск TCP сервера
                TCPServer.Start(int.Parse(args[1]));
                ConsoleWrite($"Сервер запущен. Доступен по порту {TCPServer.GetSocketServerPort()}");
            }
            catch (Exception e)
            {
                ConsoleWrite($"Ошибка! {e.Message}", 3);
                ConsoleWrite("Проверьте параметры запуска сервера!", 3);
                ExitStatus = true;
            }

            while (!ExitStatus)
            {
                Console.Write("> ");
                string[] CommandWithArgs = Console.ReadLine().Split(' ');

                switch (CommandWithArgs[0])
                {
                    case "":
                    case "exit":
                        ConsoleWrite("Остановка сервера. Сбрасываем все подключения...", 2);
                        TCPServer.CloseAllSockets();
                        ExitStatus = true;
                        break;
                    // Команда Make Dictionary - создание словаря из файла
                    case "make-dict":
                        ConsoleWrite("Формирование нового словаря...", 1);
                        SQLiteDB.Instance.ClearDictionary();
                        MakeDictionary(CommandWithArgs.ElementAtOrDefault(1) ?? null);
                        break;
                    // Команда Update Dictionary - обновление существующего справочника из файла
                    case "update-dict":
                        ConsoleWrite("Дополнение словаря", 1);
                        MakeDictionary(CommandWithArgs.ElementAtOrDefault(1) ?? null);
                        break;
                    // Команда Clear Dictionary - отчистка существующего справочника
                    case "clear-dict":
                        ConsoleWrite("Отчистка текущего словаря...", 1);
                        SQLiteDB.Instance.ClearDictionary();
                        ConsoleWrite("Словарь отчищен!", 1);
                        break;
                    default:
                        Console.Write(SQLiteDB.Instance.GetWords(CommandWithArgs[0]));
                        break;
                }
            }

            Console.WriteLine("Для закрытия окна нажмите любую клавишу...");
            Console.ReadKey();
        }

        public static void MakeDictionary(string FilePath)
        {
            char[] SplitSybols = new char[] { ' ', ',', ';', ')', '(', ':', '.', '\r', '\n', '\t' };

            if (!File.Exists(FilePath))
            {
                Program.ConsoleWrite($"Ошибка! По указанному пути файл со словарем не был найден. Путь: {FilePath}", 3);
                return;
            }

            // Выгружаем содержимое файла в память
            string TempText = File.ReadAllText(FilePath, Encoding.UTF8);
            // Получаем необходимые слова для записи в базу данных
            var Words = from word in (TempText.Split(SplitSybols))
                        where word.Length >= Settings.Default.WordMinLenght & word.Length <= Settings.Default.WordMaxLenght
                        group word by word into grp
                        where grp.Count() >= 3
                        select new { grp.Key, Count = grp.Count() };

            foreach (var Word in Words)
            {
                SQLiteDB.Instance.InsertWord(Word.Key, Word.Count);
            }
        }
    }
}
