using System;
using DG.Tweening;
using TMPro;
using UnityEngine;

namespace MagicWords.Monobehaviour
{
    [RequireComponent(typeof(RectTransform))]
    public class DialogueBoxView : MonoBehaviour
    {
        [SerializeField] private TMP_Text nameText;
        [SerializeField] private TMP_Text dialogueText;

        [Header("Width is anchor-stretched (scales with canvas x-axis) - only the offscreen hide distance is computed, not a fixed pixel width")]
        [SerializeField] private float offscreenMargin = 60f;
        [Tooltip("-1 for a box anchored to the left edge, +1 for the right (mirrored) edge - which direction is offscreen for this instance.")]
        [SerializeField] private float hideDirectionSign = -1f;

        public void Initialize()
        {
            SnapHidden();
            gameObject.SetActive(false);
        }

        public void Bind(string speakerName)
        {
            nameText.text = speakerName;
        }

        public void SnapHidden()
        {
            transform.localScale = Vector3.zero;
        }

        public void SlideIn(float duration, Ease ease)
        {
            Debug.Log($"[MagicWords] {gameObject.name}.SlideIn - duration={duration}, ease={ease}, "
                + $"localScale before={transform.localScale}");
            transform.DOScale(Vector3.one, duration).SetEase(ease).SetTarget(transform)
                .OnComplete(() => Debug.Log($"[MagicWords] {gameObject.name}.SlideIn complete - localScale={transform.localScale}"));
        }

        public void PlayReveal(string text, float charactersPerSecond, Action onComplete)
        {
            DOTween.Kill(dialogueText);

            dialogueText.text = text;
            dialogueText.ForceMeshUpdate();
            var totalVisibleChars = dialogueText.textInfo.characterCount;
            dialogueText.maxVisibleCharacters = 0;

            var duration = charactersPerSecond > 0f ? totalVisibleChars / charactersPerSecond : 0f;
            Debug.Log($"[MagicWords] {gameObject.name}.PlayReveal - textLength={text?.Length ?? -1}, "
                + $"totalVisibleChars={totalVisibleChars}, charactersPerSecond={charactersPerSecond}, "
                + $"computedDuration={duration}, dialogueText.enabled={dialogueText.enabled}, "
                + $"dialogueText.gameObject.activeInHierarchy={dialogueText.gameObject.activeInHierarchy}, "
                + $"rectWidth={((RectTransform)dialogueText.transform).rect.width}");

            if (duration <= 0f)
            {
                Debug.Log($"[MagicWords] {gameObject.name}.PlayReveal - duration<=0, snapping to full "
                    + $"({totalVisibleChars} chars) and completing immediately");
                dialogueText.maxVisibleCharacters = totalVisibleChars;
                onComplete?.Invoke();
                return;
            }

            dialogueText.DOMaxVisibleCharacters(totalVisibleChars, duration)
                .SetTarget(dialogueText)
                .OnComplete(() =>
                {
                    Debug.Log($"[MagicWords] {gameObject.name}.PlayReveal tween OnComplete - "
                        + $"maxVisibleCharacters={dialogueText.maxVisibleCharacters}");
                    onComplete?.Invoke();
                });
        }

        public void CompleteRevealImmediately()
        {
            Debug.Log($"[MagicWords] {gameObject.name}.CompleteRevealImmediately");
            DOTween.Kill(dialogueText);
            dialogueText.maxVisibleCharacters = dialogueText.textInfo.characterCount;
        }

        private void OnDisable()
        {
            Debug.Log($"[MagicWords] {gameObject.name}.OnDisable - killing tweens");
            transform.DOKill();
            DOTween.Kill(dialogueText);
        }
    }
}
