using Ricimi;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Code.Controllers.MessageBox
{
    public class MessageBoxController : Popup
    {
        public GameObject Template_Info;
        public GameObject Template_Warn;
        public GameObject Template_Error;

        public TextMeshProUGUI Caption;
        public int MaximumCaptionLength = 50;
        public TextMeshProUGUI Text;
        public int MaximumMessageLength = 78;

        private MessageBoxResult m_messageBoxResult = MessageBoxResult.Cancel;

        public MessageBoxResult MessageBoxResult { get => m_messageBoxResult; }
        
        private MessageBoxType m_type = MessageBoxType.Information;

        private RSG.Promise<MessageBoxResult> m_promise;

        // Start is called before the first frame update
        void Start()
        {

        }

        public void SetType(MessageBoxType type)
        {
            m_type = type;

            switch (type)
            {
                case MessageBoxType.Information:
                    {
                        Template_Info.SetActive(true);
                        Template_Warn.SetActive(false);
                        Template_Error.SetActive(false);
                        break;
                    }

                case MessageBoxType.Warning:
                    {
                        Template_Warn.SetActive(true);
                        Template_Info.SetActive(false);
                        Template_Error.SetActive(false);
                        break;
                    }

                case MessageBoxType.Error:
                    {
                        Template_Error.SetActive(true);
                        Template_Warn.SetActive(false);
                        Template_Info.SetActive(false);
                        break;
                    }
            }
        }

        public void SetCaption(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                text = "";

            if (text.Length > MaximumCaptionLength)
                Debug.LogWarning("Слишком большой текст в заголовке сообщения!");

            Caption.text = text;
        }

        public void SetText(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                text = "<текст сообщения не задан>";

            if (text.Length > MaximumMessageLength)
                Debug.LogWarning("Слишком большой текст в окне сообщения!");

            Text.text = text;
        }

        /// <summary>
        /// 0 - OK, 1- Cancel, 2 - Yes, 3 - No, 4- Abort
        /// </summary>
        /// <param name="boxButtons"></param>
        public void OnClickButton(int boxButtons)
        {
            m_messageBoxResult = (MessageBoxResult)boxButtons;
            Close();
        }

        public RSG.Promise<MessageBoxResult> OpenExclusive()
        {
            m_promise = new RSG.Promise<MessageBoxResult>();

            Open(false);

            return m_promise;
        }

        public new void Close()
        {
            base.Close();
            m_promise.Resolve(m_messageBoxResult);
        }
    }
}