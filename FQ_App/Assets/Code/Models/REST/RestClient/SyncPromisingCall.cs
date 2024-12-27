using System;
using System.Collections.Generic;

using RSG;

namespace Code.Models.REST
{
    public class SyncPromisingCall
    {
        private static Dictionary<string, object> m_prevPromise = new Dictionary<string, object>();

        /// <summary>
        /// Выполнение промайс-функций по очереди в рамках указанной. 
        /// Логика основана на том, чтобы связывать промайсы с друг другом. По факту, запуск функции выполнится после резолва промайса предыдущей.
        /// </summary>
        /// <typeparam name="T1">Тип параметра "обертываемой" функции</typeparam>
        /// <typeparam name="T2">Тип возвращаемого значения функции <see cref="processPromisingFunc"/> и параметра функции <see cref="resultParser"/>, обрабатывающей и возвращающей результат этой функции</typeparam>
        /// <typeparam name="T3">Тип возвращаемого значения этой функции и функции <see cref="resultParser"/></typeparam>
        /// <param name="processPromisingFunc">Функция, которая возвращает IPromise, и которая будет запущена только после завершения предыдущего своего вызова</param>
        /// <param name="resultParser">Функция, которая возвращает IPromise, и которая будет выполняться после завершения функции <see cref="processPromisingFunc"/>, в том числе для формирования нового результата типа IPromise</param>
        /// <param name="parameter">Имя очереди, в рамках которой будет выполняться функция <see cref="processPromisingFunc"/>. Если не задан, очередь будет называться по имени выполняемой функции.</param>
        /// <returns></returns>
        public static IPromise<T3> Execute<T1, T2, T3>(Func<T1, IPromise<T2>> processPromisingFunc, Func<T2, T3> resultParser, T1 parameter, string queueName = null)
        {
            if (string.IsNullOrEmpty(queueName))
                queueName = processPromisingFunc.Method.Name;

            Promise<T3> promise = new Promise<T3>();

            if (true == m_prevPromise.TryGetValue(queueName, out object value))
            {
                // если есть уже выполняемые запросы
                // достаем 
                var prevPromise = (Promise<DataModelOperationResult>)value;
                m_prevPromise.Remove(queueName);
                // привязываем, что после него будет выполнен запрос
                prevPromise.ContinueWith(() =>
                {
                    try
                    {
                        processPromisingFunc(parameter)
                        .Then((res2) =>
                        {
                                // после выполнения парсим ответ в общем виде (без привязки к выполняемой операции)
                                var opRes = resultParser(res2);
                                // резолвим промайс, по которому будет выполнен следующий запрос
                                promise.Resolve(opRes);
                        })
                        .Catch((ex) =>
                        {                            
                            // резолвим промайс, по которому будет выполнен следующий запрос
                            promise.Reject(ex);
                        });
                    }
                    catch (Exception ex)
                    {
                        // резолвим промайс, по которому будет выполнен следующий запрос
                        promise.Reject(ex);
                    }
                    return promise;
                })
                .Done(); // завершаем этот промайс, так как он уже не нужен
                         // сохраняем новый в ту же очередь
                m_prevPromise[queueName] = promise;
            }
            else
            {
                // если этот запрос первый, то он выполняется как обычно
                promise = (Promise<T3>)processPromisingFunc(parameter)
                .Then((res) =>
                {
                // после выполнения парсим ответ в общем виде (без привязки к выполняемой операции)
                var opRes = resultParser(res);
                // возвращаем результат (тем самым резолвим текущий промайс),
                // который и ждут последующие
                return opRes;
                });
                // запоминаем в новую очередь, чтоб в следующий раз привязаться уже к ней
                m_prevPromise.Add(queueName, promise);
            }
            return promise;
        }


        /// <summary>
        /// Выполнение обычных функций по очереди в рамках указанной, но с возвратом промайса.
        /// Логика основана на том, чтобы связывать промайсы с друг другом. По факту, запуск функции выполнится после резолва промайса предыдущей.
        /// </summary>
        /// <typeparam name="T1">Тип параметра "обертываемой" функции</typeparam>
        /// <typeparam name="T2">Тип возвращаемого значения функции <see cref="processPromisingFunc"/> и параметра функции <see cref="resultParser"/>, обрабатывающей и возвращающей результат этой функции</typeparam>
        /// <typeparam name="T3">Тип возвращаемого значения этой функции и функции <see cref="resultParser"/></typeparam>
        /// <param name="processPromisingFunc">Функция, которая будет запущена только после завершения предыдущего своего вызова</param>
        /// <param name="resultParser">Функция, которая возвращает IPromise, и которая будет выполняться после завершения функции <see cref="processPromisingFunc"/>, в том числе для формирования нового результата типа IPromise</param>
        /// <param name="parameter">Имя очереди, в рамках которой будет выполняться функция <see cref="processPromisingFunc"/>. Если не задан, очередь будет называться по имени выполняемой функции.</param>
        /// <returns></returns>
        public static IPromise<T3> Execute<T1, T2, T3>(Func<T1, T2> processPromisingFunc, Func<T2, T3> resultParser, T1 parameter, string queueName = null)
        {
            if (string.IsNullOrEmpty(queueName))
                queueName = processPromisingFunc.Method.Name;

            Promise<T3> promise = new Promise<T3>();

            if (true == m_prevPromise.TryGetValue(queueName, out object value))
            {
                // если есть уже выполняемые запросы
                // достаем 
                var prevPromise = (Promise<DataModelOperationResult>)value;
                m_prevPromise.Remove(queueName);
                // привязываем, что после него будет выполнен запрос
                prevPromise.Then((res) =>
                {
                    var res2 = processPromisingFunc(parameter);
                    // после выполнения парсим ответ в общем виде (без привязки к выполняемой операции)
                    var opRes = resultParser(res2);
                    // резолвим промайс, по которому будет выполнен следующий запрос
                    promise.Resolve(opRes);
                })
                .Done(); // завершаем этот промайс, так как он уже не нужен
                         // сохраняем новый в ту же очередь
                m_prevPromise[queueName] = promise;
            }
            else
            {
                // запоминаем в новую очередь, чтоб в следующий раз привязаться уже к ней
                m_prevPromise.Add(queueName, promise);

                // если этот запрос первый, то он выполняется как обычно
                var res = processPromisingFunc(parameter);
                // после выполнения парсим ответ в общем виде (без привязки к выполняемой операции)
                var opRes = resultParser(res);
                // возвращаем результат (тем самым резолвим текущий промайс),
                // который и ждут последующие
                promise.Resolve(opRes);
            }
            return promise;
        }
    }

}