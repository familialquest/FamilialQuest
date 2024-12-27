using System;
using Microsoft.AspNetCore.Mvc;
using EventService.Services;
using CommonLib;

namespace EventService.Controllers
{
    [ApiController]
    public class Controller : ControllerBase
    {
        static NLog.Logger logger = NLog.Web.NLogBuilder.ConfigureNLog("nlog.config").GetCurrentClassLogger();

        private readonly IEventServices _services;

        public Controller(IEventServices services)
        {
            _services = services;
        }

        [HttpPost]
        [Route("CreateHistoryEvent")]
        public ActionResult<FQResponseInfo> CreateHistoryEventRequest(FQRequestInfo ri)
        {
            try
            {
                logger.Trace("CreateHistoryEventRequest started.");

                _services.CreateHistoryEvent(ri);
                
                return Ok();                
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
            finally
            {
                logger.Trace("CreateHistoryEventRequest leave.");
            }
        }

        [HttpPost]
        [Route("GetHistoryEvents")]
        public ActionResult<FQResponseInfo> GetHistoryEventsController(FQRequestInfo ri)
        {
            try
            {
                logger.Trace("GetHistoryEventController started.");

                var selectedEvents = _services.GetHistoryEvents(ri);

                FQResponseInfo response = new FQResponseInfo(selectedEvents);

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
            finally
            {
                logger.Trace("GetHistoryEventController leave.");
            }
        }
    }
}