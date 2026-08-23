using SceneFlow.Logic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SceneFlow.Monobehaviour
{
    internal static class EditorSceneBootstrap
    {
#if UNITY_EDITOR
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void EnsureAppSceneLoaded()
        {
            if (SceneService.Instance != null)
                return;

            if (SceneManager.GetActiveScene().name == SceneNames.App)
                return;

            if (SceneManager.GetSceneByName(SceneNames.App).isLoaded)
                return;

            SceneManager.LoadScene(SceneNames.App, LoadSceneMode.Additive);
        }
#endif
    }
}
