using TMPro;
using UnityEngine;

namespace MagicWords.Monobehaviour
{
    public class DialogueFinishedView : MonoBehaviour
    {
        [SerializeField] private GameObject root;
        [SerializeField] private TMP_Text message;

        private const string FinishedMessage = "That's the end of the conversation.";

        public void Initialize()
        {
            root.SetActive(false);
        }

        public void Show()
        {
            SetMessage(FinishedMessage);
            root.SetActive(true);
        }

        /// A failed fetch must not be presented as a completed conversation - showing the
        /// same "end of the conversation" panel tells the player everything worked.
        public void ShowFailure(string reason)
        {
            SetMessage(string.IsNullOrWhiteSpace(reason)
                ? "The conversation could not be loaded."
                : $"The conversation could not be loaded.\n{reason}");
            root.SetActive(true);
        }

        public void Hide()
        {
            root.SetActive(false);
        }

        private void SetMessage(string text)
        {
            if (message != null)
                message.text = text;
        }
    }
}
