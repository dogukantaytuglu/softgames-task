using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

namespace AceOfShadows.Monobehaviour
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

        [Header("Target Stack Exit (all cards moved)")]
        [SerializeField] private float exitDistance = 1600f;
        [SerializeField] private float exitDuration = 0.5f;
        [SerializeField] private Ease exitEase = Ease.InBack;
        [SerializeField] private float exitStagger = 0.02f;

        public int TotalCards => totalCards;
        public float MoveInterval => moveInterval;
        public float MaxRotationDegrees => maxRotationDegrees;
        public float MoveDuration => moveDuration;
        public Ease MoveEase => moveEase;
        public List<GameObject> CardVisuals => cardVisuals;
        public float ExitDistance => exitDistance;
        public float ExitDuration => exitDuration;
        public Ease ExitEase => exitEase;
        public float ExitStagger => exitStagger;
    }
}
