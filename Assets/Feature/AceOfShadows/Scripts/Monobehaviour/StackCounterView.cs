using DG.Tweening;
using TMPro;
using UnityEngine;

namespace AceOfShadows.Monobehaviour
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

        private void OnDisable()
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

        // Toggles the whole counter, not just its label: the counter is a pill
        // (background + number + caps label) now, so hiding only the number would
        // leave an empty pill floating over the cleared table.
        public void Hide()
        {
            transform.DOKill();
            gameObject.SetActive(false);
        }

        public void Show()
        {
            gameObject.SetActive(true);
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
