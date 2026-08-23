using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

namespace Feature.AceOfShadows.Scripts.Monobehaviour
{
    [CreateAssetMenu(fileName = "AceOfShadowsConfig", menuName = "AceOfShadows/Config")]
    public class AceOfShadowsConfig : ScriptableObject
    {
        [SerializeField] private int totalCards = 144;
        [SerializeField] private float moveInterval = 1f;
        [SerializeField] private float maxRotationDegrees = 6f;
        [SerializeField] private float moveDuration = 0.35f;
        [SerializeField] private Ease moveEase = Ease.OutQuad;
        [SerializeField] private List<GameObject> cardVisuals;

        public int TotalCards => totalCards;
        public float MoveInterval => moveInterval;
        public float MaxRotationDegrees => maxRotationDegrees;
        public float MoveDuration => moveDuration;
        public Ease MoveEase => moveEase;
        public List<GameObject> CardVisuals => cardVisuals;
    }
}
