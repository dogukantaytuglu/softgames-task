using System.Collections.Generic;
using DG.Tweening;
using Sound;
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
        [SerializeField] private SoundConfig moveSound;

        [Header("Stack Layout")]
        [Tooltip("Vertical fan per card, in canvas pixels at the 1080x1920 reference. " +
                 "Small on purpose: at 2px a full 144-card deck stands 288px tall next to a " +
                 "375px card, so the two piles are never the same height at any point in the " +
                 "run and pile height works as an analog read-out of the counter beside it. " +
                 "An earlier 3px fan capped at 12 cards made both piles bottom out at the same " +
                 "36px silhouette for 118 of the demo's 144 seconds.")]
        [SerializeField] private float perCardOffset = 2f;

        [Tooltip("Safety valve, not the normal case. A 144-card deck rises 288px and never " +
                 "reaches this; it only binds if totalCards is raised well past 144, and stops " +
                 "the pile growing into the counter pills or off the top of a short screen. " +
                 "Expressed as a pixel height rather than a card count so it stays meaningful " +
                 "if perCardOffset is retuned.")]
        [SerializeField] private float maxPileRise = 340f;

        [Header("Target Stack Exit (all cards moved)")]
        [SerializeField] private float exitDistance = 1600f;
        [SerializeField] private float exitDuration = 0.5f;
        [SerializeField] private Ease exitEase = Ease.InBack;
        [SerializeField] private float exitStagger = 0.02f;

        [Header("Fast Forward")]
        [Tooltip("How much faster the deal runs while the fast-forward button is held - both " +
                 "the move cadence and each card's own slide are divided by this, so the whole " +
                 "sequence compresses instead of just piling up more overlapping card tweens.")]
        [SerializeField] private float speedMultiplier = 3f;

        public int TotalCards => totalCards;
        public float MoveInterval => moveInterval;
        public float MaxRotationDegrees => maxRotationDegrees;
        public float MoveDuration => moveDuration;
        public Ease MoveEase => moveEase;
        public List<GameObject> CardVisuals => cardVisuals;
        public SoundConfig MoveSound => moveSound;
        public float PerCardOffset => perCardOffset;
        public float MaxPileRise => maxPileRise;
        public float ExitDistance => exitDistance;
        public float ExitDuration => exitDuration;
        public Ease ExitEase => exitEase;
        public float ExitStagger => exitStagger;
        public float SpeedMultiplier => speedMultiplier;
    }
}
