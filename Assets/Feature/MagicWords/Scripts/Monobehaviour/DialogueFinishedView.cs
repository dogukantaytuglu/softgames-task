using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MagicWords.Monobehaviour
{
    /// <summary>
    /// The two end states of the screen. They are separate panels rather than one panel with
    /// swapped colours because they mean opposite things: a conversation that finished is a
    /// reward moment (gold badge, gold "Play again"), a conversation that never arrived is not
    /// (muted badge, the real error text, a neutral "Try again"). Reward gold stays reserved
    /// for the win, so a failure can never be mistaken for one at a glance.
    /// </summary>
    public class DialogueFinishedView : MonoBehaviour
    {
        [SerializeField] private GameObject root;
        [SerializeField] private GameObject successPanel;
        [SerializeField] private GameObject failurePanel;
        [SerializeField] private TMP_Text lineCountText;
        [SerializeField] private TMP_Text failureReasonText;
        [SerializeField] private Button replayButton;
        [SerializeField] private Button retryButton;

        public void Initialize()
        {
            root.SetActive(false);
        }

        public void Show(int lineCount, Action onReplay)
        {
            lineCountText.text = $"{lineCount} / {lineCount}";
            Wire(replayButton, onReplay);
            successPanel.SetActive(true);
            failurePanel.SetActive(false);
            root.SetActive(true);
        }

        /// A failed fetch must not be presented as a completed conversation - showing the
        /// same "end of the conversation" panel tells the player everything worked.
        public void ShowFailure(string reason, Action onRetry)
        {
            failureReasonText.text = string.IsNullOrWhiteSpace(reason) ? "Unknown error." : reason;
            Wire(retryButton, onRetry);
            successPanel.SetActive(false);
            failurePanel.SetActive(true);
            root.SetActive(true);
        }

        public void Hide()
        {
            root.SetActive(false);
        }

        private static void Wire(Button button, Action action)
        {
            button.onClick.RemoveAllListeners();
            if (action != null)
                button.onClick.AddListener(() => action());
        }
    }
}
