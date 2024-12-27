using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Assets.Code.Controllers;
using Code.Controllers.MessageBox;
using Code.Models.REST;
using Code.Models.REST.Administrative;
using Code.Models.REST.Users;
using Code.Models.RoleModel;
using Newtonsoft.Json;
using Proyecto26;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Code.Models
{
    public class CredentialsModel
    {
        public enum OnlineStatusFilter
        {
            All = 0,
            Online,
            NotOnline
        }

        public event EventHandler<EventArgs> OnListChanged;

        public virtual void ListChanged(EventArgs e)
        {
            EventHandler<EventArgs> handler = OnListChanged;
            handler?.Invoke(this, e);
        }

        /// <summary>
        /// Функция получает список наград списком диктов. Нужно для отображения в списке. 
        /// Списки мои заточены под списки диктов.
        /// </summary>
        /// <param name="user">Ползователь</param>
        /// <returns></returns>
        public static List<Dictionary<string, object>> ToListOfDictionary<T>(List<T> objectList)
        {
            if (objectList == null)
            {
                return new List<Dictionary<string, object>>();
            }

            List<Dictionary<string, object>> list = new List<Dictionary<string, object>>();
            foreach (var item in objectList)
            {
                list.Add(item.GetType()
                .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .ToDictionary(prop => prop.Name, prop => prop.GetValue(item, null)));
            }
            return list;
        }

        public CredentialsModel() { }

        private List<User> m_Users = null;
        public List<User> Users { get => m_Users; set => m_Users = value; }

        public List<User> ParentUsers
        {
            get
            {
                if (Users != null)
                {
                    return Users.FindAll((item) => (item.Role == RoleTypes.Administrator));
                }
                else
                {
                    return new List<User>();
                }
            }
        }
        public List<User> ChildrenUsers
        {
            get
            {
                if (Users != null)
                {
                    return Users.FindAll((item) => (item.Role == RoleTypes.User));
                }
                else
                {
                    return new List<User>();
                }
            }
        }

        public RSG.IPromise<DataModelOperationResult> GetAllUsers()
        {
            GetAllUsersRequest req = new GetAllUsersRequest();

            var prom = RestClientEx.PostEx(req.request)
                .Then((res) => DataModelOperationResult.Wrap(res.RawResponse, new GetAllUsersResponse(res.RawResponse)))
                .Catch((ex) => DataModelOperationResult.Wrap(ex));

            return prom;                
        }
        
        public RSG.IPromise<DataModelOperationResult> UpdateAllCredentials(bool updateTasksPageUI = true, bool updateRewardsPageUI = true, bool updateHistorysPageUI = true)
        {
            var prom = GetAllUsers()
                .Then((res) =>
                {
                    if (res.result)
                    {
                        Debug.Log(((GetAllUsersResponse)res.ParsedResponse).Users?.Count);

                        Users = ((GetAllUsersResponse)res.ParsedResponse).Users;

                        //Обновление инфы о текущем пользователе
                        var updatedCurrentUser = Users.Where(x => x.Id == CredentialHandler.Instance.Credentials.userId).FirstOrDefault();

                        if (updatedCurrentUser != null)
                        {
                            CredentialHandler.Instance.CurrentUser = updatedCurrentUser;
                        }

                        //Принудительно обновим UI страницы наград
                        //т.к. могло измениться финансовое состояние некоторых пользователей - 
                        //и нужно актуализировать инфу о доступности наград и количество доступных монет (для ребенка)

                        if (updateTasksPageUI)
                        {
                            DataModel.Instance.Tasks.UpdatePageUI();
                        }

                        if (updateRewardsPageUI)
                        {
                            DataModel.Instance.Rewards.UpdatePageUI();
                        }                        

                        if (updateHistorysPageUI)
                        {
                            DataModel.Instance.HistoryEvents.UpdatePageUI();
                        }

                        //TODO: чтобы не прокидывать эвент
                        ViewControllers.SettingsPageController.UpdateUserDetails();

                        ListChanged(EventArgs.Empty);
                    }                    

                    return res;
                });

            return prom;
        }        

        public RSG.IPromise<DataModelOperationResult> AddUser(string newUserLogin, string newUserPassword, User newUserInfo)
        {
            string newUserPasswordHash = AuthModel.GetPasswordHash(newUserLogin, newUserPassword);

            AddUserRequest req = new AddUserRequest(newUserLogin, newUserPasswordHash, newUserInfo);

            var prom = RestClientEx.PostEx(req.request)
               .Then((res) => DataModelOperationResult.Wrap(res.RawResponse, new AddUserResponse(res.RawResponse)))
               .Catch((ex) => DataModelOperationResult.Wrap(ex));

            return prom;
        }

        public RSG.IPromise<DataModelOperationResult> GetFQTag(string newUserLogin)
        {
            GetFQTagRequest req = new GetFQTagRequest(newUserLogin);

            var prom = RestClientEx.PostEx(req.request)
                .Then((res) => DataModelOperationResult.Wrap(res.RawResponse, new GetFQTagResponse(res.RawResponse)))
                .Catch((ex) => DataModelOperationResult.Wrap(ex));

            return prom;
        }

        public RSG.IPromise<DataModelOperationResult> UpdateUser(User newUserInfo)
        {
            UpdateUserRequest req = new UpdateUserRequest(newUserInfo);

            var prom = RestClientEx.PostEx(req.request)
               .Then((res) => DataModelOperationResult.Wrap(res.RawResponse, new AddUserResponse(res.RawResponse)))
               .Catch((ex) => DataModelOperationResult.Wrap(ex));

            return prom;
        }


        public RSG.IPromise<DataModelOperationResult> RemoveUser(Guid userId)
        {
            RemoveUserRequest req = new RemoveUserRequest(userId);

            var prom = RestClientEx.PostEx(req.request)
               .Then((res) => DataModelOperationResult.Wrap(res.RawResponse, new RemoveUserResponse(res.RawResponse)))
               .Catch((ex) => DataModelOperationResult.Wrap(ex));

            return prom;
        }

        public RSG.IPromise<DataModelOperationResult> ChangeSelfPassword(string passwordCurrent, string passwordNew)
        {
            string login = CredentialHandler.Instance.Credentials.Login;

            string passwordHash_Current = AuthModel.GetPasswordHash(login, passwordCurrent);
            string passwordHash_New = AuthModel.GetPasswordHash(login, passwordNew);

            ChangeSelfPasswordRequest req = new ChangeSelfPasswordRequest(passwordHash_Current, passwordHash_New);

            var prom = RestClientEx.PostEx(req.request)
               .Then((res) => DataModelOperationResult.Wrap(res.RawResponse, new ChangeSelfPasswordResponse(res.RawResponse)))
               .Catch((ex) => DataModelOperationResult.Wrap(ex));

            return prom;
        }

        public RSG.IPromise<DataModelOperationResult> ChangeGroupUserPassword(Guid userId, string passwordCurrent, string passwordNew)
        {
            var destUser = DataModel.Instance.Credentials.Users.Where(x => x.Id == userId).FirstOrDefault();

            string login_destUser = destUser.Login;
            string login_currentUser = CredentialHandler.Instance.Credentials.Login;

            string passwordHash_Current = AuthModel.GetPasswordHash(login_currentUser, passwordCurrent);
            string passwordHash_New = AuthModel.GetPasswordHash(login_destUser, passwordNew);

            ChangeGroupUserPasswordRequest req = new ChangeGroupUserPasswordRequest(userId, passwordHash_Current, passwordHash_New);

            var prom = RestClientEx.PostEx(req.request)
               .Then((res) => DataModelOperationResult.Wrap(res.RawResponse, new ChangeGroupUserPasswordResponse(res.RawResponse)))
               .Catch((ex) => DataModelOperationResult.Wrap(ex));

            return prom;
        }
    }  
}
