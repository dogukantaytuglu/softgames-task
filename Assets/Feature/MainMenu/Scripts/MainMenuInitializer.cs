using UnityEngine;

namespace Feature.MainMenu.Scripts
{
    public class MainMenuInitializer : MonoBehaviour
    {
        private void Awake()
        {
            foreach (var loader in GetComponentsInChildren<MenuButtonSceneLoader>())
                loader.Initialize();
        }
    }
}
