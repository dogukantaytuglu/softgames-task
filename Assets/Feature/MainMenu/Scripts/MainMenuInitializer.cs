using UnityEngine;

namespace Feature.MainMenu.Scripts
{
    public class MainMenuInitializer : MonoBehaviour
    {
        [SerializeField] private MainMenuAnimationHandler animationHandler;

        private void Awake()
        {
            foreach (var loader in GetComponentsInChildren<MenuButtonSceneLoader>())
                loader.Initialize();

            animationHandler.PlayIntro();
        }
    }
}
