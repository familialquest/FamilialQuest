using CommonLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RouteService.Services
{
    /// <summary>
    /// Интерфейс сервиса
    /// </summary>
    public interface IRouteServices
    {
        /// <summary>
        /// Первичная проверка корректности запроса,
        /// аутентификация пользователя и дальнейший роутинг к соответствующему,
        /// или роутинг неавторизированного пользователя по вопросам регистрации.
        /// </summary>
        /// <param name="ri"></param>
        /// <returns>Результат обработки запроса</returns>
        FQResponseInfo Route(FQRequestInfo ri);
    }
}
