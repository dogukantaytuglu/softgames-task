using System;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MagicWords.Monobehaviour
{
    [RequireComponent(typeof(RectTransform))]
    public class DialogueBoxView : MonoBehaviour
    {
        [SerializeField] private TMP_Text nameText;
        [SerializeField] private TMP_Text dialogueText;
        [SerializeField] private Image avatarImage;
        [SerializeField] private Sprite fallbackAvatarSprite;

        [Header("Width is anchor-stretched (scales with canvas x-axis) - only the offscreen hide distance is computed, not a fixed pixel width")]
        [SerializeField] private float offscreenMargin = 60f;
        [Tooltip("-1 for a box anchored to the left edge, +1 for the right (mirrored) edge - which direction is offscreen for this instance.")]
        [SerializeField] private float hideDirectionSign = -1f;

        private RectTransform _rectTransform;
        private RectTransform RectTransform => _rectTransform ??= (RectTransform)transform;

        private float HiddenAnchoredX => hideDirectionSign * (RectTransform.rect.width + offscreenMargin);

        public void Initialize()
        {
            SnapHidden();
            gameObject.SetActive(false);
        }

        public void Bind(string speakerName, Sprite avatarSprite)
        {
            nameText.text = speakerName;
            avatarImage.sprite = avatarSprite != null ? avatarSprite : fallbackAvatarSprite;
        }

        public void SnapHidden()
        {
            transform.DOKill();
            RectTransform.anchoredPosition = new Vector2(HiddenAnchoredX, RectTransform.anchoredPosition.y);
        }

        public void SlideIn(float duration, Ease ease)
        {
            transform.DOKill();
            RectTransform.DOAnchorPosX(0f, duration).SetEase(ease).SetTarget(transform);
        }

        public void PlayReveal(string text, float charactersPerSecond, Action onComplete)
        {
            DOTween.Kill(dialogueText);

            dialogueText.text = text;
            dialogueText.ForceMeshUpdate();
            var totalVisibleChars = dialogueText.textInfo.characterCount;
            dialogueText.maxVisibleCharacters = 0;

            var duration = charactersPerSecond > 0f ? totalVisibleChars / charactersPerSecond : 0f;
            if (duration <= 0f)
            {
                dialogueText.maxVisibleCharacters = totalVisibleChars;
                onComplete?.Invoke();
                return;
            }

            dialogueText.DOMaxVisibleCharacters(totalVisibleChars, duration)
                .SetTarget(dialogueText)
                .OnComplete(() => onComplete?.Invoke());
        }

        public void CompleteRevealImmediately()
        {
            DOTween.Kill(dialogueText);
            dialogueText.maxVisibleCharacters = dialogueText.textInfo.characterCount;
        }

        private void OnDestroy()
        {
            transform.DOKill();
            DOTween.Kill(dialogueText);
        }
    }
}
