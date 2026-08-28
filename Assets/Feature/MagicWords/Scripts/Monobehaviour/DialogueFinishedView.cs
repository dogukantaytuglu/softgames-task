using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
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

        private UnityAction _replayListener;
        private UnityAction _retryListener;

        public void Initialize()
        {
            root.SetActive(false);
        }

        public void Show(int lineCount, Action onReplay)
        {
            lineCountText.text = $"{lineCount} / {lineCount}";
            Wire(replayButton, ref _replayListener, onReplay);
            successPanel.SetActive(true);
            failurePanel.SetActive(false);
            root.SetActive(true);
        }

        /// A failed fetch must not be presented as a completed conversation - showing the
        /// same "end of the conversation" panel tells the player everything worked.
        public void ShowFailure(string reason, Action onRetry)
        {
            failureReasonText.text = string.IsNullOrWhiteSpace(reason) ? "Unknown error." : reason;
            Wire(retryButton, ref _retryListener, onRetry);
            successPanel.SetActive(false);
            failurePanel.SetActive(true);
            root.SetActive(true);
        }

        public void Hide()
        {
            root.SetActive(false);
        }

        // Removes only the listener this method previously added (never ButtonClickSound's,
        // which self-wires additively on the same Button) so repeated calls - e.g. a second
        // failed fetch re-wiring retryButton - don't stack duplicate callbacks either.
        private static void Wire(Button button, ref UnityAction cached, Action action)
        {
            if (cached != null)
                button.onClick.RemoveListener(cached);

            cached = action != null ? () => action() : null;

            if (cached != null)
                button.onClick.AddListener(cached);
        }
    }
}
