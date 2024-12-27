using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Code.ViewControllers.TList
{
    [RequireComponent(typeof(TextFieldsFiller))]
    public class TListItemController : MonoBehaviour
    {
        [Tooltip("Image using for showing select")]
        public Image SelectSign;
        [Tooltip("Main button that opens popup with task details")]
        public GameObject ButtonOpenDetails;
        [Tooltip("Button that will be activate instead opens popup with task details")]
        public GameObject ButtonSelect;

        private bool m_isSelected = false;
        public bool IsSelected { get => m_isSelected; }

        public Dictionary<string, object> Data;

        private TextFieldsFiller m_textFieldsFiller;

        /// <summary>
        /// Событие, что список изменен
        /// </summary>
        public event EventHandler ItemChanged;
        protected virtual void OnItemChanged(ItemChangedArgs e)
        {
            EventHandler handler = ItemChanged;
            handler?.Invoke(this, e);
        }
        public class ItemChangedArgs : EventArgs
        {
            bool isSelected = false;
            public ItemChangedArgs(bool selected)
            {
                isSelected = selected;
            }
        }


        private void Awake()
        {
            m_textFieldsFiller = GetComponent<TextFieldsFiller>();
        }

        /// <summary>
        /// Установка значений для элемента. 
        /// Из <paramref name="data"/> заполняются поля, определенные в <see cref="TextFieldsFiller"/>.
        /// </summary>
        /// <param name="data"></param>
        public void SetData(Dictionary<string, object> data)
        {
            if (data == null)
                return;

            Data = data;
            m_textFieldsFiller.SetData(Data);
        }

        /// <summary>
        /// Выбор или снятие выбора с элемента.
        /// </summary>
        /// <param name="enable">true - выбран</param>
        public void SetSelectMode(bool enable)
        {
            if (enable)
            {
                ButtonOpenDetails.SetActive(false);
                ButtonSelect.SetActive(true);
                SelectSign.gameObject.SetActive(true);
                m_isSelected = true;
            }
            else
            {
                ButtonOpenDetails.gameObject.SetActive(true);
                ButtonSelect.SetActive(false);
                SelectSign.gameObject.SetActive(false);
                m_isSelected = false;
            }
        }

        public void SwitchSelect()
        {
            m_isSelected = !m_isSelected;
            if (m_isSelected)
            {
                SelectSign.color = new Color(SelectSign.color.r, SelectSign.color.g, SelectSign.color.b, 1f);
            }
            else
            {
                SelectSign.color = new Color(SelectSign.color.r, SelectSign.color.g, SelectSign.color.b, 0f);
            }
            OnItemChanged(new ItemChangedArgs(m_isSelected));
        }

        public void SetSelection(bool isSelect)
        {
            m_isSelected = isSelect;
            if (m_isSelected)
            {
                SelectSign.color = new Color(SelectSign.color.r, SelectSign.color.g, SelectSign.color.b, 1f);
            }
            else
            {
                SelectSign.color = new Color(SelectSign.color.r, SelectSign.color.g, SelectSign.color.b, 0f);
            }
        }

    }
}