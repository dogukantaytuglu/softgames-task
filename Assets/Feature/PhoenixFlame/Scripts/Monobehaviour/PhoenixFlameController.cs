using UnityEngine;

namespace PhoenixFlame.Monobehaviour
{
    public class PhoenixFlameController : MonoBehaviour
    {
        [SerializeField] private PhoenixFlameConfig config;
        [SerializeField] private Transform flameSpawnPoint;
        [SerializeField] private Animator fakeLightAnimator;
        [SerializeField] private PhoenixFlameColorButton[] buttons;

        private FlameParticle _flameParticle;

        private void Awake()
        {
            _flameParticle = Instantiate(config.FlamePrefab, flameSpawnPoint.position, flameSpawnPoint.rotation, flameSpawnPoint);
            _flameParticle.Initialize(config, fakeLightAnimator);

            foreach (var button in buttons)
            {
                button.Initialize(_flameParticle.SetColor);
            }
        }

        private void Update()
        {
            var interactable = !_flameParticle.IsTransitioning;
            foreach (var button in buttons)
            {
                button.SetInteractable(interactable);
            }
        }

        private void OnValidate()
        {
            buttons =  FindObjectsByType<PhoenixFlameColorButton>(FindObjectsSortMode.None);
        }
    }
}
