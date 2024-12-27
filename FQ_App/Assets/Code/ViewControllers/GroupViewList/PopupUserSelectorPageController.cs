using Ricimi;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Code.Controllers.MessageBox;
using Assets.Code.Controllers;
using Code.Models.REST;
using Code.Models.REST.Rewards;
using System;
using Code.Models;
using System.Linq;
using Code.Models.REST.Users;
using Code.Controllers;
using Code.ViewControllers.TList;

public class PopupUserSelectorPageController : MonoBehaviour
{
    public TListViewController ScrollRect_ChildrensGroup;

    [HideInInspector]
    public bool readOnlyMode = false;

    private Popup m_thisPopup;

    public delegate void AfterCredentialsSelected();
    private AfterCredentialsSelected m_afterCredentialsSelectedDelegate = null;
    public AfterCredentialsSelected AfterCredentialsSelectedDelegate { get => m_afterCredentialsSelectedDelegate; set => m_afterCredentialsSelectedDelegate = value; }

    void Start()
    {
        try
        {
            m_thisPopup = GetComponent<Popup>();

            //ScrollRect_ChildrensGroup.SetListItems(CredentialsModel.ToListOfDictionary(DataModel.Instance.Credentials.ChildrenUsers));

            if (readOnlyMode)
            {
                var listItems = m_thisPopup.GetComponentsInChildren<Item_GroupListSimple_Controler>();

                foreach (var listItem in listItems)
                {
                    listItem.readOnlyMode = true;
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogError(ex);
            throw;
        }
    }

    public void ValidateNotEmpty(TMP_InputField inputField)
    {
        try
        {
            string text = inputField.text;

            if (string.IsNullOrWhiteSpace(text))
            {
                ShowExclamation(inputField, true);
            }
            else
            {
                ShowExclamation(inputField, false);
            }
        }
        catch (Exception ex)
        {
            Debug.LogError(ex);
            throw;
        }
    }

    private static void ShowExclamation(TMP_InputField inputField, bool show)
    {
        try
        {
            var excl = inputField.transform.Find("CImage_Exclamation");
            excl?.gameObject.SetActive(show);
        }
        catch (Exception ex)
        {
            Debug.LogError(ex);
            throw;
        }
    }

    public void OnClick_ButtonChange()
    {
        try
        {
            bool totalUnselectResult = true;

            foreach (var user in DataModel.Instance.Credentials.ChildrenUsers)
            {
                totalUnselectResult = totalUnselectResult && !user.Selected;
            }

            if (totalUnselectResult)
            {
                foreach (var user in DataModel.Instance.Credentials.ChildrenUsers)
                {
                    user.Selected = true;
                }
            }

            ReturnAndClose();
        }
        catch (Exception ex)
        {
            Debug.LogError(ex);
            throw;
        }
    }

    private bool ReadForm(out Dictionary<string, string> taskProps)
    {
        try
        {
            taskProps = new Dictionary<string, string>();
            var inputFields = transform.GetComponentsInChildren<TMP_InputField>();

            foreach (var inputField in inputFields)
            {
                if (inputField.IsActive())
                {
                    if (!string.IsNullOrWhiteSpace(inputField.text))
                    {
                        string propName = inputField.name.Replace("Input_", "");
                        taskProps.Add(propName, inputField.text);
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            return true;
        }
        catch (Exception ex)
        {
            Debug.LogError(ex);
            throw;
        }
    }

    //TODO: не актуально?
    public void ShowTooltip(string text)
    {
        //if (!m_tooltipController.IsActive)
        //    m_tooltipController.Show(text);
        //else
        //    m_tooltipController.Hide();

    }

    public void SetDestinationUserId(string userId)
    {
        //if (Guid.TryParse(userId, out Guid destUserId))
        //{
        //    var destinationUser = DataModel.Instance.Credentials.Users.Where(x => x.Id == destUserId).FirstOrDefault();

        //    if (destinationUser != null)
        //    {
        //        m_DestUser = destinationUser;
        //    }
        //    else
        //    {
        //        Global_MessageBoxHandlerController.ShowMessageBox("Упс..", $"Не удалось выполнить обновление данных пользователя: кажется, что-то пошло не так...\nПожалуйста, повторите операцию позднее.", MessageBoxType.Warning);
        //        return;
        //    }
        //}
        //else
        //{
        //    Global_MessageBoxHandlerController.ShowMessageBox("Упс..", $"Не удалось выполнить обновление данных пользователя: кажется, что-то пошло не так...\nПожалуйста, повторите операцию позднее.", MessageBoxType.Warning);
        //    return;
        //}
    }

    private void ReturnAndClose()
    {
        try
        {
            if (m_afterCredentialsSelectedDelegate != null)
                m_afterCredentialsSelectedDelegate();
            m_thisPopup.Close();
        }
        catch (Exception ex)
        {
            Debug.LogError(ex);
            throw;
        }
    }
}
