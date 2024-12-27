using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CommonLib;
using TaskService.Models;

namespace TaskService.Services
{
    /// <summary>
    /// Интерфейс сервиса работы с задачами
    /// </summary>
    public interface ITaskService
    {
        /// <summary>
        /// Создание задачи
        /// </summary>
        /// <param name="name">Имя/заголовок задачи</param>
        /// <param name="description">Описание задачи</param>
        /// <param name="cost">Стоимость. Может равняться 0.</param>
        /// <param name="penalty">Штраф. Может равняться 0.</param>
        /// <param name="availableUntil">Время жизни задачи</param>
        /// <param name="solutionTime">Время решения задачи. Не может быть меньше AvailableUntil.</param>
        /// <param name="speedBonus">Бонус за преждевременное выполнение. Может равняться 0.</param>
        /// <param name="creator">Владелец/создатель задачи</param>
        /// <param name="executor">Получатель задачи</param>
        /// <returns>ID новой задачи</returns>
        BaseTask CreateTask(
            FQRequestInfo ri,
            string name, 
            string description, 
            int cost, 
            int penalty,
            DateTime availableUntil, 
            TimeSpan solutionTime,
            int speedBonus,
            Guid creator,
            Guid executor
            );

        /// <summary>
        /// Создание новой задачи из параметров другой.
        /// Поля:<list type="bullet">
        /// <item>Status</item>
        /// <item>CreationDate</item>
        /// <item>CompletionDate</item>
        /// <item>ModificationTime</item>
        /// </list>
        /// создаются новые.
        /// </summary>
        /// <param name="newTask"></param>
        /// <returns></returns>
        BaseTask CreateTask(FQRequestInfo ri, BaseTask newTask, bool isStartingItem = false);

        /// <summary>
        /// Обновление информации о задаче.
        /// </summary>
        /// <remarks>
        /// Поля <see cref="BaseTask.Id"/> и <see cref="BaseTask.Creator"/> не изменяются.
        /// Поле <see cref="BaseTask.Status"/> обновляется через функцию <see cref="UpdateTaskStatus(Guid, TaskStatus)"/>.
        /// Поля <see cref="BaseTask.CreationDate"/>, <see cref="BaseTask.CompletionDate"/> не изменяются напрямую.
        /// Поле <see cref="BaseTask.ModificationTime"/> изменяется автоматически.
        /// </remarks>
        /// <param name="taskId">Идентификатор обновляемой задачи</param>
        /// <param name="newParams">Набор новых значений полей задачи</param>        
        /// <returns>Идентификатор измененной задачи</returns>
        void UpdateTask(Guid taskId, Guid groupId, Dictionary<string, object> newParams);

        /// <summary>
        /// Изменение статуса задачи.
        /// </summary>
        /// <remarks>
        /// Статусы могут изменяться только в соответствии с графом в <see cref="TaskStatusRelations.Rels"/>.
        /// </remarks>
        /// <param name="taskId">Идентификатор задачи</param>
        /// <param name="newStatus">Новый статус</param>
        /// <returns>Установленный статус</returns>
        //[Obsolete]
        //BaseTaskStatus UpdateTaskStatus(
        //    List<Guid> taskId,
        //    BaseTaskStatus newStatus
        //    );

        /// <summary>
        /// Изменение статуса задачи.
        /// </summary>
        /// <remarks>
        /// Статусы могут изменяться только в соответствии с графом в <see cref="TaskStatusRelations.Rels"/>.
        /// </remarks>
        /// <param name="taskId">Идентификатор задачи</param>
        /// <param name="newStatus">Новый статус</param>
        /// <param name="currentUser">Пользователь, выполнивший смену статуса</param>
        /// <returns>Установленный статус</returns>
        BaseTaskStatus UpdateTaskStatus(
            List<Guid> taskId,
            BaseTaskStatus newStatus,
            FQRequestInfo ri,
            List<BaseTask> _currentTasks = null,
            bool isStartingItem = false
            );

        void UpdateRelatedTasksStatus(FQRequestInfo ri);

        /// <summary>
        /// Получение задач по списку идентификаторов
        /// </summary>
        /// <param name="taskIdList">Список идентификаторов</param>
        /// <returns>Список задач</returns>
        List<BaseTask> GetTasks(List<Guid> taskIdList);

        /// <summary>
        /// Получение задач по признакам
        /// </summary>
        /// <param name="searchParams">Искомые значения полей, задачи с которыми нужно вернуть</param>
        /// <returns>Список задач</returns>
        List<BaseTask> GetTasks(Dictionary<string, object> searchParams);

        /// <summary>
        /// Получение задач по признакам
        /// </summary>
        /// <param name="searchParams">Искомые значения полей, задачи с которыми нужно вернуть</param>
        /// <returns>Список задач</returns>
        List<BaseTask> GetUserTasks(FQRequestInfo ri, Dictionary<string, object> searchParams);

        /// <summary>
        /// Удаление задач по идентификаторам
        /// </summary>
        /// <param name="taskIdList">Список идентификаторов задач</param>
        void DeleteTasks(FQRequestInfo ri, List<Guid> taskIdList);

        /// <summary>
        /// Удаление задач по признакам.
        /// </summary>
        /// <param name="searchParams">Искомые значения полей, с которыми нужно удалить задачи</param>
        void DeleteTasks(
            FQRequestInfo ri,
            Dictionary<string, object> searchParams
            );

        /// <summary>
        /// Поиск задач, время исполнения которых истекло, и первод в статус "AvailableUntilPassed" или "SolutionTimeOver" со списанием штрафа
        /// </summary>
        void CloseExpiredTasks();
    }
}
