using System;
using UnityEngine;
using UnityEngine.UI;

namespace Code.Models.RoleModel
{
    /// <summary>
    /// Включает соответствующий для роли элемент.
    /// Используется в случае группы, в которой по одному элементу для каждой роли,
    /// и другие контроллеры не влияют на их видимость/активность.
    /// </summary>
    public class GroupRoleApplicator : MonoBehaviour
    {        
        /// <summary>
        /// Описатель для соотношения роли-элемент
        /// </summary>
        [System.Serializable]
        public class RoleToObject
        {
            /// <summary>
            /// Роль 
            /// </summary>
            public RoleTypes role;
            /// <summary>
            /// Объект, применяемый при активной роли
            /// </summary>
            public GameObject obj;
            /// <summary>
            /// Отключать взаимодействие с элементом без его скрытия.
            /// </summary>
            public bool DisableInteractable;
        }

        /// <summary>
        /// Данная роль используется в случае, если в <see cref="RoleToObjects"/> отсутствует текущая роль пользователя.
        /// </summary>
        public RoleTypes DefaultRole = RoleTypes.User;

        /// <summary>
        /// Набор отношений роль-объект для роли.
        /// Для неуказанной роли будет использован <see cref="DefaultRole"/>
        /// </summary>
        public RoleToObject[] RoleToObjects;

        private void Awake()
        {
            try
            {
                RoleToObject defaultRto = null;
                bool isFound = false;
                foreach (var rto in RoleToObjects)
                {
                    if (DefaultRole == rto.role)
                        defaultRto = rto;

                    // нужный элемент включается, остальные отключаются
                    isFound = RoleModel.Instance.Equals(rto.role);
                    SetActiveObject(rto, isFound);
                }

                // если не найдена активная роль, то используется DefaultRole
                if (!isFound && defaultRto != null)
                    SetActiveObject(defaultRto, true);
            }
            catch (Exception ex)
            {
                Debug.LogError(ex);
                throw;
            }
        }

        private void SetActiveObject(RoleToObject rto, bool isEnable)
        {
            try
            {
                rto.obj.gameObject.SetActive(isEnable);
                if (isEnable && rto.DisableInteractable)
                {
                    if (rto.obj.TryGetComponent<Selectable>(out var select))
                        select.interactable = true;
                }
                return;
            }
            catch (Exception ex)
            {
                Debug.LogError(ex);
                throw;
            }
        }
    }

}