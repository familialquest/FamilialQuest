using Code.Models;
using System;
using TMPro;
using UnityEngine;
using Code.ViewControllers.TList;
using Code.Models.REST;
using UnityEngine.UI;
using Assets.Code.Models.REST.CommonTypes;
using Code.Controllers.MessageBox;
using System.Linq;
using Ricimi;

namespace Code.ViewControllers
{
    public class WizardPageController : MonoBehaviour
    {
        public GameObject thisForm;
        public GameObject WizardBG;

        public GameObject[] StepsBodies;
        //public GameObject[] StepsIcons_Active;

        void Awake()
        {
        }

        public void OnChangeStep(int activeStep)
        {
            foreach (var stepBody in StepsBodies)
            {
                stepBody.SetActive(false);
            }
            StepsBodies[activeStep].SetActive(true);


            //foreach (var activeStepIcon in StepsIcons_Active)
            //{
            //    activeStepIcon.SetActive(false);
            //}
            //for (int i = 0; i <= activeStep; i++)
            //{
            //    StepsIcons_Active[i].SetActive(true);
            //}
        }

        public void OnComplited()
        {
            //OnChangeStep(0);   
            //thisForm.SetActive(false);
            //WizardBG.SetActive(false);
            var m_thisPopup = GetComponent<Popup>();

            m_thisPopup.Close();

        }
    }
}