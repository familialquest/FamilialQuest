using Code.Models;
using System;
using TMPro;
using UnityEngine;
using Code.ViewControllers.TList;
using Code.Models.REST;
using UnityEngine.UI;
using static Code.Models.CredentialsModel;
using Code.Models.RoleModel;
using Assets.Code.Models.REST.CommonTypes;
using Code.Controllers.MessageBox;
using System.Globalization;

namespace Code.ViewControllers
{
    public class GroupPageController : MonoBehaviour
    {
        public GameObject thisForm;

        public GameObject CircleProgressBar;
        //public TextMeshProUGUI AmountText;
        public TextMeshProUGUI CoinsText;
        //public GameObject CreateRewardBtnGroup;
        public TListViewController ScrollRect_ParentsGroup;
        public TListViewController ScrollRect_ChildrensGroup;
        public TListViewController ScrollRect_ParentsGroup_Children;
        public TListViewController ScrollRect_ChildrensGroup_Children;

        public BaseGroupListFilter TypeChooser_GroupListFilter;

        public GameObject SelectedIcon_All;

        private TListViewController m_currentList
        {
            get
            {
                switch (TypeChooser_GroupListFilter.CurrentActiveFilter)
                {
                    case RoleTypes.Administrator:
                        return ScrollRect_ParentsGroup;

                    case RoleTypes.User:
                        return ScrollRect_ChildrensGroup;
                };
                return ScrollRect_ChildrensGroup;
            }
        }

        // Start is called before the first frame update
        void Awake()
        {

            try
            {
                // подпишемся на изменение списка
                DataModel.Instance.Credentials.OnListChanged += Group_ListChanged;

                if (CredentialHandler.Instance.CurrentUser.Role == Models.RoleModel.RoleTypes.User)
                {
                    //null - в случае, если это не отобрадение на странице пользователей, а окошко userselector
                    if (ScrollRect_ParentsGroup_Children != null)
                    {
                        ScrollRect_ParentsGroup = ScrollRect_ParentsGroup_Children;
                    }

                    if (ScrollRect_ChildrensGroup_Children != null)
                    {
                        ScrollRect_ChildrensGroup = ScrollRect_ChildrensGroup_Children;
                    }
                }

                // подпишемся на событие требования обновления от листвью
                ScrollRect_ParentsGroup.OnNeedListRefresh += M_listViewController_NeedListRefresh;
                ScrollRect_ChildrensGroup.OnNeedListRefresh += M_listViewController_NeedListRefresh;

                // если лист прежде не был получен, то сымитируем требование обновления от листа
                if (DataModel.Instance.Credentials.Users == null)
                    M_listViewController_NeedListRefresh(null, null);
                else
                {
                    SetNewList();
                }

                TypeChooser_GroupListFilter.OnFilterChanged += TypeChooser_GroupListFilter_OnFilterChanged;

                SetSelectedIcon_All();
            }
            catch (Exception ex)
            {
                Debug.LogError(ex);
                throw;
            }

            thisForm.SetActive(false);
        }

        private void Group_ListChanged(object sender, EventArgs e)
        {
            try
            {
                SetNewList();
                SetSelectedIcon_All();
            }
            catch (Exception ex)
            {
                Debug.LogError(ex);
                throw;
            }
        }

