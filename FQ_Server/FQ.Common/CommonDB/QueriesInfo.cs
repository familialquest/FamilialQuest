using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace CommonDB
{
    public static class QueriesInfo
    {
        private static Dictionary<string, string> queries = null;

        public static Dictionary<string, string> Queries { get => queries; set => queries = value; }

        static QueriesInfo()
        {
            loadData();
            CheckChanges();
        }

        /// <summary>
        /// Считывание запросов из сопутствующего queries.json
        /// </summary>
        private static void loadData()
        {
            string queriesJson = File.ReadAllText("queries.json");
            Queries = JsonConvert.DeserializeObject<Dictionary<string, string>>(queriesJson);

        }

        /// <summary>
        /// Получение шаблона запроса
        /// </summary>
        /// <param name="queryName">Имя запроса</param>
        /// <returns></returns>
        public static string GetQueryTemplate(string queryName)
        {
            try
            {
                //logger.Trace("GetQueryTemplate started.");
                //logger.Trace($"Получение  шаблона запроса: ${queryName}.");
                if (!Queries.TryGetValue(queryName, out string query))
                {
                    throw new Exception("Ошибка: шаблон запроса не найден.");
                }

                //logger.Trace($"Query: ${query}");

                return query;
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                //logger.Trace("GetQueryTemplate leave.");
            }
        }

        /// <summary>
        /// Таск периодически проверяет триггер readagain.1
        /// </summary>
        /// <returns></returns>
        private static async Task CheckChanges()
        {
            while (true)
            {
                try
                {
                    if (File.Exists("readagain.1"))
                    {
                        loadData();
                        File.Delete("readagain.1");
                    }
                }
                catch (Exception ex)
                {
                    //упс
                }

                await Task.Delay(5000);
            }
        }
    }
}
