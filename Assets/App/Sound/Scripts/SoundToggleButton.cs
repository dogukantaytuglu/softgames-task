using UnityEngine;
using UnityEngine.UI;

namespace Sound
{
    [RequireComponent(typeof(Button))]
    public class SoundToggleButton : MonoBehaviour
    {
        [SerializeField] private Button button;
        [SerializeField] private Image icon;
        [SerializeField] private Sprite enabledSprite;
        [SerializeField] private Sprite disabledSprite;

        private void Awake()
        {
            button.onClick.AddListener(Toggle);
            Refresh();
        }

        private void OnValidate()
        {
            if (TryGetComponent<Button>(out var b))
                button = b;
        }

        private void Toggle()
        {
            SoundService.SetEnabled(!SoundService.IsEnabled);
            Refresh();
        }

        private void Refresh()
        {
            if (icon != null)
                icon.sprite = SoundService.IsEnabled ? enabledSprite : disabledSprite;
        }
    }
}
