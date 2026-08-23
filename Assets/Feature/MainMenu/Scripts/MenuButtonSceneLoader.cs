using SceneFlow.Monobehaviour;
using UnityEngine;
using UnityEngine.UI;

namespace Feature.MainMenu.Scripts
{
	[RequireComponent(typeof(Button))]
    public class MenuButtonSceneLoader : MonoBehaviour
    {
        [SerializeField] private string sceneName;
        [SerializeField] private Button button;

        private void Awake()
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(LoadScene);
        }

        private void OnValidate()
        {
            if (TryGetComponent<Button>(out var b))
            {
                button = b;
            }
        }

        public void LoadScene()
        {
            if (SceneFlowController.Instance == null)
            {
                Debug.LogWarning("MenuButtonSceneLoader: no SceneFlowController in the scene.");
                return;
            }

            SceneFlowController.Instance.Navigate(sceneName);
        }
    }
}
