using Ricimi;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Code.Controllers.MessageBox.MessageBoxController;

namespace Code.Controllers.MessageBox
{
    public class MessageBoxOpener : PopupOpener
    {
        public GameObject OkPrefab;
        public GameObject OkCancelPrefab;
        public GameObject YesNoPrefab;
        public GameObject OkCancelAbortPrefab;
        public GameObject YesNoAbortPrefab;
        
        // Start is called before the first frame update
        //void Start()
        //{
        //    m_canvas = GameObject.Find("Canvas").GetComponent<Canvas>();
        //}

        public RSG.Promise<MessageBoxResult> Show(string caption, string text, MessageBoxType type = MessageBoxType.Information, MessageBoxButtonsType buttons = MessageBoxButtonsType.Ok)
        {
            GameObject openedMessageBox = null;
            RSG.Promise<MessageBoxResult> promise = null;
            GameObject currentPrefab = null;
            switch (buttons)
            {
                case MessageBoxButtonsType.Ok:
                    currentPrefab = OkPrefab;
                    break;
                case MessageBoxButtonsType.OkCancel:
                    currentPrefab = OkCancelPrefab;
                    break;
                case MessageBoxButtonsType.YesNo:
                    currentPrefab = YesNoPrefab;
                    break;
                case MessageBoxButtonsType.OkCancelAbort:
                    currentPrefab = OkCancelAbortPrefab;
                    break;
                case MessageBoxButtonsType.YesNoAbort:
                    currentPrefab = YesNoAbortPrefab;
                    break;
                default:
                    currentPrefab = OkPrefab;
                    break;
            }

            promise = Show(currentPrefab, out openedMessageBox);
            var messageBoxController = openedMessageBox.GetComponent<MessageBoxController>();

            try
            {
                messageBoxController.SetType(type);
            }
            catch
            {

            }

            messageBoxController.SetCaption(caption);
            messageBoxController.SetText(text);

            

            return promise;
        }

        public RSG.Promise<MessageBoxResult> Show(GameObject prefab, out GameObject messageBox)
        {
            messageBox = Instantiate(prefab) as GameObject;
            messageBox.SetActive(true);
            messageBox.transform.localScale = Vector3.zero;
            messageBox.transform.SetParent(m_canvas.transform, false);
            return messageBox.GetComponent<MessageBoxController>().OpenExclusive();
        }
    }
}