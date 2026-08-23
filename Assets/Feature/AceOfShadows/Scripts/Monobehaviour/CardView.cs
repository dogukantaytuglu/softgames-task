using System;
using DG.Tweening;
using UnityEngine;

namespace AceOfShadows.Monobehaviour
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class CardView : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private float moveDuration = 0.35f;
        [SerializeField] private Ease moveEase = Ease.OutQuad;

        private void OnValidate()
        {
            if (spriteRenderer == null)
                TryGetComponent(out spriteRenderer);
        }

        public void SetPositionImmediate(Vector3 localPosition, Quaternion localRotation, int sortingOrder)
        {
            transform.localPosition = localPosition;
            transform.localRotation = localRotation;
            spriteRenderer.sortingOrder = sortingOrder;
        }

        public void MoveTo(Vector3 localPosition, Quaternion localRotation, int sortingOrder, Action onComplete)
        {
            spriteRenderer.sortingOrder = sortingOrder;
            transform.DOKill();

            DOTween.Sequence()
                .Join(transform.DOLocalMove(localPosition, moveDuration).SetEase(moveEase))
                .Join(transform.DOLocalRotateQuaternion(localRotation, moveDuration).SetEase(moveEase))
                .OnComplete(() => onComplete?.Invoke());
        }
    }
}
