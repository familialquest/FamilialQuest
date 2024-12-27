using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;
using GroupService.Services;
using System.Net.Http;
using System.Text;
using System.Net;
using CommonLib;
using GroupService.Models;

namespace GroupService.Controllers
{
    [ApiController]
    public class Controller : ControllerBase
    {
        static NLog.Logger logger = NLog.Web.NLogBuilder.ConfigureNLog("nlog.config").GetCurrentClassLogger();

        private readonly IGroupServices _services;

        public Controller(IGroupServices services)
        {
            _services = services;
        }

        #region Пользовательские контроллеры  //TODO: зарезервировано     
        //[HttpPost]
        //[Route("UpdateGroup")]
        //public ActionResult<FQResponseInfo> UpdateGroupController(FQRequestInfo ri)
        //{
        //    try
        //    {
        //        logger.Trace("UpdateGroupController started.");

        //        _services.UpdateGroup(ri);

        //        return Ok();
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(500, ex.Message);
        //    }
        //    finally
        //    {
        //        logger.Trace("UpdateGroupController leave.");
        //    }
        //}
        #endregion


        [HttpPost]
        [Route("GetGroup")]
        public ActionResult<FQResponseInfo> GetGroupController(FQRequestInfo ri)
        {
            try
            {
                logger.Trace("GetGroupController started.");

                var selectedGroup = _services.GetGroup(ri);

                FQResponseInfo response = new FQResponseInfo(selectedGroup);

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
            finally
            {
                logger.Trace("GetGroupController leave.");
            }
        }

        [HttpPost]
        [Route("ProcessPurchase")]
        public ActionResult<FQResponseInfo> ProcessPurchaseController(FQRequestInfo ri)
        {
            try
            {
                logger.Trace("ProcessPurchaseController started.");

                _services.ProcessPurchase(ri);

                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
            finally
            {
                logger.Trace("ProcessPurchaseController leave.");
            }
        }

        #region Административные контроллеры
        [HttpPost]
        [Route("CreateGroup")]
        public ActionResult<FQResponseInfo> CreateGroupController(FQRequestInfo ri)
        {
            try
            {
                logger.Trace("CreateGroupController started.");

                Guid createdGroupId = _services.CreateGroup();

                FQResponseInfo response = new FQResponseInfo(createdGroupId);

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
            finally
            {
                logger.Trace("CreateGroupController leave.");
            }
        }

        //TODO: зарезервировано
        //[HttpPost]
        //[Route("RemoveGroup")]
        //public ActionResult<FQResponseInfo> RemoveGroupController(FQRequestInfo ri)
        //{
        //    try
        //    {
        //        logger.Trace("RemoveGroupController started.");

        //        _services.RemoveGroup(ri);

        //        return Ok();
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(500, ex.Message);
        //    }
        //    finally
        //    {
        //        logger.Trace("RemoveGroupController leave.");
        //    }
        //} 
        #endregion
    }
}