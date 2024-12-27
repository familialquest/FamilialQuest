using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Code.Models.REST
{
    /// <summary>
    /// Клиентский запрос к сервису
    /// </summary>
    public class ActionData
    {
        /// <summary>
        /// Имя запрашиваемого обработчика
        /// </summary>
        public string actionName;

        /// <summary>
        /// Входные данные от клиента
        /// </summary>
        public object postData;

        public ActionData()
        {

        }

        public ActionData(bool empty)
        {
            if (empty)
            {
                actionName = string.Empty;
                postData = string.Empty;
            }
            else
            {
                actionName = null;
                postData = null;
            }
        }
    }
}
