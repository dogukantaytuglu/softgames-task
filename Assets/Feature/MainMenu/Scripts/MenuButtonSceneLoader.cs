using SceneServices.Monobehaviour;
using UnityEngine;
using UnityEngine.UI;

namespace Feature.MainMenu.Scripts
{
	[RequireComponent(typeof(Button))]
    public class MenuButtonSceneLoader : MonoBehaviour
    {
        [SerializeField] private string sceneName;
        [SerializeField] private Button button;

        public void Initialize()
        {
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
            if (SceneService.Instance == null)
            {
                Debug.LogWarning("MenuButtonSceneLoader: no SceneService in the scene.");
                return;
            }

            SceneService.Instance.Navigate(sceneName);
        }
    }
}
