using PhoenixFlame.Logic;
using Sound;
using UnityEngine;

namespace PhoenixFlame.Monobehaviour
{
    public class FlameParticle : MonoBehaviour
    {
        [SerializeField] private ParticleSystem particle;
        [SerializeField] private Animator animator;
        [SerializeField] private SpriteRenderer bowlHighlightRenderer;

        private PhoenixFlameConfig _config;
        private PhoenixFlameColorState _colorState;
        private Animator _fakeLightAnimator;
        private Material _runtimeMaterial;
        private Material _highlightMaterial;

        private static readonly int ColorIndex = Animator.StringToHash(ColorIndexParam);
        private const string ColorIndexParam = "ColorIndex";

        public void Initialize(PhoenixFlameConfig config, Animator fakeLightAnimator = null)
        {
            _config = config;

            // The Animator writes colour straight onto the renderer's material, so each
            // flame needs its own copy - animating the shared asset would edit it on disk
            // in the Editor and bleed between instances at runtime. We own this copy, so
            // we're also the ones who have to destroy it (see OnDestroy) - scene unload
            // won't free it on its own.
            var particleSystemRenderer = particle.GetComponent<ParticleSystemRenderer>();
            _runtimeMaterial = Instantiate(config.BaseMaterial);
            particleSystemRenderer.material = _runtimeMaterial;

            // Same reasoning as above: the Animator also drives the bowl's fake-light
            // sprite (see path "BowlHighlight" in the color clips), so it needs its own
            // material instance too, or previewing/playing writes straight onto
            // BrazierBowlFakeLight.mat on disk.
            if (bowlHighlightRenderer != null)
            {
                _highlightMaterial = Instantiate(bowlHighlightRenderer.sharedMaterial);
                bowlHighlightRenderer.material = _highlightMaterial;
            }

            animator.runtimeAnimatorController = config.AnimatorController;
            _colorState = new PhoenixFlameColorState(config.ColorOptions.Count);
            animator.SetInteger(ColorIndex, _colorState.CurrentIndex);

            SoundService.Play(config.FireLoopSound);

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

            SoundService.Play(_config.ColorChangeSound);
        }

        private void OnDestroy()
        {
            // The pooled AudioSource playing the loop lives on SoundService's own
            // DontDestroyOnLoad object, so it outlives this scene unless stopped here.
            if (_config != null)
                SoundService.Stop(_config.FireLoopSound);

            if (_runtimeMaterial != null)
            {
                Destroy(_runtimeMaterial);
                _runtimeMaterial = null;
            }

            if (_highlightMaterial != null)
            {
                Destroy(_highlightMaterial);
                _highlightMaterial = null;
            }
        }
    }
}
