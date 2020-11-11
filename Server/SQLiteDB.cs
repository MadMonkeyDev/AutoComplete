using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

using System.Data.SQLite;
using System.Data.SqlClient;
using Server.Properties;
using System.Diagnostics;

namespace Server
{
    public class SQLiteDB
    {
        private static SQLiteDB _Instance { get; set; }
        public static SQLiteDB Instance => _Instance ?? (_Instance = new SQLiteDB());

        /// <summary>
        /// Подключение к базе данных
        /// </summary>
        private static SQLiteConnection _DBConnect;

        /// <summary>
        /// Описание комманд к базе данных
        /// </summary>
        public static SQLiteCommand ValidateTables, CreateTables, InsertRecord, SelectWords, ClearTable;        

        /// <summary>
        /// Конструктор экземпляра класса SQLiteDB. Вспомогательный класс для работы с базой данных в рамках проекта
        /// </summary>
        /// <param name="Path">Полный путь до файла базы данных. Если не заполненно, то создается файл по умолчанию WordsDictionary.sqlite</param>
        public void Connect(string DBPath = null)
        {
            // Инициализация файла базы данных
            if (!File.Exists(DBPath))
            {
                SQLiteConnection.CreateFile(DBPath);
            }

            // Инициализация подключения к базе данных
            _DBConnect = new SQLiteConnection($"Data Source={DBPath};");
            try
            {
                // Открытие подключения и инициализация комманд
                _DBConnect.Open();
                SQliteCommandsInit();

                // Проверки и предустановки
                DBSetup();
            }
            catch (Exception e)
            {
                Program.ConsoleWrite($"Ошибка: {e.Message}");
                _DBConnect.Close();
                return;
            }  
        }

        /// <summary>
        /// Инициализация комманд к базе данных
        /// </summary>
        private static void SQliteCommandsInit()
        {
            ValidateTables = new SQLiteCommand(SQLiteCommands.ValidateTables, _DBConnect);
            CreateTables = new SQLiteCommand(SQLiteCommands.CreateTables, _DBConnect);
            InsertRecord = new SQLiteCommand(SQLiteCommands.InsertRecord, _DBConnect);
            SelectWords = new SQLiteCommand(SQLiteCommands.SelectWords, _DBConnect);
            ClearTable = new SQLiteCommand(SQLiteCommands.ClearTable, _DBConnect);
        }

        /// <summary>
        /// Инициализация первоначальной структуры базы данных
        /// </summary>
        /// <param name="Path">Путь до файоа базы данных</param>
        private static void DBSetup()
        {
            try
            {
                SQLiteDataReader Result = ValidateTables.ExecuteReader();
                if (!Result.HasRows)
                {
                    CreateTables.ExecuteNonQuery();
                }
            }
            catch (Exception e)
            {
                Program.ConsoleWrite($"Ошибка: {e.Message}", 3);
                _DBConnect.Close();
                return;
            }            
        }

        /// <summary>
        /// Внесение слова в словарь
        /// </summary>
        /// <param name="Word">Слово</param>
        /// <param name="Count">Кол-во повторений</param>
        public void InsertWord(string Word, int Count)
        {
            try
            {
                InsertRecord.Parameters.AddWithValue("@word", Word);
                InsertRecord.Parameters.AddWithValue("@countofreplayes", Count);
                InsertRecord.ExecuteNonQuery();
            }
            catch(Exception e)
            {
                Program.ConsoleWrite($"Ошибка: {e.Message}", 3);
                return;
            }
        }

        /// <summary>
        /// Получить слова, подходящие по заданным отборам
        /// </summary>
        /// <param name="WordPart"></param>
        /// <returns></returns>
        public string GetWords(string WordPart)
        {
            try
            {
                string Responce = "";
                SelectWords.Parameters.AddWithValue("@WordPart", $"{WordPart}%");
                SelectWords.Parameters.AddWithValue("@WordsCount", Settings.Default.MaxWordsGet);
                SQLiteDataReader Result = SelectWords.ExecuteReader();
                while (Result.Read())
                {
                    Responce += $"- {Result.GetString(0)}\r\n";
                }
                Result.Close();
                return Responce;
            }
            catch(Exception e)
            {
                Program.ConsoleWrite($"Ошибка: {e.Message}", 3);
                return null;
            }
        }

        /// <summary>
        /// Отчистка словаря в базе данных
        /// </summary>
        public void ClearDictionary()
        {
            try
            {
                ClearTable.ExecuteNonQuery();
                Program.ConsoleWrite("Процесс отчистки словаря был завершен!", 1);
            }
            catch(Exception e)
            {
                Program.ConsoleWrite($"Ошибка: {e.Message}", 3);
            }
        }
    }
}
