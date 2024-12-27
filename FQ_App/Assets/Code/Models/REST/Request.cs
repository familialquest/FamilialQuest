using System.Collections.Generic;
using Proyecto26;
using UnityEngine.Networking;
using System.Text;
using Assets.Code.Models.REST.CommonTypes.Common;
using System.Security.Cryptography.X509Certificates;
using System;
using UnityEngine;

namespace Code.Models.REST
{
    class RequestHelperEx : RequestHelper
    {
        class NoCheckCertificateHandler : CertificateHandler
        {
            protected override bool ValidateCertificate(byte[] certificateData)
            {
                return true;
                //X509Certificate2 certificate = new X509Certificate2(certificateData);
                //X509Chain chain = new X509Chain();
                //chain.ChainPolicy = new X509ChainPolicy()
                //{
                //    RevocationMode = X509RevocationMode.Online,
                //    VerificationFlags = X509VerificationFlags.IgnoreWrongUsage,
                //    UrlRetrievalTimeout = TimeSpan.FromSeconds(10)
                //};
                //var result = chain.Build(certificate);

                //for (int i = 0; i < chain.ChainStatus.Length; i++)
                //{
                //    if (chain.ChainStatus[i].Status != X509ChainStatusFlags.RevocationStatusUnknown)
                //    {
                //        chain.ChainPolicy.RevocationFlag = X509RevocationFlag.EntireChain;
                //        chain.ChainPolicy.RevocationMode = X509RevocationMode.Online;
                //        chain.ChainPolicy.UrlRetrievalTimeout = new TimeSpan(0, 1, 0);
                //        chain.ChainPolicy.VerificationFlags = X509VerificationFlags.AllFlags;
                //        bool chainIsValid = chain.Build((X509Certificate2)certificate);
                //        if (!chainIsValid)
                //        {
                //            isOk = false;
                //        }
                //    }
                //}
            }
        }


        private static string _requestedServiceUri = $"{DataModel.Instance.ServerProtocol}://{DataModel.Instance.ServerAddress}:{DataModel.Instance.ServerPort}/request";

        /// <summary>
        /// Создание пустого RequestHelper
        /// </summary>
        /// <returns></returns>
        public static RequestHelperEx Create(string serviceUri = null)
        {
            RequestHelperEx request = new RequestHelperEx()
            {
                EnableDebug = true,
                Headers = new Dictionary<string, string>() { { "accept", "text/plain" } },
                ContentType = "application/json-patch+json"
                //CertificateHandler = new NoCheckCertificateHandler()
            };

            if (!string.IsNullOrEmpty(serviceUri))
                request.Uri = serviceUri;

            return request;
        }

        /// <summary>
        /// Создание RequestHelper с телом
        /// </summary>
        /// <param name="body"></param>
        /// <returns></returns>
        public static RequestHelperEx Create(FQRequest fqRequest)
        {
            RequestHelperEx request = Create();

            string bodyString = Newtonsoft.Json.JsonConvert.SerializeObject(fqRequest.ri);
            request.BodyRaw = Encoding.UTF8.GetBytes(bodyString.ToCharArray());
            request.Uri += fqRequest.action;

            return request;
        }

        /// <summary>
        /// TESTovo.
        /// Создание RequestHelper с телом
        /// </summary>
        /// <param name="body"></param>
        /// <returns></returns>
        public static RequestHelperEx Create(FQRequest fqRequest, string serviceUri)
        {
            RequestHelperEx request = Create(serviceUri);

            string bodyString = Newtonsoft.Json.JsonConvert.SerializeObject(fqRequest.ri);
            request.BodyRaw = Encoding.UTF8.GetBytes(bodyString.ToCharArray());
            request.Uri += fqRequest.action;

            return request;
        }

        public static RequestHelperEx Create(FQRequestInfo ri)
        {
            try
            {
                RequestHelperEx request = Create(_requestedServiceUri);

                PrepareRequest(ri);

                string bodyString = Newtonsoft.Json.JsonConvert.SerializeObject(ri);

                //Debug.Log($"Request bodystring: \n{bodyString}");

                request.BodyRaw = Encoding.UTF8.GetBytes(bodyString.ToCharArray());
                request.Timeout = CommonData.requestTimeOut;
                request.EnableDebug = false;

                return request;
            }
            catch (System.Exception ex)
            {
                Debug.LogError(ex);
                throw;
            }
        }


        public static void PrepareRequest(FQRequestInfo fqRequestInfo)
        {
            try
            {
                fqRequestInfo.Credentials.tokenB64 = CredentialHandler.Instance.Credentials.tokenB64;
                fqRequestInfo.Credentials.DeviceId = UnityEngine.SystemInfo.deviceUniqueIdentifier;
            }
            catch (System.Exception ex)
            {
                Debug.LogError(ex);
                throw;
            }
        }
    }

    class FQRequest
    {
        public FQRequestInfo ri;
        public string action = "";

        public FQRequest(string actionName, ActionData requestActionData)
        {
            action = actionName;

            ri = new FQRequestInfo()
            {
                Credentials = CredentialHandler.Instance.Credentials,
                RequestData = requestActionData
            };
        }
    }
}
