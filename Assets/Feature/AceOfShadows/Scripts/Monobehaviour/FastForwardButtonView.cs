using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;

namespace AceOfShadows.Monobehaviour
{
    // A press-and-hold control, not a click - the fast-forward effect only applies while
    // the pointer stays down, so this reads raw down/up events instead of using Button.onClick.
    [RequireComponent(typeof(RectTransform))]
    public class FastForwardButtonView : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        [SerializeField] private float pressedScale = 0.9f;
        [SerializeField] private float pressDuration = 0.1f;

        public event Action HoldStarted;
        public event Action HoldEnded;

        private Vector3 _initScale;
        private bool _isHeld;

        private void Awake()
        {
            _initScale = transform.localScale;
        }

        // Covers the button being hidden (finished screen, restart) while still held -
        // without this the accelerated timer would keep running with nothing to release it.
        // Also the general DOTween teardown: OnDisable always runs before OnDestroy, so this
        // still catches the destroy path - Release() only kills a tween if one was left running.
        private void OnDisable()
        {
            Release();
            transform.DOKill();
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            _isHeld = true;
            transform.DOKill();
            transform.DOScale(_initScale * pressedScale, pressDuration).SetTarget(transform);
            HoldStarted?.Invoke();
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            Release();
        }

        private void Release()
        {
            if (!_isHeld)
                return;

            _isHeld = false;
            transform.DOKill();
            transform.DOScale(_initScale, pressDuration).SetTarget(transform);
            HoldEnded?.Invoke();
        }
    }
}
