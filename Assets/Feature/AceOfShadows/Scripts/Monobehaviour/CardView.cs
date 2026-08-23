using System;
using DG.Tweening;
using UnityEngine;

namespace AceOfShadows.Monobehaviour
{
    public class CardView : MonoBehaviour
    {
        private AceOfShadowsConfig _config;

        public void Initialize(AceOfShadowsConfig config)
        {
            _config = config;
        }

        public void SetPositionImmediate(Vector3 localPosition, Quaternion localRotation)
        {
            transform.localPosition = localPosition;
            transform.localRotation = localRotation;
        }

        public void MoveTo(Vector3 localPosition, Quaternion localRotation, Action onComplete)
        {
            transform.DOKill();

            DOTween.Sequence()
                .SetTarget(transform)
                .Join(transform.DOLocalMove(localPosition, _config.MoveDuration).SetEase(_config.MoveEase))
                .Join(transform.DOLocalRotateQuaternion(localRotation, _config.MoveDuration).SetEase(_config.MoveEase))
                .OnComplete(() => onComplete?.Invoke());
        }

        private void OnDestroy()
        {
            transform.DOKill();
        }
    }
}