        private void TypeChooser_GroupListFilter_OnFilterChanged(object sender, EventArgs e)
        {
            try
            {
                ScrollRect_ParentsGroup.gameObject.SetActive(TypeChooser_GroupListFilter.CurrentActiveFilter == RoleTypes.Administrator);
                ScrollRect_ChildrensGroup.gameObject.SetActive(TypeChooser_GroupListFilter.CurrentActiveFilter == RoleTypes.User);

                //if (m_currentList.Items?.Count > 0)
                //{
                //    AmountText.text = $"{m_currentList.Items?.Count}";
                //}
                //else
                //{
                //    AmountText.text = "-";
                //}

                if (CredentialHandler.Instance.CurrentUser.Role == RoleTypes.User)
                {
                    //CreateRewardBtnGroup.SetActive(false);
                    //CoinsIcon.SetActive(true);
                    //CoinsText.text = $"{CredentialHandler.Instance.CurrentUser.Coins}";
                }
                else
                {
                    if (CredentialHandler.Instance.CurrentUser.Role == RoleTypes.Administrator)
                    {
                        //CreateRewardBtnGroup.SetActive(true);
                        //CoinsIcon.SetActive(false);
                        //CoinsText.text = "";
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError(ex);
                throw;
            }
        }

        private void M_listViewController_ListChanged(object sender, EventArgs e)
        {
            //SetSelectedAmountText();
        }

        //private void SetSelectedAmountText()
        //{
        //    //m_bottomBarText.text = m_listViewController.GetSelectedItems().Count.ToString();
        //}

        private void OnDestroy()
        {
            try
            {
                // отпишемся от изменение списка
                DataModel.Instance.Credentials.OnListChanged -= Group_ListChanged;
                // отпишемся от события требования обновления от листвью
                ScrollRect_ParentsGroup.OnNeedListRefresh -= M_listViewController_NeedListRefresh;
                ScrollRect_ChildrensGroup.OnNeedListRefresh -= M_listViewController_NeedListRefresh;
                // отпишемся от обновлений фильтра
                TypeChooser_GroupListFilter.OnFilterChanged -= TypeChooser_GroupListFilter_OnFilterChanged;

                OnlineGroupListFilter.ResetFilters();
                OnlineGroupParentsListFilter.ResetFilters();
            }
            catch (Exception ex)
            {
                Debug.LogError(ex);
                throw;
            }
        }

        private void M_listViewController_NeedListRefresh(object sender, System.EventArgs e)
        {
            try
            {
                CircleProgressBar.SetActive(true);
                DataModel.Instance.Credentials.UpdateAllCredentials()
                    .Then((res) =>
                    {
                        CircleProgressBar.SetActive(false);
                    })
                    .Catch((ex) =>
                    {
                        //Если сюда попали, то ошибку уже обработали в DataModel и показали уведомление
                        //Нужно только убрать индикацию загрузки
                        CircleProgressBar.SetActive(false);
                    });
            }
            catch (Exception ex)
            {
                Debug.LogError(ex);

                CircleProgressBar.SetActive(false);

                FQServiceException.ShowExceptionMessage(ex);

                throw;
            }
        }

        private void SetNewList(/*List<Dictionary<string, object>> newList*/)
        {
            try
            {
                ScrollRect_ParentsGroup.SetListItems(CredentialsModel.ToListOfDictionary(DataModel.Instance.Credentials.ParentUsers));
                ScrollRect_ChildrensGroup.SetListItems(CredentialsModel.ToListOfDictionary(DataModel.Instance.Credentials.ChildrenUsers));

                //if (m_currentList.Items?.Count > 0)
                //{
                //    AmountText.text = $"{m_currentList.Items?.Count}";
                //}
                //else
                //{
                //    AmountText.text = "-";
                //}

                ////Сброс отображаемых фильтров
                //OnlineGroupListFilter.ResetFilters();

                //Актуализация отображаемых фильтров
                var childrensFilters = GetComponentsInChildren<OnlineGroupListFilter>(true);
                foreach (var filter in childrensFilters)
                {
                    try
                    {
                        filter.AfterTaskStatusSelected();
                    }
                    catch
                    {

                    }
                }

                //Актуализация отображаемых фильтров
                var parentsFilters = GetComponentsInChildren<OnlineGroupParentsListFilter>(true);
                foreach (var filter in parentsFilters)
                {
                    try
                    {
                        filter.AfterTaskStatusSelected();
                    }
                    catch
                    {

                    }
                }

                if (CredentialHandler.Instance.CurrentUser.Role == Models.RoleModel.RoleTypes.User)
                {
                    if (CoinsText != null)
                    {
                        if (CredentialHandler.Instance.CurrentUser.Coins >= 1000)
                        {
                            CoinsText.text = CredentialHandler.Instance.CurrentUser.Coins.ToString("0,0", CultureInfo.CreateSpecificCulture("el-GR"));
                        }
                        else
                        {
                            CoinsText.text = $"{CredentialHandler.Instance.CurrentUser.Coins}";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError(ex);
                throw;
            }
        }

        private void SetSelectedIcon_All()
        {
            try
            {
                //Отображение 
                if (SelectedIcon_All != null)
                {
                    bool selectedAll = true;

                    foreach (var user in DataModel.Instance.Credentials.ChildrenUsers)
                    {
                        if (!user.Selected)
                        {
                            selectedAll = false;
                            break;
                        }
                    }

                    if (selectedAll)
                    {
                        SelectedIcon_All.SetActive(true);
                    }
                    else
                    {
                        SelectedIcon_All.SetActive(false);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError(ex);
                throw;
            }
        }
    }

}