using Code.Models.REST.CommonType.Tasks;
using UnityEngine;
using Code.Controllers;

namespace Code.ViewControllers
{
    public class StatusWindowController : MonoBehaviour
    {
        public GameObject ButtonAccept;
        public GameObject ButtonReject;
        public GameObject ButtonCancel;

        public bool IsOpened => this.gameObject.activeInHierarchy;

        public void Open(BaseTaskStatus current)
        {
            //check current status
            if (TaskLogicController.CheckTransition(current, BaseTaskStatus.Successed))
                ButtonAccept.SetActive(true);

            if (TaskLogicController.CheckTransition(current, BaseTaskStatus.Failed))
                ButtonReject.SetActive(true);

            if (TaskLogicController.CheckTransition(current, BaseTaskStatus.Canceled))
                ButtonCancel.SetActive(true);

            // если смена статуса невозможна, не открываем окно
            if (!(ButtonAccept.activeSelf ||
                ButtonReject.activeSelf ||
                ButtonCancel.activeSelf))
                return;

            this.gameObject.SetActive(true);
        }

        public void Close()
        {
            this.gameObject.SetActive(false);
            ButtonCancel.SetActive(false);
            ButtonReject.SetActive(false);
            ButtonAccept.SetActive(false);
        }
    }
}