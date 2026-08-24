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

        [Header("Anchored X - hidden (off to this box's own screen edge) vs shown")]
        [SerializeField] private float hiddenAnchoredX;
        [SerializeField] private float shownAnchoredX;

        private RectTransform _rectTransform;
        private RectTransform RectTransform => _rectTransform ??= (RectTransform)transform;

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
            RectTransform.anchoredPosition = new Vector2(hiddenAnchoredX, RectTransform.anchoredPosition.y);
        }

        public void SlideIn(float duration, Ease ease)
        {
            transform.DOKill();
            RectTransform.DOAnchorPosX(shownAnchoredX, duration).SetEase(ease).SetTarget(transform);
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
