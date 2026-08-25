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

        private static readonly int ColorIndex = Animator.StringToHash(ColorIndexParam);
        private const string ColorIndexParam = "ColorIndex";

        public void Initialize(PhoenixFlameConfig config)
        {
            var particleSystemRenderer = particle.GetComponent<ParticleSystemRenderer>();
            particleSystemRenderer.material = Instantiate(config.BaseMaterial);
            animator.runtimeAnimatorController = config.AnimatorController;
            _colorState = new PhoenixFlameColorState(config.ColorOptions.Count);
            animator.SetInteger(ColorIndex, _colorState.CurrentIndex);
        }

        public void SetColor(int index)
        {
            if (_colorState.TrySelect(index))
                animator.SetInteger(ColorIndex, index);
        }
    }
}
