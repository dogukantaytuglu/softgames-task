using DG.Tweening;
using TMPro;
using UnityEngine;

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

        /// <summary>
        /// Grown by moving its right anchor, not by <see cref="UnityEngine.UI.Image.fillAmount"/>:
        /// the fill is a 9-sliced capsule, so resizing the rect keeps its rounded caps intact,
        /// while a Filled image would ignore the sprite border and stretch the caps into a taper.
        /// </summary>
        [SerializeField] private RectTransform fillRect;

        [Tooltip("A single line advances the bar by one seventeenth of a 324px track - about "
                 + "19px - so this is a short travel, not a long sweep. Kept under the 0.4s box "
                 + "slide-in it runs alongside: the bar is the secondary read-out of the two and "
                 + "should have settled by the time the box lands.")]
        [SerializeField] private float fillDuration = 0.3f;
        [Tooltip("Overshoots the new width by about a tenth of the step and springs back, which "
                 + "reads as the bar being pushed forward rather than wiped forward.")]
        [SerializeField] private Ease fillEase = Ease.OutBack;

        public void Initialize()
        {
            SetFill(0f);
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

            // Advancing is one tap, and nothing gates the next tap on this tween finishing, so
            // a player tapping through faster than 0.3s a line can start a second advance mid-fill.
            // The newest target wins outright rather than two tweens writing the same anchor.
            fillRect.DOKill();

            var progress = Mathf.Clamp01(currentLine / (float)totalLines);
            var from = fillRect.anchorMax.x;
            DOTween.To(() => from, SetFill, progress, fillDuration)
                .SetEase(fillEase)
                .SetTarget(fillRect);
        }

        public void Hide()
        {
            fillRect.DOKill();

            // Emptied while hidden, not on the way back in: a replay hands back line 1 of n, and
            // draining the full bar down to it in view would read as losing progress.
            SetFill(0f);
            root.SetActive(false);
        }

        private void OnDisable()
        {
            fillRect.DOKill();
        }

        /// <summary>
        /// Clamped here rather than left to the ease. <see cref="Ease.OutBack"/> overshoots past
        /// its target, and on the last line that would carry anchorMax.x past 1 and push the
        /// fill's rounded cap out through the end of the track it is supposed to sit inside -
        /// which is also why the tween runs through this setter rather than as a plain
        /// DOAnchorMax, which has nowhere to put the clamp.
        /// </summary>
        private void SetFill(float progress)
        {
            fillRect.anchorMax = new Vector2(Mathf.Clamp01(progress), fillRect.anchorMax.y);
        }
    }
}
