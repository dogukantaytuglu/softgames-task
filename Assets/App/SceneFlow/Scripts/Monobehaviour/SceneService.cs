using SceneFlow.Logic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SceneFlow.Monobehaviour
{
    public class SceneService : MonoBehaviour
    {
        public static SceneService Instance { get; private set; }

        [SerializeField] private string homeSceneName = SceneNames.MainMenu;

        private SceneFlowState _state;
        private string _pendingScene;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            _state = new SceneFlowState();

            var activeScene = SceneManager.GetActiveScene();
            if (activeScene.IsValid() && activeScene.name != gameObject.scene.name)
            {
                _state = new SceneFlowState(activeScene.name);
                return;
            }

            Navigate(homeSceneName);
        }

        public void Navigate(string sceneName)
        {
            if (!_state.TryBeginNavigation(sceneName, out var previousScene))
                return;

            if (!string.IsNullOrEmpty(previousScene))
                SceneManager.UnloadSceneAsync(previousScene);

            var loadOperation = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
            if (loadOperation == null)
            {
                Debug.LogError($"SceneService: '{sceneName}' is not registered in Build Settings.");
                _state.CompleteNavigation();
                return;
            }

            _pendingScene = sceneName;
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        public void NavigateHome()
        {
            Navigate(homeSceneName);
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (scene.name != _pendingScene)
                return;

            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.SetActiveScene(scene);
            _state.CompleteNavigation();
        }

        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }
}
