using System;
using UnityEngine;

namespace Code.Controllers.MessageBox
{
    public class Global_MessageBoxHandlerController : MonoBehaviour
    {
        private MessageBoxOpener m_messageBoxBottomOpener;
        private MessageBoxOpener m_messageBoxCenterOpener;
        private MessageBoxOpener m_messageBoxTopOpener;

        // Start is called before the first frame update
        void Start()
        {
            m_messageBoxBottomOpener = GetComponent<MessageBoxOpener>();
            m_messageBoxCenterOpener = GetComponent<MessageBoxOpener>();
            m_messageBoxTopOpener = GetComponent<MessageBoxOpener>();
        }

        public static RSG.Promise<MessageBoxResult> ShowMessageBox(
            string caption, string text,
            MessageBoxType type = MessageBoxType.Information, 
            MessageBoxButtonsType buttons = MessageBoxButtonsType.Ok, 
            MessageBoxPosition pos = MessageBoxPosition.Center
            )
        {
            var boxController = GameObject.Find("MessageBoxHandler").GetComponent<Global_MessageBoxHandlerController>();
            return boxController.Show(caption, text, type, buttons, pos);
        }

        public RSG.Promise<MessageBoxResult> Show(
            string caption, 
            string text, 
            MessageBoxType type = MessageBoxType.Information, 
            MessageBoxButtonsType buttons = MessageBoxButtonsType.Ok, 
            MessageBoxPosition pos = MessageBoxPosition.Center
            )
        {
            RSG.Promise<MessageBoxResult> result = null;
            switch (pos)
            {
                case MessageBoxPosition.Top:
                    throw new NotImplementedException();

                case MessageBoxPosition.Center:
                    result = ShowMessageBoxCenter(caption, text, type, buttons);
                    break;

                case MessageBoxPosition.Bottom:
                    throw new NotImplementedException();

                default:
                    break;
            }
            result?.Then((res) => 
            { 
                Debug.Log($"result: {res.ToString()}"); 
            });

            return result;
        }

        public void TestMessageBox()
        {
            Show("CAPTION", "TEXT", MessageBoxType.Information, MessageBoxButtonsType.Ok, MessageBoxPosition.Center)
                .ContinueWith(() => Show("CAPTION", "TEXT", MessageBoxType.Information, MessageBoxButtonsType.OkCancel, MessageBoxPosition.Center))
                    .ContinueWith(() => Show("CAPTION", "TEXT", MessageBoxType.Information, MessageBoxButtonsType.OkCancelAbort, MessageBoxPosition.Center))
                        .ContinueWith(() => Show("CAPTION", "TEXT", MessageBoxType.Information, MessageBoxButtonsType.YesNo, MessageBoxPosition.Center))
                            .ContinueWith(() => Show("CAPTION", "TEXT", MessageBoxType.Information, MessageBoxButtonsType.YesNoAbort, MessageBoxPosition.Center));
        }

        private RSG.Promise<MessageBoxResult> ShowMessageBoxCenter(
            string caption, 
            string text, 
            MessageBoxType type = MessageBoxType.Information, 
            MessageBoxButtonsType buttons = MessageBoxButtonsType.Ok 
            )
        {
            return m_messageBoxCenterOpener.Show(caption, text, type, buttons);
             
        }
    }

    public enum MessageBoxType : int
    {
        Information,
        Warning,
        Error
    }

    public enum MessageBoxPosition : int
    {
        Top,
        Center,
        Bottom
    }

    public enum MessageBoxButtonsType : int
    {
        Ok,
        OkCancel,
        YesNo,
        OkCancelAbort,
        YesNoAbort
    }

    public enum MessageBoxResult : int
    {
        Ok,
        Cancel,
        Yes,
        No,
        Abort
    }
}