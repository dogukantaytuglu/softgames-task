using System.Collections.Generic;
using UnityEngine;

namespace PhoenixFlame.Monobehaviour
{
    [CreateAssetMenu(fileName = "PhoenixFlameConfig", menuName = "PhoenixFlame/Config")]
    public class PhoenixFlameConfig : ScriptableObject
    {
        [SerializeField] private FlameParticle flamePrefab;
        [SerializeField] private Material baseMaterial;
        [SerializeField] private List<PhoenixFlameColorOption> colorOptions;

        [SerializeField] private float colorTransitionDuration = 1.5f;

        public FlameParticle FlamePrefab => flamePrefab;
        public Material BaseMaterial => baseMaterial;
        public IReadOnlyList<PhoenixFlameColorOption> ColorOptions => colorOptions;
        public float ColorTransitionDuration => colorTransitionDuration;
    }
}
