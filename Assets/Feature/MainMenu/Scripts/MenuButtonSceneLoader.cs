using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Feature.MainMenu.Scripts
{
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
            SceneManager.LoadScene(sceneName);
        }
    }
}
