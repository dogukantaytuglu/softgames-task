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

            // Start position of the step that was added last: the anchor a Join attaches to.
            // DOTween keeps the same value internally (Sequence.lastTweenInsertTime) but does
            // not expose it, so we mirror it here.
            var lastStepStart = 0f;

            foreach (var step in steps)
            {
                var tween = BuildTween(step);
                if (tween == null)
                    continue;

                switch (step.sequenceMode)
                {
                    case MainMenuSequenceMode.Append:
                    {
                        var startTime = _sequence.Duration(false) + Mathf.Max(0f, step.delay);
                        if (step.delay > 0f)
                            _sequence.AppendInterval(step.delay);
                        _sequence.Append(tween);
                        lastStepStart = startTime;
                        break;
                    }
                    case MainMenuSequenceMode.Join:
                    {
                        // AppendInterval + Join cannot express "join, but delay seconds later":
                        // AppendInterval moves DOTween's join anchor to the sequence's current
                        // end and Join lands exactly there, so the delay never reaches the
                        // joined tween - it only pads the sequence's total length. Inserting at
                        // the computed absolute position is the only way to offset a join.
                        var startTime = Mathf.Max(0f, lastStepStart + step.delay);
                        _sequence.Insert(startTime, tween);
                        lastStepStart = startTime;
                        break;
                    }
                    default:
                        _sequence.Insert(step.delay, tween);
                        lastStepStart = step.delay;
                        break;
                }
            }
        }

        public void SkipIntro()
        {
            _sequence?.Complete();
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
