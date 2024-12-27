using UnityEngine;
using Code.Models.RoleModel;
using UnityEngine.UI;

namespace Code.ViewControllers
{
    /// <summary>
    /// Отключает объект в случае несовпадения указанной для него роли с текущей ролью пользователя.
    /// Используется в случае, если объект для роли один и он не входит в группу, в которой по одному элементу для каждой роли,
    /// или наоборот - объектов для одной роли несколько, а определение, какой объект будет включаться, происходит другим контроллером.
    /// </summary>
    public class RoleApplicator : MonoBehaviour
    {
        /// <summary>
        /// Для каких ролей данный элемент будет доступен.
        /// В случае другой роли этот элемент будет:
        /// - скрыт полностью
        /// - заменен на <see cref="Replacement"/>
        /// - будет отключено взаимодействие у "интерактивных" элементов, если включен <see cref="DisableInteractable"/>
        /// </summary>
        public RoleTypes[] AvailableFor;

        /// <summary>
        /// Объект, который будет включен вместо текущего.
        /// Если не указан, элемент будет просто отключен или скрыт.
        /// </summary>
        public GameObject Replacement; // TODO: возможная замена на префаб

        /// <summary>
        /// Отключать взаимодействие с элементом без его скрытия.
        /// </summary>
        public bool DisableInteractable;

        private void Awake()
        {
            if (RoleModel.Instance.Contains(AvailableFor))
            {
                this.gameObject.SetActive(true);
                if (DisableInteractable)
                {
                    if (TryGetComponent<Selectable>(out var select))
                        select.interactable = true;
                }

                if (Replacement != null)
                    Replacement.SetActive(false);
            }
        }

        /// <summary>
        /// Ловим все попытки включения этого элемента.
        /// Включать или не включать определяется совпадением ролей.
        /// </summary>
        private void OnEnable()
        {
            if (!RoleModel.Instance.Contains(AvailableFor))
            {
                if (DisableInteractable)
                {
                    if (TryGetComponent<Selectable>(out var select))
                        select.interactable = false;
                }
                else
                {
                    this.gameObject.SetActive(false);
                }
                if (Replacement != null)
                    Replacement.SetActive(true);
            }
        }

        /// <summary>
        /// Ловим все попытки показа этого элемента.
        /// Включать или не включать определяется совпадением ролей.
        /// (ни разу не попадало, но на всякий случай)
        /// </summary>
        private void OnBecameVisible()
        {
            if (!RoleModel.Instance.Contains(AvailableFor))
            {
                if (DisableInteractable)
                {
                    if (TryGetComponent<Selectable>(out var select))
                        select.interactable = false;
                }
                else
                {
                    this.gameObject.SetActive(false);
                }
                if (Replacement != null)
                    Replacement.SetActive(true);
            }
        }
    }
}