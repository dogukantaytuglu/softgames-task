using SceneFlow.Logic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SceneFlow.Monobehaviour
{
    public class SceneFlowController : MonoBehaviour
    {
        public static SceneFlowController Instance { get; private set; }

        [SerializeField] private string homeSceneName = "MainMenu";

        private SceneFlowState _state;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            _state = new SceneFlowState();
        }

        private void Start()
        {
            Navigate(homeSceneName);
        }

        public void Navigate(string sceneName)
        {
            if (!_state.TryNavigate(sceneName, out var previousScene))
                return;

            if (!string.IsNullOrEmpty(previousScene))
                SceneManager.UnloadSceneAsync(previousScene);

            var loadOperation = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
            loadOperation.completed += _ => SceneManager.SetActiveScene(SceneManager.GetSceneByName(sceneName));
        }

        public void NavigateHome()
        {
            Navigate(homeSceneName);
        }
    }
}
