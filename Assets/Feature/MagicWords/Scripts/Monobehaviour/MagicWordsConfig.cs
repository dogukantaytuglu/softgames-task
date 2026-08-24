using DG.Tweening;
using UnityEngine;

namespace MagicWords.Monobehaviour
{
    [CreateAssetMenu(fileName = "MagicWordsConfig", menuName = "MagicWords/Config")]
    public class MagicWordsConfig : ScriptableObject
    {
        [SerializeField] private string endpointUrl = "https://private-624120-softgamesassignment.apiary-mock.com/v3/magicwords";
        [SerializeField] private float charactersPerSecond = 45f;
        [SerializeField] private float autoAdvanceDelay = 2.5f;
        [SerializeField] private float boxMoveDuration = 0.4f;
        [SerializeField] private Ease boxMoveEase = Ease.OutBack;

        public string EndpointUrl => endpointUrl;
        public float CharactersPerSecond => charactersPerSecond;
        public float AutoAdvanceDelay => autoAdvanceDelay;
        public float BoxMoveDuration => boxMoveDuration;
        public Ease BoxMoveEase => boxMoveEase;
    }
}
