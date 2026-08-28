using UnityEngine;
using UnityEngine.UI;

namespace Feature.MainMenu.Scripts
{
    public class MainMenuInitializer : MonoBehaviour
    {
        [SerializeField] private MainMenuAnimationHandler animationHandler;
        [SerializeField] private Button skipIntroButton;

        private void Awake()
        {
            foreach (var loader in GetComponentsInChildren<MenuButtonSceneLoader>())
                loader.Initialize();

            skipIntroButton.onClick.AddListener(animationHandler.SkipIntro);

            animationHandler.PlayIntro();
        }
    }
}
