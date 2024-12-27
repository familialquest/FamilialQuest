using Code.Models.REST.CommonType.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TaskStatusIconController : MonoBehaviour
{
    public GameObject NewSign;
    public GameObject CreatedStatus;
    public GameObject AnnouncedStatus;
    public GameObject InProgressStatus;
    public GameObject CompletedStatus;
    public GameObject PendingReview;
    public GameObject SuccessedStatus;
    public GameObject FailedStatus;
    public GameObject CanceledStatus;
    public GameObject DeclinedStatus;
    public GameObject AvailableUntilPassedStatus;
    public GameObject SolutionTimeOverStatus;

    // Start is called before the first frame update
    void Start()
    {
        //SwitchOffAllStatus();
    }

    private void SwitchOffAllStatus()
    {
        try
        {
            NewSign.SetActive(false);
            CreatedStatus.SetActive(false);
            AnnouncedStatus.SetActive(false);
            InProgressStatus.SetActive(false);
            CompletedStatus.SetActive(false);
            PendingReview.SetActive(false);
            SuccessedStatus.SetActive(false);
            CanceledStatus.SetActive(false);
            DeclinedStatus.SetActive(false);
            FailedStatus.SetActive(false);
            AvailableUntilPassedStatus.SetActive(false);
            SolutionTimeOverStatus.SetActive(false);
        }
        catch (Exception ex)
        {
            Debug.LogError(ex);
            throw;
        }
    }

    private void SwitchOnStatus(GameObject statusObject)
    {
        try
        {
            SwitchOffAllStatus();
            statusObject.SetActive(true);
            return;

            if (statusObject == InProgressStatus)
            {
                InProgressStatus.SetActive(true);
                CompletedStatus.SetActive(false);
                FailedStatus.SetActive(false);
            }
            else if (statusObject == CompletedStatus)
            {
                InProgressStatus.SetActive(false);
                CompletedStatus.SetActive(true);
                FailedStatus.SetActive(false);
            }
            else if (statusObject == FailedStatus)
            {
                InProgressStatus.SetActive(false);
                CompletedStatus.SetActive(false);
                FailedStatus.SetActive(true);
            }
        }
        catch (Exception ex)
        {
            Debug.LogError(ex);
            throw;
        }
    }

    public void SetStatus(BaseTaskStatus currentStatus, TextFieldsFiller m_textFieldsFiller = null)
    {
        try
        {
            switch (currentStatus)
            {
                case BaseTaskStatus.Created:
                    CreatedStatus.SetActive(true);
                    break;
                case BaseTaskStatus.Assigned:
                    //TODO: временно отключено
                    //if (m_textFieldsFiller != null && DateTime.TryParse(m_textFieldsFiller.TextData["CreationDate"], out DateTime taskCretionDate))
                    //{
                    //    var diffTime = DateTime.UtcNow - taskCretionDate;

                    //    if (diffTime.TotalMinutes > 0 && diffTime.TotalMinutes < 120) //TODO: (не забыть вынести константу)
                    //    {
                    //        NewSign.SetActive(true);
                    //    }
                    //}
                    AnnouncedStatus.SetActive(true);
                    break;
                case BaseTaskStatus.Accepted:
                case BaseTaskStatus.InProgress:
                    SwitchOnStatus(InProgressStatus);
                    break;
                case BaseTaskStatus.Completed:
                    SwitchOnStatus(CompletedStatus);
                    break;
                case BaseTaskStatus.PendingReview:
                    SwitchOnStatus(PendingReview);
                    break;
                case BaseTaskStatus.Successed:
                case BaseTaskStatus.Closed:
                    SwitchOnStatus(SuccessedStatus);
                    break;
                case BaseTaskStatus.Deleted:
                    break;
                case BaseTaskStatus.AvailableUntilPassed:
                    SwitchOnStatus(AvailableUntilPassedStatus);
                    break;
                case BaseTaskStatus.SolutionTimeOver:
                    SwitchOnStatus(SolutionTimeOverStatus);
                    break;
                case BaseTaskStatus.Declined:
                    SwitchOnStatus(DeclinedStatus);
                    break;
                case BaseTaskStatus.Canceled:
                    SwitchOnStatus(CanceledStatus);
                    break;
                case BaseTaskStatus.Failed:
                    SwitchOnStatus(FailedStatus);
                    break;
                case BaseTaskStatus.None:
                    break;
                default:
                    break;
            }
        }
        catch (Exception ex)
        {
            Debug.LogError(ex);
            throw;
        }
    }
}
