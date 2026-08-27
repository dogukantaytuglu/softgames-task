using PhoenixFlame.Logic;
using UnityEngine;

namespace PhoenixFlame.Monobehaviour
{
    public class FlameParticle : MonoBehaviour
    {
        [SerializeField] private ParticleSystem particle;
        [SerializeField] private Animator animator;

        private PhoenixFlameColorState _colorState;
        private ParticleSystemRenderer _particleSystemRenderer;
        private Animator _fakeLightAnimator;

        private static readonly int ColorIndex = Animator.StringToHash(ColorIndexParam);
        private const string ColorIndexParam = "ColorIndex";

        public void Initialize(PhoenixFlameConfig config, Animator fakeLightAnimator = null)
        {
            var particleSystemRenderer = particle.GetComponent<ParticleSystemRenderer>();
            particleSystemRenderer.material = Instantiate(config.BaseMaterial);
            animator.runtimeAnimatorController = config.AnimatorController;
            _colorState = new PhoenixFlameColorState(config.ColorOptions.Count);
            animator.SetInteger(ColorIndex, _colorState.CurrentIndex);

            // The glow sprites live outside this prefab's hierarchy, so an Animator
            // can't reach them from here - they get their own, driven off the same
            // ColorIndex and the same clips.
            _fakeLightAnimator = fakeLightAnimator;

            if (_fakeLightAnimator == null)
                return;

            _fakeLightAnimator.runtimeAnimatorController = config.AnimatorController;
            _fakeLightAnimator.SetInteger(ColorIndex, _colorState.CurrentIndex);
        }

        public void SetColor(int index)
        {
            if (!_colorState.TrySelect(index))
                return;

            animator.SetInteger(ColorIndex, index);

            if (_fakeLightAnimator != null)
                _fakeLightAnimator.SetInteger(ColorIndex, index);
        }
    }
}
