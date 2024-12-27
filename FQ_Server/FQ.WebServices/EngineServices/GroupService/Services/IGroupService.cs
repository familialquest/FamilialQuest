using CommonLib;
using CommonTypes;
using GroupService.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GroupService.Services
{
    /// <summary>
    /// Интерфейс сервиса
    /// </summary>
    public interface IGroupServices
    {
        /// <summary>
        /// Создание группы
        /// </summary>
        /// <returns></returns>
        Guid CreateGroup();
        
        /// <summary>
        /// Обновление группы
        /// </summary>
        /// <param name="ri"></param>
        void UpdateGroup(FQRequestInfo ri);

        /// <summary>
        /// Удаление группы
        /// </summary>
        /// <param name="ri"></param>
        void RemoveGroup(FQRequestInfo ri);

        /// <summary>
        /// Получение информации о группе
        /// </summary>
        /// <param name="ri"></param>
        /// <returns></returns>
        Group GetGroup(FQRequestInfo ri);

        /// <summary>
        /// Покупка расширения
        /// </summary>
        /// <param name="ri"></param>
        void ProcessPurchase(FQRequestInfo ri);
    }
}
