using System;
using UnityEngine;
using UnityEngine.UI;

namespace PhoenixFlame.Monobehaviour
{
    [RequireComponent(typeof(Button))]
    public class PhoenixFlameColorButton : MonoBehaviour
    {
        [SerializeField] private int colorIndex;
        [SerializeField] private Button button;

        private Action<int> _onSelected;

        private void OnValidate()
        {
            if (TryGetComponent<Button>(out var b))
                button = b;
        }

        public void Initialize(Action<int> onSelected)
        {
            _onSelected = onSelected;
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => _onSelected?.Invoke(colorIndex));
        }
    }
}
