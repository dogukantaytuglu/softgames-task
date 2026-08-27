using System;
using UnityEngine;

namespace PhoenixFlame.Monobehaviour
{
    public class PhoenixFlameController : MonoBehaviour
    {
        [SerializeField] private PhoenixFlameConfig config;
        [SerializeField] private Transform flameSpawnPoint;
        [SerializeField] private Animator fakeLightAnimator;
        [SerializeField] private PhoenixFlameColorButton[] buttons;

        private Animator _flameAnimator;

        private void Awake()
        {
            var flameParticle = Instantiate(config.FlamePrefab, flameSpawnPoint.position, flameSpawnPoint.rotation, flameSpawnPoint);
            flameParticle.Initialize(config, fakeLightAnimator);

            foreach (var button in buttons)
            {
                button.Initialize(flameParticle.SetColor);
            }
        }

        private void OnValidate()
        {
            buttons =  FindObjectsByType<PhoenixFlameColorButton>(FindObjectsSortMode.None);
        }
    }
}
