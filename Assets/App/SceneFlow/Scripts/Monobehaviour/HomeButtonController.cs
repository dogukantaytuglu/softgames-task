using UnityEngine;
using UnityEngine.UI;

namespace SceneFlow.Monobehaviour
{
    [RequireComponent(typeof(Button))]
    public class HomeButtonController : MonoBehaviour
    {
        [SerializeField] private Button button;

        private void Awake()
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(GoHome);
        }

        private void OnValidate()
        {
            if (TryGetComponent<Button>(out var b))
            {
                button = b;
            }
        }

        public void GoHome()
        {
            if (SceneFlowController.Instance == null)
            {
                Debug.LogWarning("HomeButtonController: no SceneFlowController in the scene.");
                return;
            }

            SceneFlowController.Instance.NavigateHome();
        }
    }
}
