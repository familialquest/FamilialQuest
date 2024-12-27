using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using CommonLib;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace CommonRoutes
{
    public static class RouteInfo
    {
        static NLog.Logger logger = NLog.Web.NLogBuilder.ConfigureNLog("nlog.config").GetCurrentClassLogger();

        //ActionName-ServiceName
        public static Dictionary<string, string> routes = new Dictionary<string, string>();

        //ServiceName-ServiceAddress
        public static Dictionary<string, string> services = new Dictionary<string, string>();

        static RouteInfo()
        {
            loadData();
            _ = CheckChanges();
        }
        
        public static string RouteToService(FQRequestInfo ri, IHttpContextAccessor _httpContextAccessor = null)
        {
            try
            {
                logger.Trace("RouteToService started.");

                //Получение имени сервиса, соответствующего запрашиваему Action-у
                if (routes.TryGetValue(ri.RequestData.actionName, out string serviceName))
                {
                    logger.Trace($"serviceName: {serviceName}.");

                    //Получение адреса сервиса
                    if (services.TryGetValue(serviceName, out string serviceAddress))
                    {
                        logger.Trace($"serviceAddress: {serviceAddress}.");

                        var requestUrl = string.Format("{0}/{1}", serviceAddress, ri.RequestData.actionName);
                        logger.Trace($"requestUrl: {requestUrl}.");

                        FQRequestInfo ri_route = ri.Clone();
                        HttpClient client = new HttpClient();
                        client.Timeout = TimeSpan.FromSeconds(Settings.Current[Settings.Name.Route.RequestTimeout, CommonData.requestTimeOut]);

                        if (_httpContextAccessor != null)
                        {
                            client.DefaultRequestHeaders.Add("fq_sessionid", _httpContextAccessor.HttpContext.Request.Headers["fq_sessionid"].ToString());
                        }

                        var response = client.PostAsJsonAsync(requestUrl, ri_route);
                        response.Wait();

                        if (response.Result.StatusCode == HttpStatusCode.OK || 
                            response.Result.StatusCode == HttpStatusCode.NoContent)
                        {
                            //Всё ок, формирование ответа
                            var responseStrTask = response.Result.Content.ReadAsAsync<FQResponseInfo>();
                            responseStrTask.Wait();

                            var responseData = string.Empty;

                            if (responseStrTask.Result != null)
                            {
                                responseData = responseStrTask.Result.ResponseData;
                            }

                            return responseData;
                        }
                        else
                        {
                            var error = response.Result.Content.ReadAsStringAsync();
                            error.Wait();

                            logger.Error($"response.Result.StatusCode: {response.Result.StatusCode}");
                            throw new FQServiceException(error.Result);
                        }
                    }
                    else
                    {
                        throw new Exception("Неизвестный ServiceName.");
                    }
                }
                else
                {
                    throw new Exception("Неизвестный action.");
                }
            }
            finally
            {
                logger.Trace("RouteToService leave.");
            }
        }


        /// <summary>
        /// Считывание списка роутов из сопутствующего routes.json
        /// </summary>
        private static void loadData()
        {
            try
            {
                string routesJson = File.ReadAllText("routes.json");
                routes = JsonConvert.DeserializeObject<Dictionary<string, string>>(routesJson);
            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }

            try
            {
                string servicesJson = File.ReadAllText("services.json");
                services = JsonConvert.DeserializeObject<Dictionary<string, string>>(servicesJson);
            }
            catch (Exception ex)
            {
                logger.Error(ex);
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
                    logger.Error(ex);
                }

                await Task.Delay(5000);
            }
        }

        public static async Task<string> RouteToServiceAsync(FQRequestInfo ri, IHttpContextAccessor _httpContextAccessor = null)
        {
            try
            {
                var requestUrl = GetRoutingAddress(ri);
                FQRequestInfo ri_route = ri.Clone(); ;

                HttpClient client = CreateCustomHttpClient(_httpContextAccessor);

                await client.PostAsJsonAsync(requestUrl, ri_route)
                    .ContinueWith((responseResult) =>
                    {
                        return CheckResponse(responseResult);
                    });

                return null;
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                return null;
            }
        }

        private static async Task<string> CheckResponse(Task<HttpResponseMessage> response)
        {
            try
            {
                if (response.Result.StatusCode == HttpStatusCode.OK ||
                        response.Result.StatusCode == HttpStatusCode.NoContent)
                {
                    //Всё ок, формирование ответа
                    return await response.Result.Content.ReadAsAsync<FQResponseInfo>()
                        .ContinueWith((responseResult) =>
                    {
                        var responseData = string.Empty;

                        if (responseResult.Result != null)
                        {
                            responseData = responseResult.Result.ResponseData;
                        }

                        return responseData;
                    });

                }
                else
                {
                    await response.Result.Content.ReadAsStringAsync()
                        .ContinueWith((responseResult) =>
                        {
                            logger.Error($"response.Result.StatusCode: {response.Result.StatusCode}");
                            throw new FQServiceException(responseResult.Result);
                        });

                    return null;
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                throw;
            }
        }

        private static HttpClient CreateCustomHttpClient(IHttpContextAccessor _httpContextAccessor)
        {
            HttpClient client = new HttpClient();
            client.Timeout = TimeSpan.FromSeconds(Settings.Current[Settings.Name.Route.RequestTimeout, CommonData.requestTimeOut]);

            if (_httpContextAccessor != null && _httpContextAccessor.HttpContext.Request.Headers.ContainsKey("fq_sessionid"))
            {
                client.DefaultRequestHeaders.Add("fq_sessionid", _httpContextAccessor.HttpContext.Request.Headers["fq_sessionid"].ToString());
            }

            return client;
        }

        private static string GetRoutingAddress(FQRequestInfo ri)
        {
            if (routes.TryGetValue(ri.RequestData.actionName, out string serviceName))
            {
                logger.Trace($"serviceName: {serviceName}.");

                //Получение адреса сервиса
                if (services.TryGetValue(serviceName, out string serviceAddress))
                {
                    logger.Trace($"serviceAddress: {serviceAddress}.");

                    var requestUrl = string.Format("{0}/{1}", serviceAddress, ri.RequestData.actionName);
                    logger.Trace($"requestUrl: {requestUrl}.");

                    return requestUrl;
                }
                else
                {
                    throw new Exception("Неизвестный ServiceName.");
                }
            }
            else
            {
                throw new Exception("Неизвестный action.");
            }
        }
    }
}
