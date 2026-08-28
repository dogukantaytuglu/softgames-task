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

            var particleSystemRenderer = particle.GetComponent<ParticleSystemRenderer>();
            _runtimeMaterial = Instantiate(config.BaseMaterial);
            particleSystemRenderer.material = _runtimeMaterial;

            if (bowlHighlightRenderer != null)
            {
                _highlightMaterial = Instantiate(bowlHighlightRenderer.sharedMaterial);
                bowlHighlightRenderer.material = _highlightMaterial;
            }

            animator.runtimeAnimatorController = config.AnimatorController;
            _colorState = new PhoenixFlameColorState(config.ColorOptions.Count);
            animator.SetInteger(ColorIndex, _colorState.CurrentIndex);

            SoundService.Play(config.FireLoopSound);
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
