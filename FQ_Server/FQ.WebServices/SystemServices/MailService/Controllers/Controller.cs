using System;
using CommonLib;
using Microsoft.AspNetCore.Mvc;
using MailService.Services;

namespace MailService.Controllers
{
    [ApiController]
    public class Controller : ControllerBase
    {
        static NLog.Logger logger = NLog.Web.NLogBuilder.ConfigureNLog("nlog.config").GetCurrentClassLogger();

        private readonly IMailServices _services;

        public Controller(IMailServices services)
        {
            _services = services;
        }

        [HttpPost]
        [Route("SendMessage")]
        public ActionResult<FQResponseInfo> SendMessageController(FQRequestInfo ri)
        {
            try
            {
                logger.Trace("SendMessageController started.");

                _services.SendMessage(ri);

                return Ok();                             
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
            finally
            {
                logger.Trace("SendMessageController leave.");
            }
        }
    }
}