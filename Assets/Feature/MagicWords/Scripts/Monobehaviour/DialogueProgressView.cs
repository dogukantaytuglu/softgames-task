using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MagicWords.Monobehaviour
{
    /// <summary>
    /// The "LINE 3 OF 17" header: tells the player the conversation has an end, and how
    /// far off it is. Purely a read-out - it owns no state, the sequence does.
    /// </summary>
    public class DialogueProgressView : MonoBehaviour
    {
        [SerializeField] private GameObject root;
        [SerializeField] private TMP_Text label;
        [SerializeField] private Image fill;

        public void Initialize()
        {
            root.SetActive(false);
        }

        public void SetProgress(int currentLine, int totalLines)
        {
            if (totalLines <= 0)
            {
                Hide();
                return;
            }

            root.SetActive(true);
            label.text = $"LINE {currentLine} OF {totalLines}";
            fill.fillAmount = Mathf.Clamp01(currentLine / (float)totalLines);
        }

        public void Hide()
        {
            root.SetActive(false);
        }
    }
}
