using DG.Tweening;
using UnityEngine;

namespace MagicWords.Monobehaviour
{
    [CreateAssetMenu(fileName = "MagicWordsConfig", menuName = "MagicWords/Config")]
    public class MagicWordsConfig : ScriptableObject
    {
        [SerializeField] private string endpointUrl = "https://private-624120-softgamesassignment.apiary-mock.com/v3/magicwords";
        [Tooltip("Seconds before an unanswered request is abandoned. UnityWebRequest has no "
                 + "timeout by default, so without this a stalled request hangs the screen forever.")]
        [SerializeField] private int requestTimeoutSeconds = 10;
        [SerializeField] private float duration = 2f;
        [SerializeField] private float autoAdvanceDelay = 2.5f;
        [SerializeField] private float boxMoveDuration = 0.4f;
        [SerializeField] private Ease boxMoveEase = Ease.OutBack;

        public string EndpointUrl => endpointUrl;
        public int RequestTimeoutSeconds => requestTimeoutSeconds;
        public float Duration => duration;
        public float AutoAdvanceDelay => autoAdvanceDelay;
        public float BoxMoveDuration => boxMoveDuration;
        public Ease BoxMoveEase => boxMoveEase;
    }
}
