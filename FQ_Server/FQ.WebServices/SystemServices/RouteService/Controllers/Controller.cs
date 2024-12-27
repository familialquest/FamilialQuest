using System;
using Microsoft.AspNetCore.Mvc;
using RouteService.Services;
using CommonLib;
using static CommonLib.FQServiceException;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace RouteService.Controllers
{
    [ApiController]
    public class Controller : ControllerBase
    {
        static NLog.Logger logger = NLog.Web.NLogBuilder.ConfigureNLog("nlog.config").GetCurrentClassLogger();

        private readonly IRouteServices _services;

        public Controller(IRouteServices services)
        {
            _services = services;
        }

        ///// <summary>
        ///// Test RTDN controller
        ///// </summary>
        //[HttpPost]
        //[Route("rtdn")]
        //public ActionResult RTDNController(JObject requestDataJson)
        //{
        //    try
        //    {
        //        logger.Trace("RTDNController started.");

        //        logger.Trace("Trying requestData to string.");
        //        var requestDataStr = requestDataJson.ToString();

        //        logger.Trace($"requestDataStr: {requestDataStr}");

        //        logger.Trace("Trying parse json string.");
        //        var data = requestDataJson["message"].Children()["data"].Value<string>();

        //        logger.Trace($"data: {data}");

        //        logger.Trace("Trying decode Base64.");
        //        var decodedData = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(data));

        //        logger.Trace($"decodedData: {decodedData}");

        //        return Ok();
        //    }
        //    catch (Exception ex)
        //    {
        //        logger.Error(ex);

        //        return StatusCode(Microsoft.AspNetCore.Http.StatusCodes.Status500InternalServerError);
        //    }
        //}

        /// <summary>
        /// Контроллер роутера клиентских запросов
        /// </summary>
        /// <param name="ri">Стандартный в рамках сервиса формат входных данных от клиента</param>
        /// <returns>StatusCode + Content</returns>
        [HttpPost]
        [Route("request")]
        public ActionResult<FQResponseInfo> RouteController(FQRequestInfo ri)
        {
            Guid sessionId = Guid.NewGuid();

            try
            {
                logger.Trace("RouteController started.");

                // чтоб лог начиная с первой записи имел корректный FQSessionID
                HttpContext.Request.Headers.Add("fq_sessionid", sessionId.ToString()); 

                FQResponseInfo requestResult = _services.Route(ri);

                requestResult.SessionId = sessionId;
                return Ok(requestResult);
            }
            catch (Exception ex)
            {
                var errorMessage = ex.Message;

                if (errorMessage == string.Empty)
                {
                    errorMessage = ((int)(FQServiceExceptionType.DefaultError)).ToString();
                }

                FQResponseInfo requestResult = new FQResponseInfo(true);
                requestResult.Successfuly = false;
                requestResult.ResponseData = errorMessage;

                requestResult.SessionId = sessionId;
                return Ok(requestResult);
            }
            finally
            {
                logger.Trace("RouteController leave.");
            }
        }
    }
}