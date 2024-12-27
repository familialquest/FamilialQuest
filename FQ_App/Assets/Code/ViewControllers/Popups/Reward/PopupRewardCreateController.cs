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
using static Assets.Code.Models.REST.CommonTypes.FQServiceException;
using Assets.Code.Models.REST.CommonTypes;

public class PopupRewardCreateController : MonoBehaviour
{
    public GameObject Placeholder_AvailableUsers;

    public GameObject CircleProgressBar;

    //bool IsAllFieldValid = false;
    private Popup m_thisPopup;

    private TooltipController m_tooltipController;

    private List<Guid> availableFor = new List<Guid>();

    // Start is called before the first frame update
    void Start()
    {
        try
        {
            SetActualSelectedUsers();

            m_thisPopup = GetComponent<Popup>();
            m_tooltipController = GetComponent<TooltipController>();
        }
        catch (Exception ex)
        {
            Debug.LogError(ex);
            throw;
        }
    }

    //TODO: не актуально?
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

    //TODO: не актуально?
    private static void ShowExclamation(TMP_InputField inputField, bool show)
    {
        var excl = inputField.transform.Find("CImage_Exclamation");
        excl?.gameObject.SetActive(show);
    }

    public void OnClick_ButtonCreate()
    {
        try
        {
            if (!ReadForm(out var rewardProps))
            {
                throw new FQServiceException(FQServiceException.FQServiceExceptionType.EmptyRequiredField);
            }

            if (rewardProps.ContainsKey("Cost"))
            {
                if (!VerifyCost(rewardProps["Cost"]))
                {
                    Global_MessageBoxHandlerController.ShowMessageBox("Ещё одна деталь", "Минимальная стоимость сокровища: 1 монета.", MessageBoxType.Information);
                    return;
                }
            }

            Global_MessageBoxHandlerController.ShowMessageBox("Новое сокровище", "Экземпляр сокровища будет объявлен отдельно для каждого указанного Героя\n\nПродолжить?", MessageBoxType.Information, MessageBoxButtonsType.OkCancel)
                  .Then((dialogRes) =>
                  {
                      if (dialogRes == MessageBoxResult.Ok)
                      {
                          CircleProgressBar.SetActive(true);

                          Reward r = new Reward();
                          r.Cost = Convert.ToInt32(rewardProps["Cost"]);
                          r.Title = rewardProps["Title"];

                          if (rewardProps.ContainsKey("Description"))
                          {
                              r.Description = rewardProps["Description"];
                          }

                          RewardController.CreateReward(r, availableFor)
                              .Then((res) =>
                              {
                                  Debug.Log($"status: {res.status}");

                                  if (res.result)
                                  {
                                      //Сброс селекта с пользаков AvailableFor
                                      foreach (var user in DataModel.Instance.Credentials.ChildrenUsers)
                                      {
                                          user.Selected = false;
                                      }

                                      Global_MessageBoxHandlerController.ShowMessageBox("Новое сокровище", $"Сокровище успешно объявлено!");
                                      m_thisPopup.Close();
                                  }
                                  else
                                  {
                                      CircleProgressBar.SetActive(false);
                                  }
                              })
                              .Catch((ex) =>
                              {
                                  //Если сюда попали, то ошибку уже обработали в DataModel и показали уведомление
                                  //Нужно только убрать индикацию загрузки
                                  CircleProgressBar.SetActive(false);
                              });
                      }
                  })
                  .Catch((ex) =>
                  {
                      Debug.LogError(ex);

                      CircleProgressBar.SetActive(false);

                      FQServiceException.ShowExceptionMessage(ex);
                  });
        }
        catch (Exception ex)
        {
            Debug.LogError(ex);

            CircleProgressBar.SetActive(false);

            FQServiceException.ShowExceptionMessage(ex);
        }
    }

