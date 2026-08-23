using System.Collections.Generic;
using AceOfShadows.Logic;
using TimerUtil;
using UnityEngine;
using UnityEngine.UI;

namespace Feature.AceOfShadows.Scripts.Monobehaviour
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

        private CardDeck _deck;
        private readonly Dictionary<int, CardView> _cardViewsByCardId = new();
        private CountdownTimer _timer;

        private void Awake()
        {
            _deck = new CardDeck(config.TotalCards);
            _deck.CardMoved += OnCardMoved;
            _deck.AllAnimationsFinished += OnAllAnimationsFinished;

            CreateCardViews();

            sourceCounterView.SetCount(_deck.Source.Count);
            targetCounterView.SetCount(_deck.Target.Count);
            finishedMessageView.Initialize();

            _timer = TimerService.CreateCountdownTimer(config.MoveInterval, loopCount: -1)
                .OnTick(UpdateCountdownFill)
                .OnLoop(TryMoveNext);
            _timer.Start();
        }

        private void OnDestroy()
        {
            _deck.CardMoved -= OnCardMoved;
            _deck.AllAnimationsFinished -= OnAllAnimationsFinished;
            _timer?.Stop();
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

                var localPosition = CardStackLayout.GetOffset(i);
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
                if (countdownFill != null)
                    countdownFill.fillAmount = 0f;
            }
        }

        private void UpdateCountdownFill()
        {
            if (countdownFill == null)
                return;

            countdownFill.fillAmount = 1f - _timer.CountdownPercent;
        }

        private void OnCardMoved(CardMove move)
        {
            var view = _cardViewsByCardId[move.Card.Id];
            var distanceFromBottom = move.To.Count - 1;

            view.transform.SetParent(targetStackRoot);

            var localPosition = CardStackLayout.GetOffset(distanceFromBottom);
            var localRotation = CardStackLayout.GetRandomZRotation(config.MaxRotationDegrees);

            view.MoveTo(localPosition, localRotation, OnCardLanded);
        }

        private void OnCardLanded()
        {
            _deck.NotifyCardLanded();
            targetCounterView.Refresh(_deck.Target.Count);
        }

        private void OnAllAnimationsFinished()
        {
            finishedMessageView.Show();
        }
    }
}
