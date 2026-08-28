using System.Collections.Generic;
using Sound;
using UnityEngine;

namespace PhoenixFlame.Monobehaviour
{
    [CreateAssetMenu(fileName = "PhoenixFlameConfig", menuName = "PhoenixFlame/Config")]
    public class PhoenixFlameConfig : ScriptableObject
    {
        [SerializeField] private FlameParticle flamePrefab;
        [SerializeField] private Material baseMaterial;
        [SerializeField] private RuntimeAnimatorController animatorController;

        // Auto-derived from animatorController by a custom editor - do not hand-edit.
        [SerializeField] private List<PhoenixFlameColorOption> colorOptions;

        [SerializeField] private SoundConfig fireLoopSound;
        [SerializeField] private SoundConfig colorChangeSound;

        public FlameParticle FlamePrefab => flamePrefab;
        public Material BaseMaterial => baseMaterial;
        public RuntimeAnimatorController AnimatorController => animatorController;
        public IReadOnlyList<PhoenixFlameColorOption> ColorOptions => colorOptions;
        public SoundConfig FireLoopSound => fireLoopSound;
        public SoundConfig ColorChangeSound => colorChangeSound;
    }
}
