using System;
using DG.Tweening;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Feature.AceOfShadows.Scripts.Monobehaviour
{
    [RequireComponent(typeof(RectTransform))]
    public class CardView : MonoBehaviour
    {
        private AceOfShadowsConfig _config;
        private RectTransform _rectTransform;

        private RectTransform RectTransform => _rectTransform ??= (RectTransform)transform;

        public void Initialize(AceOfShadowsConfig config)
        {
            _config = config;
            PickRandomVisual(config);
        }

        private void PickRandomVisual(AceOfShadowsConfig config)
        {
            var randomIndex = Random.Range(0, config.CardVisuals.Count);
            var randomVisual = config.CardVisuals[randomIndex];
            var generatedVisual = Instantiate(randomVisual, RectTransform);
            var generatedRect = (RectTransform)generatedVisual.transform;
            generatedRect.anchoredPosition = Vector2.zero;
            generatedRect.localRotation = Quaternion.identity;
        }

        public void SetPositionImmediate(Vector2 anchoredPosition, Quaternion localRotation)
        {
            RectTransform.anchoredPosition = anchoredPosition;
            RectTransform.localRotation = localRotation;
        }

        public void MoveTo(Vector2 anchoredPosition, Quaternion localRotation, Action onComplete)
        {
            transform.DOKill();

            DOTween.Sequence()
                .SetTarget(transform)
                .Join(RectTransform.DOAnchorPos(anchoredPosition, _config.MoveDuration))
                .Join(transform.DOLocalRotateQuaternion(localRotation, _config.MoveDuration))
                .SetEase(_config.MoveEase)
                .OnComplete(() => onComplete?.Invoke());
        }

        public void AnimateExitDown(float distance, float duration, Ease ease, float delay, Action onComplete)
        {
            transform.DOKill();

            var targetPosition = RectTransform.anchoredPosition + new Vector2(0f, -distance);
            RectTransform.DOAnchorPos(targetPosition, duration)
                .SetTarget(transform)
                .SetDelay(delay)
                .SetEase(ease)
                .OnComplete(() => onComplete?.Invoke());
        }

        private void OnDestroy()
        {
            transform.DOKill();
        }
    }
}
