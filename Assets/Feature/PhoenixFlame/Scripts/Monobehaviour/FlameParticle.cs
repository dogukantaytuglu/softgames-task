using PhoenixFlame.Logic;
using UnityEngine;

namespace PhoenixFlame.Monobehaviour
{
    public class FlameParticle : MonoBehaviour
    {
        [SerializeField] private ParticleSystem particle;
        [SerializeField] private Animator animator;

        private PhoenixFlameColorState _colorState;
        private Animator _fakeLightAnimator;
        private Material _runtimeMaterial;

        private static readonly int ColorIndex = Animator.StringToHash(ColorIndexParam);
        private const string ColorIndexParam = "ColorIndex";

        public void Initialize(PhoenixFlameConfig config, Animator fakeLightAnimator = null)
        {
            // The Animator writes colour straight onto the renderer's material, so each
            // flame needs its own copy - animating the shared asset would edit it on disk
            // in the Editor and bleed between instances at runtime. We own this copy, so
            // we're also the ones who have to destroy it (see OnDestroy) - scene unload
            // won't free it on its own.
            var particleSystemRenderer = particle.GetComponent<ParticleSystemRenderer>();
            _runtimeMaterial = Instantiate(config.BaseMaterial);
            particleSystemRenderer.material = _runtimeMaterial;
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

        private void OnDestroy()
        {
            if (_runtimeMaterial == null)
                return;

            Destroy(_runtimeMaterial);
            _runtimeMaterial = null;
        }
    }
}
