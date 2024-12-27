using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;
using RewardService.Services;
using System.Net.Http;
using System.Text;
using System.Net;
using CommonLib;
using RewardService.Models;
using CommonTypes;
using static CommonLib.FQServiceException;

namespace RewardService.Controllers
{
    [ApiController]
    public class Controller : ControllerBase
    {
        static NLog.Logger logger = NLog.Web.NLogBuilder.ConfigureNLog("nlog.config").GetCurrentClassLogger();

        private readonly IRewardServices _services;

        public Controller(IRewardServices services)
        {
            _services = services;
        }


        #region Пользовательские контроллеры

        [HttpPost]
        [Route("AddReward")]
        public ActionResult<FQResponseInfo> AddRewardController(FQRequestInfo ri)
        {
            try
            {
                logger.Trace("AddRewardController started.");

                Dictionary<string, object> inputData = new Dictionary<string, object>();

                Reward inputReward = new Reward(true);
                List<Guid> availableFor = new List<Guid>();                

                try
                {
                    inputData = JsonConvert.DeserializeObject<Dictionary<string, object>>(ri.RequestData.postData.ToString());

                    if (!inputData.ContainsKey("reward") ||
                        !inputData.ContainsKey("availableFor"))
                    {
                        throw new Exception("Ошибка: отсутствует входной параметр reward или availableFor");
                    }

                    inputReward = _services.GetRewardFromPostData(inputData["reward"]);
                    availableFor = JsonConvert.DeserializeObject<List<Guid>>(inputData["availableFor"].ToString());                    
                }
                catch (Exception ex)
                {
                    logger.Error(ex);
                    throw new Exception(FQServiceExceptionType.DefaultError.ToString());
                }                
                
                var createdRewardId = _services.AddReward(ri, inputReward, availableFor);

                FQResponseInfo response = new FQResponseInfo(createdRewardId);

                return Ok(response);
                //return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
            finally
            {
                logger.Trace("AddRewardController leave.");
            }
        }

        [HttpPost]
        [Route("CreateUserStartingReward")]
        public ActionResult<FQResponseInfo> CreateUserStartingRewardController(FQRequestInfo ri)
        {
            try
            {
                logger.Trace("CreateUserStartingRewardController started.");               

                Guid destinationUserId = Guid.Empty;

                try
                {
                    destinationUserId = Guid.Parse(ri.RequestData.postData.ToString());

                    if (destinationUserId == Guid.Empty)
                    {
                        throw new Exception("Guid is Empty.");
                    }
                }
                catch (Exception ex)
                {
                    logger.Error(ex);
                    throw new Exception(FQServiceExceptionType.DefaultError.ToString());
                }

                Reward inputReward = new Reward(true);
                inputReward.Title = "Время на игры";
                inputReward.Description = "15 минут на мобильные\\компьютерные\\видео игры.";
                inputReward.Cost = 5;

                List<Guid> availableFor = new List<Guid>();
                availableFor.Add(destinationUserId);

                var createdRewardId = _services.AddReward(ri, inputReward, availableFor, true);

                FQResponseInfo response = new FQResponseInfo(createdRewardId);

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
            finally
            {
                logger.Trace("CreateUserStartingRewardController leave.");
            }
        }

        [HttpPost]
        [Route("PurchaseReward")]
        public ActionResult<FQResponseInfo> PurchaseRewardController(FQRequestInfo ri)
        {
            try
            {
                logger.Trace("PurchaseRewardController started.");

                _services.PurchaseReward(ri);

                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
            finally
            {
                logger.Trace("PurchaseRewardController leave.");
            }
        }

        [HttpPost]
        [Route("GiveReward")]
        public ActionResult<FQResponseInfo> GiveRewardController(FQRequestInfo ri)
        {
            try
            {
                logger.Trace("GiveRewardController started.");

                _services.GiveReward(ri);

                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
            finally
            {
                logger.Trace("GiveRewardController leave.");
            }
        }

        [HttpPost]
        [Route("RemoveReward")]
        public ActionResult<FQResponseInfo> RemoveRewardController(FQRequestInfo ri)
        {
            try
            {
                logger.Trace("RemoveRewardController started.");

                Reward inputReward = _services.GetRewardFromPostData(ri.RequestData.postData);

                _services.RemoveReward(ri, inputReward);

                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
            finally
            {
                logger.Trace("RemoveRewardController leave.");
            }
        }
        
        [HttpPost]
        [Route("GetAllRewards")]
        public ActionResult<FQResponseInfo> GetAllRewardsController(FQRequestInfo ri)
        {
            try
            {
                logger.Trace("GetAllRewardsController started.");

                var allRewards = _services.GetAllRewards(ri);

                FQResponseInfo response = new FQResponseInfo(allRewards);

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
            finally
            {
                logger.Trace("GetAllRewardsController leave.");
            }
        }

        [HttpPost]
        [Route("GetRewardsById")]
        public ActionResult<FQResponseInfo> GetRewardsByIdController(FQRequestInfo ri)
        {
            try
            {
                logger.Trace("GetRewardsByIdController started.");

                List<Guid> inputRewards = new List<Guid>();

                try
                {
                    inputRewards = JsonConvert.DeserializeObject<List<Guid>>(ri.RequestData.postData.ToString());
                }
                catch (Exception ex)
                {
                    logger.Error(ex);
                    throw new Exception(FQServiceExceptionType.DefaultError.ToString());
                }                

                var selectedRewards = _services.GetRewardsById(ri, inputRewards);

                FQResponseInfo response = new FQResponseInfo(selectedRewards);

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
            finally
            {
                logger.Trace("GetRewardsByIdController leave.");
            }
        }
        #endregion


        #region Административные контроллеры

        [HttpPost]
        [Route("RemoveRelatedRewards")]
        public ActionResult<FQResponseInfo> RemoveRelatedRewardsController(FQRequestInfo ri)
        {
            try
            {
                logger.Trace("RemoveRelatedRewardsController started.");

                _services.RemoveRelatedRewards(ri);

                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
            finally
            {
                logger.Trace("RemoveRelatedRewardsController leave.");
            }
        }

        #endregion
    }
}