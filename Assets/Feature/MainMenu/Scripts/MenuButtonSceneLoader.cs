using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuButtonSceneLoader : MonoBehaviour
{
    [SerializeField] private string sceneName;

    public void LoadScene()
    {
        SceneManager.LoadScene(sceneName);
    }
}
