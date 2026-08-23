using AceOfShadows.Logic;
using TMPro;
using UnityEngine;

namespace AceOfShadows.Monobehaviour
{
    public class StackCounterView : MonoBehaviour
    {
        [SerializeField] private TMP_Text counterText;

        private CardStack _stack;

        public void Bind(CardStack stack)
        {
            if (_stack != null)
                _stack.CountChanged -= OnCountChanged;

            _stack = stack;
            _stack.CountChanged += OnCountChanged;
            OnCountChanged(_stack);
        }

        private void OnDestroy()
        {
            if (_stack != null)
                _stack.CountChanged -= OnCountChanged;
        }

        private void OnCountChanged(CardStack stack)
        {
            counterText.text = stack.Count.ToString();
        }
    }
}
