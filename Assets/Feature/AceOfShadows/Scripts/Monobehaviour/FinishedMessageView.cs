using UnityEngine;

namespace AceOfShadows.Monobehaviour
{
    public class FinishedMessageView : MonoBehaviour
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
    }
}
