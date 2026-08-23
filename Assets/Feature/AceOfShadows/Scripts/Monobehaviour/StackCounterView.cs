using DG.Tweening;
using TMPro;
using UnityEngine;

namespace Feature.AceOfShadows.Scripts.Monobehaviour
{
    public class StackCounterView : MonoBehaviour
    {
        [SerializeField] private TMP_Text counterText;
        [SerializeField] private float popScale = 0.25f;
        [SerializeField] private float popDuration = 0.25f;

        private Vector3 _initScale;

        private void Awake()
        {
            _initScale = transform.localScale;
        }

        private void OnDestroy()
        {
            transform.DOKill();
        }

        public void SetCount(int count)
        {
            counterText.text = count.ToString();
        }

        public void Refresh(int count)
        {
            SetCount(count);
            PlayPopAnimation();
        }

        private void PlayPopAnimation()
        {
            transform.DOKill();
            transform.localScale = _initScale;
            transform.DOPunchScale(_initScale * popScale, popDuration, vibrato: 1, elasticity: 0.5f)
                .SetTarget(transform);
        }
    }
}
