using System.Collections.Generic;
using AceOfShadows.Logic;
using TimerUtil;
using UnityEngine;
using UnityEngine.UI;

namespace AceOfShadows.Monobehaviour
{
    public class AceOfShadowsController : MonoBehaviour
    {
        [SerializeField] private AceOfShadowsConfig config;
        [SerializeField] private CardView cardPrefab;
        [SerializeField] private RectTransform sourceStackRoot;
        [SerializeField] private RectTransform targetStackRoot;
        [SerializeField] private StackCounterView sourceCounterView;
        [SerializeField] private StackCounterView targetCounterView;
        [SerializeField] private FinishedMessageView finishedMessageView;
        [SerializeField] private Image countdownFill;
        [SerializeField] private FastForwardButtonView fastForwardButton;

        private const float CountdownFillStep = 0.01f;

        private CardDeck _deck;
        private readonly Dictionary<int, CardView> _cardViewsByCardId = new();
        private CountdownTimer _timer;
        private float _lastCountdownFillAmount = -1f;
        private bool _isFastForwarding;
        private float _extraTickAccumulator;

        private void Awake()
        {
            _deck = new CardDeck(config.TotalCards);
            _deck.CardMoved += OnCardMoved;
            _deck.AllAnimationsFinished += OnAllAnimationsFinished;

            CreateCardViews();

            sourceCounterView.SetCount(_deck.Source.Count);
            targetCounterView.SetCount(_deck.Target.Count);
            finishedMessageView.Initialize(Restart);

            _timer = TimerService.CreateCountdownTimer(config.MoveInterval, loopCount: -1)
                .OnTick(UpdateCountdownFill)
                .OnLoop(TryMoveNext);
            _timer.Start();

            fastForwardButton.HoldStarted += OnFastForwardHoldStarted;
            fastForwardButton.HoldEnded += OnFastForwardHoldEnded;
        }

        private void OnDisable()
        {
            _deck.CardMoved -= OnCardMoved;
            _deck.AllAnimationsFinished -= OnAllAnimationsFinished;
            _timer?.Stop();

            fastForwardButton.HoldStarted -= OnFastForwardHoldStarted;
            fastForwardButton.HoldEnded -= OnFastForwardHoldEnded;
        }

        // The countdown timer only advances on real Time.deltaTime (see TimerUtil.Timer.Tick),
        // and it's shared plumbing used elsewhere - so instead of teaching it about speed, we
        // feed it extra ticks ourselves. Each TryTick() consumes one more frame's worth of
        // countdown, so calling it (speedMultiplier - 1) extra times a frame makes the move
        // cadence land speedMultiplier-times more often. The accumulator carries the
        // fractional remainder so a non-integer multiplier still averages out correctly.
        private void Update()
        {
            if (!_isFastForwarding)
                return;

            _extraTickAccumulator += config.SpeedMultiplier - 1f;
            while (_extraTickAccumulator >= 1f)
            {
                _timer.TryTick();
                _extraTickAccumulator -= 1f;
            }
        }

        private void OnFastForwardHoldStarted()
        {
            _isFastForwarding = true;
        }

        private void OnFastForwardHoldEnded()
        {
            _isFastForwarding = false;
            _extraTickAccumulator = 0f;
        }

        private void CreateCardViews()
        {
            var sourceCards = _deck.Source.Cards;
            for (var i = 0; i < sourceCards.Count; i++)
            {
                var card = sourceCards[i];
                var view = Instantiate(cardPrefab, sourceStackRoot);
                view.name = $"Card {card.Id}";
                view.Initialize(config);

                var localPosition = CardStackLayout.GetOffset(i, config.PerCardOffset, config.MaxPileRise);
                var localRotation = CardStackLayout.GetRandomZRotation(config.MaxRotationDegrees);
                view.SetPositionImmediate(localPosition, localRotation);

                _cardViewsByCardId[card.Id] = view;
            }
        }

        private void TryMoveNext()
        {
            if (_deck.Source.Count > 0)
            {
                _deck.MoveNext();
                sourceCounterView.Refresh(_deck.Source.Count);
            }
            else
            {
                _timer.Stop();
                SetCountdownFill(0f);
            }
        }

        // OnTick fires every frame - writing Image.fillAmount unconditionally would
        // dirty this Graphic (and force a canvas batch rebuild for every sibling in
        // the same Canvas, 144 cards' worth) every single frame for the whole run.
        // Same "only touch it when the displayed value actually changes" pattern
        // FpsCountUIController already uses.
        private void UpdateCountdownFill()
        {
            var fillAmount = 1f - _timer.CountdownPercent;
            if (Mathf.Abs(fillAmount - _lastCountdownFillAmount) < CountdownFillStep)
                return;

            SetCountdownFill(fillAmount);
        }

        private void SetCountdownFill(float fillAmount)
        {
            _lastCountdownFillAmount = fillAmount;
            if (countdownFill != null)
                countdownFill.fillAmount = fillAmount;
        }

        private void OnCardMoved(CardMove move)
        {
            var view = _cardViewsByCardId[move.Card.Id];
            var distanceFromBottom = move.To.Count - 1;

            view.transform.SetParent(targetStackRoot);

            var localPosition = CardStackLayout.GetOffset(distanceFromBottom, config.PerCardOffset, config.MaxPileRise);
            var localRotation = CardStackLayout.GetRandomZRotation(config.MaxRotationDegrees);
            var duration = _isFastForwarding ? config.MoveDuration / config.SpeedMultiplier : config.MoveDuration;

            view.MoveTo(localPosition, localRotation, duration, OnCardLanded);
        }

        private void OnCardLanded()
        {
            _deck.NotifyCardLanded();
            targetCounterView.Refresh(_deck.Target.Count);
        }

        private void OnAllAnimationsFinished()
        {
            finishedMessageView.Show(_deck.Target.Count, config.TotalCards);
            sourceCounterView.Hide();
            targetCounterView.Hide();
            fastForwardButton.gameObject.SetActive(false);
            AnimateTargetStackExit();
        }

        private void AnimateTargetStackExit()
        {
            var targetCards = _deck.Target.Cards;
            for (var i = 0; i < targetCards.Count; i++)
            {
                var view = _cardViewsByCardId[targetCards[i].Id];
                var delay = i * config.ExitStagger;
                view.AnimateExitDown(config.ExitDistance, config.ExitDuration, config.ExitEase, delay, null);
            }
        }

        private void Restart()
        {
            foreach (var view in _cardViewsByCardId.Values)
                Destroy(view.gameObject);
            _cardViewsByCardId.Clear();

            _deck.CardMoved -= OnCardMoved;
            _deck.AllAnimationsFinished -= OnAllAnimationsFinished;
            _deck = new CardDeck(config.TotalCards);
            _deck.CardMoved += OnCardMoved;
            _deck.AllAnimationsFinished += OnAllAnimationsFinished;

            CreateCardViews();

            sourceCounterView.Show();
            targetCounterView.Show();
            fastForwardButton.gameObject.SetActive(true);
            sourceCounterView.SetCount(_deck.Source.Count);
            targetCounterView.SetCount(_deck.Target.Count);

            finishedMessageView.Hide();
            SetCountdownFill(0f);

            _timer.Start();
        }
    }
}
