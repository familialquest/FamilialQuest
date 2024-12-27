using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;
using AccountService.Services;
using System.Net.Http;
using System.Text;
using System.Net;
using CommonLib;
using AccountService.Models;
using CommonTypes;
using static CommonLib.FQServiceException;

namespace AccountService.Controllers
{
    [ApiController]
    public class Controller : ControllerBase
    {
        static NLog.Logger logger = NLog.Web.NLogBuilder.ConfigureNLog("nlog.config").GetCurrentClassLogger();

        private readonly IAccountServices _services;

        public Controller(IAccountServices services)
        {
            _services = services;
        }

        #region Пользовательские контроллеры
        [HttpPost]
        [Route("Auth")]
        public ActionResult<FQResponseInfo> AuthenticationControl(FQRequestInfo ri)
        {
            try
            {
                logger.Trace("AuthenticationControl started.");

                FQResponseInfo response = _services.Auth(ri);

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
            finally
            {
                logger.Trace("AuthenticationControl leave.");
            }
        }

        [HttpPost]
        [Route("Logout")]
        public ActionResult<FQResponseInfo> LogoutControl(FQRequestInfo ri)
        {
            try
            {
                logger.Trace("LogoutControl started.");

                _services.Logout(ri);

                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
            finally
            {
                logger.Trace("LogoutControl leave.");
            }
        }

        [HttpPost]
        [Route("CreateTempAccount")]
        public ActionResult<FQResponseInfo> CreateTempAccountController(FQRequestInfo ri)
        {
            try
            {
                logger.Trace("CreateTempAccountController started.");

                _services.CreateTempAccount(ri);

                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
            finally
            {
                logger.Trace("CreateTempAccountController leave.");
            }
        }

        [HttpPost]
        [Route("ConfirmTempAccount")]
        public ActionResult<FQResponseInfo> ConfirmTempAccountController(FQRequestInfo ri)
        {
            try
            {
                logger.Trace("ConfirmTempAccountController started.");

                _services.ConfirmTempAccount(ri);

                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
            finally
            {
                logger.Trace("ConfirmTempAccountController leave.");
            }
        }

        [HttpPost]
        [Route("ResetPassword")]
        public ActionResult<FQResponseInfo> ResetPasswordController(FQRequestInfo ri)
        {
            try
            {
                logger.Trace("ResetPasswordController started.");

                _services.ResetPasswordCreateTempAccount(ri);

                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
            finally
            {
                logger.Trace("ResetPasswordController leave.");
            }
        }

        [HttpPost]
        [Route("ConfirmResetPassword")]
        public ActionResult<FQResponseInfo> ConfirmResetPasswordController(FQRequestInfo ri)
        {
            try
            {
                logger.Trace("ConfirmResetPasswordController started.");

                _services.ResetPasswordConfirmTempAccount(ri);

                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
            finally
            {
                logger.Trace("ConfirmResetPasswordController leave.");
            }
        }

        [HttpPost]
        [Route("AddUserToGroup")]
        public ActionResult<FQResponseInfo> AddUserToGroupController(FQRequestInfo ri)
        {
            try
            {
                logger.Trace("AddUserToGroupController started.");

                var createdUser = _services.AddNewUserToCurrentGroup(ri);

                FQResponseInfo response = new FQResponseInfo(createdUser);

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
            finally
            {
                logger.Trace("AddUserToGroupController leave.");
            }
        }

        [HttpPost]
        [Route("GetFQTag")]
        public ActionResult<FQResponseInfo> GetFQTagController(FQRequestInfo ri)
        {
            try
            {
                logger.Trace("GetFQTagController started.");

                var newFQTag = _services.GetFQTag(ri);

                FQResponseInfo response = new FQResponseInfo(newFQTag);

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
            finally
            {
                logger.Trace("GetFQTagController leave.");
            }
        }

        [HttpPost]
        [Route("RemoveUser")]
        public ActionResult<FQResponseInfo> RemoveUserController(FQRequestInfo ri)
        {
            try
            {
                logger.Trace("RemoveUserController started.");

                _services.RemoveUserAndAccount(ri);

                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
            finally
            {
                logger.Trace("RemoveUserController leave.");
            }
        }

        [HttpPost]
        [Route("UpdateUser")]
        public ActionResult<FQResponseInfo> UpdateUserController(FQRequestInfo ri)
        {
            try
            {
                logger.Trace("UpdateUserController started.");

                _services.UpdateUser(ri);

                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
            finally
            {
                logger.Trace("UpdateUserController leave.");
            }
        }

        [HttpPost]
        [Route("GetAllUsers")]
        public ActionResult<FQResponseInfo> GetAllUsersController(FQRequestInfo ri)
        {
            try
            {
                logger.Trace("GetAllUsersController started.");

                var allUsers = _services.GetAllUsers(ri);

                FQResponseInfo response = new FQResponseInfo(allUsers);

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
            finally
            {
                logger.Trace("GetAllUsersController leave.");
            }
        }

        [HttpPost]
        [Route("GetUsersById")]
        public ActionResult<FQResponseInfo> GetUsersByIdController(FQRequestInfo ri)
        {
            try
            {
                logger.Trace("GetUsersByIdController started.");

                List<Guid> inputUsers = new List<Guid>();

                try
                {
                    inputUsers = JsonConvert.DeserializeObject<List<Guid>>(ri.RequestData.postData.ToString());
                }
                catch (Exception ex)
                {
                    logger.Error(ex);
                    throw new Exception(FQServiceExceptionType.DefaultError.ToString());
                }

                var selectedUsers = _services.GetUsersById(ri, inputUsers);

                FQResponseInfo response = new FQResponseInfo(selectedUsers);

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
            finally
            {
                logger.Trace("GetUsersByIdController leave.");
            }
        }

        [HttpPost]
        [Route("ChangeSelfPassword")]
        public ActionResult<FQResponseInfo> ChangeSelfPasswordController(FQRequestInfo ri)
        {
            try
            {
                logger.Trace("ChangeSelfPasswordController started.");

                Account inputAccount = _services.GetAccountFromPostData(ri.RequestData.postData);

                _services.ChangeSelfPassword(ri, inputAccount);

                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
            finally
            {
                logger.Trace("ChangeSelfPasswordController leave.");
            }
        }

        [HttpPost]
        [Route("ChangeGroupUserPassword")]
        public ActionResult<FQResponseInfo> ChangeGroupUserPasswordController(FQRequestInfo ri)
        {
            try
            {
                logger.Trace("ChangeGroupUserPasswordController started.");

                Account inputAccount = _services.GetAccountFromPostData(ri.RequestData.postData);

                _services.ChangeGroupUserPassword(ri, inputAccount);

                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
            finally
            {
                logger.Trace("ChangeGroupUserPasswordController leave.");
            }
        }
        
        #endregion

        #region Административные контроллеры
        [HttpPost]
        [Route("WriteOffCost")]
        public ActionResult<FQResponseInfo> WriteOffCostController(FQRequestInfo ri)
        {
            try
            {
                logger.Trace("WriteOffCostController started.");

                _services.WriteOffCost(ri);

                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
            finally
            {
                logger.Trace("WriteOffCostController leave.");
            }
        }

        [HttpPost]
        [Route("MakePayment")]
        public ActionResult<FQResponseInfo> MakePaymentController(FQRequestInfo ri)
        {
            try
            {
                logger.Trace("MakePaymentController started.");

                _services.MakePayment(ri);

                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
            finally
            {
                logger.Trace("MakePaymentController leave.");
            }
        } 
        #endregion
    }
}