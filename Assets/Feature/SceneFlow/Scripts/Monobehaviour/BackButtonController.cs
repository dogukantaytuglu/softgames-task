using UnityEngine;
using UnityEngine.UI;

namespace SceneFlow.Monobehaviour
{
    [RequireComponent(typeof(Button))]
    public class BackButtonController : MonoBehaviour
    {
        [SerializeField] private Button button;

        private void Awake()
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(GoBack);
        }

        private void OnValidate()
        {
            if (TryGetComponent<Button>(out var b))
            {
                button = b;
            }
        }

        public void GoBack()
        {
            SceneFlowController.Instance.NavigateHome();
        }
    }
}
