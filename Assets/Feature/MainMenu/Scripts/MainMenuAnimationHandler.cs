using System;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;

namespace Feature.MainMenu.Scripts
{
    public enum MainMenuAnimationType
    {
        ScalePopIn,
        TextReveal
    }

    public enum MainMenuSequenceMode
    {
        Insert, // absolute position from the sequence's start; delay is that position
        Append, // starts once every step already in the sequence has finished; delay is an extra gap on top
        Join    // starts alongside the previous step; delay is an extra offset on top
    }

    [Serializable]
    public class MainMenuAnimationStep
    {
        public MainMenuAnimationType type;
        public MainMenuSequenceMode sequenceMode = MainMenuSequenceMode.Insert;
        public Transform scaleTarget;
        public TMP_Text textTarget;
        public float delay;
        public float duration = 0.35f;
        public Ease ease = Ease.OutBounce;
    }

    public class MainMenuAnimationHandler : MonoBehaviour
    {
        [SerializeField] private List<MainMenuAnimationStep> steps;

        private Sequence _sequence;

        public void PlayIntro()
        {
            _sequence?.Kill();
            _sequence = DOTween.Sequence().SetTarget(this);

            foreach (var step in steps)
            {
                var tween = BuildTween(step);
                if (tween == null)
                    continue;

                switch (step.sequenceMode)
                {
                    case MainMenuSequenceMode.Append:
                        if (step.delay > 0f)
                            _sequence.AppendInterval(step.delay);
                        _sequence.Append(tween);
                        break;
                    case MainMenuSequenceMode.Join:
                        if (step.delay > 0f)
                            _sequence.AppendInterval(step.delay);
                        _sequence.Join(tween);
                        break;
                    default:
                        _sequence.Insert(step.delay, tween);
                        break;
                }
            }
        }

        private static Tween BuildTween(MainMenuAnimationStep step)
        {
            switch (step.type)
            {
                case MainMenuAnimationType.ScalePopIn:
                    return step.scaleTarget.DOScale(Vector3.zero, step.duration).From().SetEase(step.ease);
                case MainMenuAnimationType.TextReveal:
                    return step.textTarget.DOText(string.Empty, step.duration, true).From().SetEase(step.ease);
                default:
                    return null;
            }
        }

        private void OnDestroy()
        {
            _sequence?.Kill();
        }
    }
}
