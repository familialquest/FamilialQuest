using System;
using System.Collections.Generic;
using System.Text;

namespace CommonDB
{
    public static class DBHelper
    {
        /// <summary>
        /// Формирование части строки запроса, включающей в себя параметры.
        /// "имя_параметра=@(имя_параметра) ..."
        /// </summary>
        /// <param name="searchParams">Перечисление параметров для включения в запрос</param>
        /// <param name="concatString">Разделитель параметров (например, запятая, оператор AND)</param>
        /// <param name="closed">Вставка символа ";" в конец строки</param>
        /// <returns>Строка вида "имя_параметра1=@(имя_параметра1), имя_параметра2=@(имя_параметра2) ...;", где "," - разделитель"</returns>
        public static string MakeSQLParamString(Dictionary<string, object> searchParams, string concatString, bool closed = true)
        {
            string queryParams = "";
            foreach (var param in searchParams)
            {
                queryParams += $"{param.Key}=@{param.Key}{concatString}";
            }
            queryParams = queryParams.Remove(queryParams.Length - concatString.Length); // удаляется длина concatString

            // закрываем 
            if (closed)
                queryParams += ";";

            return queryParams;
        }

        public static string MakeSQLMultipleParamString(string paramName, int count, string concatString, bool closed = true)
        {
            string queryParams = "";

            for (int i = 0; i < count; i++)
            {
                queryParams += $"{paramName}=@{paramName}{i}{concatString}";
            }
            
            queryParams = queryParams.Remove(queryParams.Length - concatString.Length); // удаляется длина concatString

            // закрываем 
            if (closed)
                queryParams += ";";

            return queryParams;
        }
    }
}
