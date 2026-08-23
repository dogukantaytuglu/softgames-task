using UnityEngine;

namespace AceOfShadows.Monobehaviour
{
    public class FinishedMessageView : MonoBehaviour
    {
        [SerializeField] private GameObject root;

        private void Awake()
        {
            root.SetActive(false);
        }

        public void Show()
        {
            root.SetActive(true);
        }
    }
}
