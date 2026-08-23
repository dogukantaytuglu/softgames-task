using System;
using UnityEngine;
using UnityEngine.UI;

namespace AceOfShadows.Monobehaviour
{
    public class FinishedMessageView : MonoBehaviour
    {
        [SerializeField] private GameObject root;
        [SerializeField] private Button restartButton;

        public void Initialize(Action onRestart)
        {
            root.SetActive(false);
            restartButton.onClick.AddListener(() => onRestart());
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
