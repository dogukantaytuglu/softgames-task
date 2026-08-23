using SceneFlow.Logic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SceneFlow.Monobehaviour
{
    internal static class EditorSceneBootstrap
    {
#if UNITY_EDITOR
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void EnsureShellLoaded()
        {
            if (SceneFlowController.Instance != null)
                return;

            if (SceneManager.GetActiveScene().name == SceneNames.Shell)
                return;

            if (SceneManager.GetSceneByName(SceneNames.Shell).isLoaded)
                return;

            SceneManager.LoadScene(SceneNames.Shell, LoadSceneMode.Additive);
        }
#endif
    }
}
