using UnityEngine;

namespace MagicWords.Monobehaviour
{
    public class DialogueFinishedView : MonoBehaviour
    {
        [SerializeField] private GameObject root;

        public void Initialize()
        {
            root.SetActive(false);
        }

        public void Show()
        {
            root.SetActive(true);
        }

        public void Hide()
        {
            root.SetActive(false);
        }
    }
}
