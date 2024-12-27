using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Ricimi;

using Code.Controllers.MessageBox;
using Code.Controllers;
using Code.Models.REST.CommonType.Tasks;
using System.Linq;
using Code.Models;
using Newtonsoft.Json;
using Code.Models.REST.Users;
using UnityEngine.EventSystems;

namespace Code.ViewControllers
{
    public enum DTPickerButtonTypes
    {
        Add_Day = 0,
        Deduct_Day,
        Add_Hour = 10,
        Deduct_Hour,
        Add_Minute = 20,
        Deduct_Minute
    }

    public class DateTimePickerButton : MonoBehaviour, IUpdateSelectedHandler, IPointerDownHandler, IPointerUpHandler
    {
        public DateTimePickerController DTPicker;
        public DTPickerButtonTypes DTPickerButtonType;

        private bool isPressed = false;
        private int counter = 0;

        //darkmagic
        public void OnUpdateSelected(BaseEventData data)
        {
            if (isPressed)
            {         
                switch (DTPickerButtonType)
                {
                    case DTPickerButtonTypes.Add_Day:
                        DTPicker.OnClick_ButtonAdd_Day();
                        break;
                    case DTPickerButtonTypes.Deduct_Day:
                        DTPicker.OnClick_ButtonDeduct_Day();
                        break;
                    case DTPickerButtonTypes.Add_Hour:
                        DTPicker.OnClick_ButtonAdd_Hour();
                        break;
                    case DTPickerButtonTypes.Deduct_Hour:
                        DTPicker.OnClick_ButtonDeduct_Hour();
                        break;
                    case DTPickerButtonTypes.Add_Minute:
                        DTPicker.OnClick_ButtonAdd_Minute();
                        break;
                    case DTPickerButtonTypes.Deduct_Minute:
                        DTPicker.OnClick_ButtonDeduct_Minute();
                        break;

                }

                isPressed = false; 
                counter++;
            }
            else
            {
                if (counter > 0)
                {
                    if (counter < 5)
                    {
                        counter++;
                        System.Threading.Thread.Sleep(60);
                    }
                    else
                    {
                        switch (DTPickerButtonType)
                        {
                            case DTPickerButtonTypes.Add_Day:
                                DTPicker.OnClick_ButtonAdd_Day();
                                break;
                            case DTPickerButtonTypes.Deduct_Day:
                                DTPicker.OnClick_ButtonDeduct_Day();
                                break;
                            case DTPickerButtonTypes.Add_Hour:
                                DTPicker.OnClick_ButtonAdd_Hour();
                                break;
                            case DTPickerButtonTypes.Deduct_Hour:
                                DTPicker.OnClick_ButtonDeduct_Hour();
                                break;
                            case DTPickerButtonTypes.Add_Minute:
                                DTPicker.OnClick_ButtonAdd_Minute();
                                break;
                            case DTPickerButtonTypes.Deduct_Minute:
                                DTPicker.OnClick_ButtonDeduct_Minute();
                                break;

                        }
                        System.Threading.Thread.Sleep(60);
                    }
                }
            }
        }

        public void OnPointerDown(PointerEventData data)
        {
            isPressed = true;            
        }
        public void OnPointerUp(PointerEventData data)
        {
            isPressed = false;
            counter = 0;
        }
    }
}