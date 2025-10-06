using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Windows;



#if nanoCAD
using HostMgd.ApplicationServices;
using HostMgd.EditorInput;
using Teigha.DatabaseServices;
using Exception = Teigha.Runtime.Exception;
#else
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Exception = Autodesk.AutoCAD.Runtime.Exception;
using Autodesk.AutoCAD.EditorInput;

#endif

namespace ElectroTools
{
    public static class BDSQL

    {

        

        static public List<string> searchAllDataInBD(string dbFilePath, string nameTable, string searchColum, string filterColumn = null, string filterValue = null)
        {

            List<string> tempList = new List<string>();
            // Строка подключения к базе данных SQLite
            string connectionString = "Data Source=" + dbFilePath;
            //string connectionString = $"Data Source={dbFilePath};Version=3;";

            // SQL-запрос для выполнения (замените на свой запрос)
            string query;
            if (string.IsNullOrEmpty(filterValue) | string.IsNullOrEmpty(filterColumn))
            {
                query = "SELECT * FROM " + nameTable;
            }
            else
            {
                query = "SELECT * FROM " + nameTable + " WHERE " + filterColumn + " = " + filterValue;
            }


            using (DocumentLock doclock = MyOpenDocument.doc.LockDocument())
            {

                using (SQLiteConnection connection = new SQLiteConnection(connectionString))
                {
                    connection.Open();
                    using (SQLiteCommand command = new SQLiteCommand(query, connection))
                    {



                        using (SQLiteDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {

                                tempList.Add(reader[searchColum].ToString());
                                // Обрабатывайте результаты запроса
                                //ed.WriteMessage(reader[$"{searchColum}"].ToString());
                            }
                        }

                    }
                    connection.Close();
                }
            }

            return tempList;
        }

        static public T searchDataInBD <T> (string dbFilePath, string nameTable, string searchItem, string searchColum, string getColumn)
        {
            T resultValue = default;
            // Строка подключения к базе данных SQLite
            string connectionString = "Data Source=" + dbFilePath;
            //string connectionString = $"Data Source={dbFilePath};Version=3;";

            // SQL-запрос для выполнения (замените на свой запрос)
            string query = "SELECT * FROM " + nameTable + " WHERE " + searchColum + "= @searchItem";
            try
            {
                using (DocumentLock doclock = MyOpenDocument.doc.LockDocument())
                {
                    using (SQLiteConnection connection = new SQLiteConnection(connectionString))
                    {
                        connection.Open();
                        using (SQLiteCommand command = new SQLiteCommand(query, connection))
                        {
                            command.Parameters.AddWithValue("@searchItem", searchItem);

                            using (SQLiteDataReader reader = command.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    var value = reader[getColumn];

                                    if (value != DBNull.Value)
                                    {
                                        resultValue = (T)Convert.ChangeType(value, typeof(T));
                                    }

                                }
                                connection.Close();
                            }

                            return resultValue;
                        }
                    }
                }
            }


            catch (Exception ex)
            {

                MessageBox.Show("В базе данных отсутствует такая позиция, добавьте ее и повторите снова попытку."); ;

                return default;
            }
        }

        static public bool updateDataInDB(string dbFilePath, string nameTable, string searchColumn, string searchValue, string updateColumn, string newValue)
        {
            // Строка подключения к базе данных SQLite
            string connectionString = "Data Source=" + dbFilePath;

            // SQL-запрос для обновления данных
            string query = $"UPDATE {nameTable} SET {updateColumn} = @newValue WHERE {searchColumn} = @searchValue";

            try
            {
                using (SQLiteConnection connection = new SQLiteConnection(connectionString))
                {
                    connection.Open();
                    using (SQLiteCommand command = new SQLiteCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@newValue", newValue);
                        command.Parameters.AddWithValue("@searchValue", searchValue);

                        int rowsAffected = command.ExecuteNonQuery();

                        // Проверяем, были ли затронуты строки
                        if (rowsAffected > 0)
                        {
                            return true; // Обновление прошло успешно
                        }
                        else
                        {
                            MessageBox.Show("Запись не найдена или значение не изменилось.");
                            return false; // Обновление не удалось
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при обновлении базы данных: {ex.Message}");
                return false; // Обновление не удалось
            }
        }


        /// <summary>
        /// Эффективно извлекает все свойства кабелей из базы данных за один запрос.
        /// </summary>
        /// <param name="dbFilePath">Путь к файлу базы данных SQLite.</param>
        /// <returns>Словарь, где ключ - это имя кабеля, а значение - его свойства.</returns>
        public static Dictionary<string, CableProperties> GetAllCableProperties(string dbFilePath)
        {
            var results = new Dictionary<string, CableProperties>();
            string connectionString = $"Data Source={dbFilePath};Version=3;";

            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();

                // SQL-команда теперь намного проще: просто выбрать все из таблицы.
                var command = connection.CreateCommand();
                command.CommandText = "SELECT name, r, x, r0, x0, rN, xN, Ke, Ce, Icrit, mydefault FROM cable";

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var props = new CableProperties
                        {
                            Name = reader["name"].ToString(),
                            R = Convert.ToDouble(reader["r"]),
                            X = Convert.ToDouble(reader["x"]),
                            R0 = Convert.ToDouble(reader["r0"]),
                            X0 = Convert.ToDouble(reader["x0"]),
                            RN = Convert.ToDouble(reader["rN"]),
                            XN = Convert.ToDouble(reader["xN"]),
                            Ke = Convert.ToDouble(reader["Ke"]),
                            Ce = Convert.ToDouble(reader["Ce"]),
                            IsDefault = reader["mydefault"].ToString().Equals("true", StringComparison.OrdinalIgnoreCase),
                            Icrict = Convert.ToDouble(reader["Icrit"])
                        };

                        // Добавляем свойства в словарь, используя имя кабеля в качестве ключа
                        if (!results.ContainsKey(props.Name))
                        {
                            results.Add(props.Name, props);
                        }
                    }
                }
            }
            return results;
        }
         
        public static Dictionary<string, TransformerProperties> GetAllTransformerProperties(string dbFilePath)
        {
            var results = new Dictionary<string, TransformerProperties>();
            string connectionString = $"Data Source={dbFilePath};Version=3;";

            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();

                // SQL-команда теперь намного проще: просто выбрать все из таблицы.
                var command = connection.CreateCommand();
                command.CommandText = "SELECT name, r, x, r0, x0 FROM transformer";

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var props = new TransformerProperties
                        {
                            Name = reader["name"].ToString(),
                            R = Convert.ToDouble(reader["r"]),
                            X = Convert.ToDouble(reader["x"]),
                            R0 = Convert.ToDouble(reader["r0"]),
                            X0 = Convert.ToDouble(reader["x0"]),
                        };

                        // Добавляем свойства в словарь, используя имя кабеля в качестве ключа
                        if (!results.ContainsKey(props.Name))
                        {
                            results.Add(props.Name, props);
                        }
                    }
                }
            }
            return results;
        }



    }
}