    public void OnClick_ButtonSelectUser(Ricimi.PopupOpener editPopupOpener)
    {
        try
        {
            SetActualSelectedUsers();

            editPopupOpener.OpenPopup(out var popup);
            PopupUserSelectorPageController userSelectorPageController = popup.GetComponent<PopupUserSelectorPageController>();
            userSelectorPageController.AfterCredentialsSelectedDelegate = AfterCredentialsSelected;
        }
        catch (Exception ex)
        {
            Debug.LogError(ex);

            CircleProgressBar.SetActive(false);

            FQServiceException.ShowExceptionMessage(ex);
        }
    }

    private void AfterCredentialsSelected()
    {
        var inputFields = transform.GetComponentsInChildren<TMP_InputField>();

        var input_availableFor = inputFields.Where(x => x.name.Equals("Input_Available")).FirstOrDefault();        

        if (input_availableFor != null)
        {
            var textComponent = input_availableFor.GetComponentsInChildren<TMP_Text>().FirstOrDefault();

            if (textComponent != null)
            {
                var destinationUsersNames = new List<string>();

                textComponent.text = string.Empty;
                availableFor.Clear();

                var selectedUsers = DataModel.Instance.Credentials.ChildrenUsers.Where(x => x.Selected);

                if (selectedUsers.Count() > 0)
                {                    
                    foreach (var selectedUser in selectedUsers)
                    {
                        availableFor.Add(selectedUser.Id);
                    }

                    //Отображение
                    Placeholder_AvailableUsers.SetActive(false);

                    destinationUsersNames = new List<string>(selectedUsers.Select(x1 => x1.Name).OrderBy(x2 => x2));

                    if (destinationUsersNames.Count() == 1)
                    {
                        textComponent.text = destinationUsersNames.First();
                    }
                    else
                    {
                        if (destinationUsersNames.Count() == DataModel.Instance.Credentials.ChildrenUsers.Count)
                        {
                            textComponent.text = "Все";
                        }
                        else
                        {
                            textComponent.text = string.Join(", ", destinationUsersNames);
                        }
                    }
                }

                TextFieldsFiller.TruncateAvailableUsers(textComponent, null, destinationUsersNames.Count());
            }
            else
            {
                ; //TODO
            }
        }
        else
        {
            ; //TODO
        }
    }

    private void SetActualSelectedUsers()
    {
        foreach (var user in DataModel.Instance.Credentials.ChildrenUsers)
        {
            user.Selected = false;
        }

        List<User> destinationUsers = new List<User>();
        
        destinationUsers = DataModel.Instance.Credentials.ChildrenUsers.Where(x => availableFor.Contains(x.Id)).ToList();        

        foreach (var user in destinationUsers)
        {
            user.Selected = true;
        }

        AfterCredentialsSelected();
    }

    private bool ReadForm(out Dictionary<string, string> taskProps)
    {        
        taskProps = new Dictionary<string, string>();
        var inputFields = transform.GetComponentsInChildren<TMP_InputField>();

        foreach (var inputField in inputFields)
        {
            if (inputField.IsActive())
            {
                //Available - Здесь отображаемые имена игнорим. Реальные данные уже в availableFor
                if (!inputField.name.Contains("Available"))
                {
                    if (!string.IsNullOrWhiteSpace(inputField.text) || inputField.name.Contains("Description"))
                    {
                        string propName = inputField.name.Replace("Input_", "");
                        taskProps.Add(propName, inputField.text.Trim());
                    }
                    else
                    {
                        return false;
                    }
                }
            }
        }

        //Если здесь пустой - значит пользователей не добавили.
        if (availableFor.Count == 0)
        {
            return false;
        }

        return true;
    }

    private bool VerifyCost(string cost)
    {
        if (Int32.TryParse(cost, out int costInt) && costInt > 0)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public void ShowTooltip(string text)
    {
        if (!m_tooltipController.IsActive)
            m_tooltipController.Show(text);
        else
            m_tooltipController.Hide();
        
    }
}
