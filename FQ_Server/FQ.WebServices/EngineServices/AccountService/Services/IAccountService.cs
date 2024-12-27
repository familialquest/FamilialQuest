using CommonLib;
using AccountService.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CommonTypes;

namespace AccountService.Services
{
    /// <summary>
    /// Интерфейс сервиса
    /// </summary>
    public interface IAccountServices
    {
        Account GetAccountFromPostData(object inputParams);
                
        User GetUserFromPostData(object inputParams);
        
        void CreateTempAccount(FQRequestInfo ri);
        
        void ConfirmTempAccount(FQRequestInfo ri);
        
        void ResetPasswordCreateTempAccount(FQRequestInfo ri);
        
        void ResetPasswordConfirmTempAccount(FQRequestInfo ri);
       
        FQResponseInfo Auth(FQRequestInfo ri);
        
        void Logout(FQRequestInfo ri);
       
        void ChangeSelfPassword(FQRequestInfo ri, Account inputAccount);
        
        void ChangeGroupUserPassword(FQRequestInfo ri, Account inputAccount);
        
        //TODO: зарезервирован
        void ChangeEmail(FQRequestInfo ri, Account inputAccount);
        
        void UpdateUser(FQRequestInfo ri);
        
        void WriteOffCost(FQRequestInfo ri);
        
        void MakePayment(FQRequestInfo ri);
        
        List<User> GetAllUsers(FQRequestInfo ri);
        
        List<User> GetUsersById(FQRequestInfo ri, List<Guid> selectedUsers);
        
        Guid AddNewUserToCurrentGroup(FQRequestInfo ri);

        string GetFQTag(FQRequestInfo ri);

        void RemoveUserAndAccount(FQRequestInfo ri);
    }
}
