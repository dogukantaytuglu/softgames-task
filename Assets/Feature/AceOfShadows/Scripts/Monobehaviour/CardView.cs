using System;
using DG.Tweening;
using UnityEngine;

namespace AceOfShadows.Monobehaviour
{
    public class CardView : MonoBehaviour
    {
        [SerializeField] private float moveDuration = 0.35f;
        [SerializeField] private Ease moveEase = Ease.OutQuad;

        public void SetPositionImmediate(Vector3 localPosition, Quaternion localRotation)
        {
            transform.localPosition = localPosition;
            transform.localRotation = localRotation;
        }

        public void MoveTo(Vector3 localPosition, Quaternion localRotation, Action onComplete)
        {
            transform.DOKill();

            DOTween.Sequence()
                .Join(transform.DOLocalMove(localPosition, moveDuration).SetEase(moveEase))
                .Join(transform.DOLocalRotateQuaternion(localRotation, moveDuration).SetEase(moveEase))
                .OnComplete(() => onComplete?.Invoke());
        }
    }
}
