using UnityEngine;
using UnityEngine.UI;

namespace Sound
{
    [RequireComponent(typeof(Button))]
    public class ButtonClickSound : MonoBehaviour
    {
        [SerializeField] private bool isActive = true;
        [SerializeField] private Button button;
        [SerializeField] private SoundConfig sound;

        private void Awake()
        {
            button.onClick.AddListener(PlaySound);
        }

        private void OnValidate()
        {
            if (TryGetComponent<Button>(out var b))
                button = b;
        }

        private void PlaySound()
        {
            if (!isActive) return;
            SoundService.Play(sound);
        }
    }
}
