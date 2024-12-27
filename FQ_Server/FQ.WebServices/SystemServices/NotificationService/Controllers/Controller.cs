using System;
using Microsoft.AspNetCore.Mvc;
using CommonLib;
using NotificationService.Services;

namespace NotificationService.Controllers
{
    [ApiController]
    public class Controller : ControllerBase
    {
        static NLog.Logger logger = NLog.Web.NLogBuilder.ConfigureNLog("nlog.config").GetCurrentClassLogger();

        private readonly INotificationServices _services;

        public Controller(INotificationServices services)
        {
            _services = services;
        }

        [HttpPost]
        [Route("RegisterDevice")]
        public ActionResult<FQResponseInfo> RegisterDevice(FQRequestInfo ri)
        {
            try
            {
                logger.Trace("RegisterDevice started.");
                
                bool isRegisteredNow = _services.RegisterDevice(ri);

                FQResponseInfo response = new FQResponseInfo((object)isRegisteredNow);

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
            finally
            {
                logger.Trace("RegisterDevice leave.");
            }
        }

        [HttpPost]
        [Route("UnregisterDevice")]
        public ActionResult<FQResponseInfo> UnregisterDevice(FQRequestInfo ri)
        {
            try
            {
                logger.Trace("UnregisterDevice started.");

                bool isRegisteredNow = _services.UnregisterDevice(ri);

                FQResponseInfo response = new FQResponseInfo((object)isRegisteredNow);

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
            finally
            {
                logger.Trace("UnregisterDevice leave.");
            }
        }

        [HttpPost]
        [Route("UnregisterDeviceInner")]
        public ActionResult<FQResponseInfo> UnregisterDeviceInner(FQRequestInfo ri)
        {
            try
            {
                logger.Trace("UnregisterDeviceInner started.");

                bool isRegisteredNow = _services.UnregisterDeviceInner(ri);

                FQResponseInfo response = new FQResponseInfo((object)isRegisteredNow);

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
            finally
            {
                logger.Trace("UnregisterDeviceInner leave.");
            }
        }

        [HttpPost]
        [Route("SetSubscriptionForUser")]
        public ActionResult<FQResponseInfo> SetSubscriptionForUser(FQRequestInfo ri)
        {
            try
            {
                logger.Trace("SetSubscriptionForUser started.");

                bool isSubsribedNow = _services.SetSubscriptionForUser(ri);

                FQResponseInfo response = new FQResponseInfo((object)isSubsribedNow);

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
            finally
            {
                logger.Trace("SetSubscriptionForUser leave.");
            }
        }

        [HttpPost]
        [Route("NotifyUsers")]
        public ActionResult<FQResponseInfo> NotifyUsers(FQRequestInfo ri)
        {
            try
            {
                logger.Trace("NotifyUsers started.");

                _services.NotifyUsers(ri);

                FQResponseInfo response = new FQResponseInfo((object)true);

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
            finally
            {
                logger.Trace("NotifyUsers leave.");
            }
        }
    }
}