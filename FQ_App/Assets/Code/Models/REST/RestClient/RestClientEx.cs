using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Proyecto26;
using RSG;

using System.Collections.Concurrent;
using UnityEditor;
using Assets.Code;

namespace Code.Models.REST
{
    public class RestClientEx
    {
        //private static Stack<IPromise<DataModelOperationResult>> m_opResPromises = new Stack<IPromise<DataModelOperationResult>>();
        //private static Stack<object> m_promises = new Stack<object>();
        //public static IPromise<DataModelOperationResult> PostEx(FQRequestInfo fqRequestInfo)
        //{
        //    Promise<DataModelOperationResult> promise = new Promise<DataModelOperationResult>();

        //    if (m_opResPromises.Count != 0)
        //    {
        //        // если есть уже выполняемые запросы
        //        // достаем последний (да там он обычно один)
        //        var prevPromise = m_opResPromises.Pop();
        //        // привязываем, что после него будет выполнен запрос
        //        prevPromise.Then((res) =>
        //        {
        //            // формируем запрос
        //            RequestHelperEx requestHelper = RequestHelperEx.Create(fqRequestInfo);

        //            // выполняем
        //            RestClient.Post(requestHelper)
        //            .Then((res2) =>
        //            {
        //                // после выполнения парсим ответ в общем виде (без привязки к выполняемой операции)
        //                var opRes = DataModelOperationResult.Wrap(res2, new FQResponse(res2));
        //                // резолвим промайс, по которому будет выполнен следующий запрос
        //                promise.Resolve(opRes);
        //            });
        //        })
        //        .Done(); // завершаем этот промайс, так как он уже не нужен
        //    }
        //    else
        //    {
        //        // если этот запрос первый, то он выполняется как обычно
        //        // формируем запрос
        //        RequestHelperEx requestHelper = RequestHelperEx.Create(fqRequestInfo);

        //        // выполняем
        //        promise = (Promise<DataModelOperationResult>)RestClient.Post(requestHelper)
        //            .Then((res) =>
        //            {
        //                // после выполнения парсим ответ в общем виде (без привязки к выполняемой операции)
        //                var opRes = DataModelOperationResult.Wrap(res, new FQResponse(res));
        //                // возвращаем результат (тем самым резолвим текущий промайс),
        //                // который и ждут последующие
        //                return opRes;
        //            });
        //    }
        //    // запоминаем новый промайс, чтоб в следующий раз привязаться уже к нему
        //    m_opResPromises.Push(promise);
        //    return promise;
        //}


        public static IPromise<ResponseHelper> Post(FQRequestInfo fqRequestInfo)
        {
            // формируем запрос
            RequestHelperEx requestHelper = RequestHelperEx.Create(fqRequestInfo);

            // выполняем
            return RestClient.Post(requestHelper);
        }

        public static DataModelOperationResult ResponseParse(ResponseHelper res)
        {
            return DataModelOperationResult.Wrap(res, new FQResponse(res), true);
        }

        public static IPromise<DataModelOperationResult> PostEx(FQRequestInfo fqRequestInfo)
        {
            fqRequestInfo.ClientVersion = FQBuildSettings.clientVersion;

            return SyncPromisingCall.Execute<FQRequestInfo, ResponseHelper, DataModelOperationResult>(Post, ResponseParse, fqRequestInfo);
        }
    }
}
