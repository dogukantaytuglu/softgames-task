using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AceOfShadows.Monobehaviour
{
    public class FinishedMessageView : MonoBehaviour
    {
        [SerializeField] private GameObject root;
        [SerializeField] private Button restartButton;
        [SerializeField] private TMP_Text countText;

        // Slot outlines and the countdown track: table furniture that only makes sense
        // while cards are being dealt. Left up, it shows as empty dashed rectangles
        // poking out from behind the completion panel and its button.
        [SerializeField] private GameObject tableDressing;

        public void Initialize(Action onRestart)
        {
            root.SetActive(false);
            SetTableDressingActive(true);
            restartButton.onClick.AddListener(() => onRestart());
        }

        // The "144 / 144" line is filled in from the real deck rather than typed
        // into the scene, so it can't quietly lie if totalCards is ever retuned.
        public void Show(int movedCards, int totalCards)
        {
            if (countText != null)
                countText.text = $"{movedCards} / {totalCards}";

            SetTableDressingActive(false);
            root.SetActive(true);
        }

        public void Hide()
        {
            root.SetActive(false);
            SetTableDressingActive(true);
        }

        private void SetTableDressingActive(bool isActive)
        {
            if (tableDressing != null)
                tableDressing.SetActive(isActive);
        }
    }
}
