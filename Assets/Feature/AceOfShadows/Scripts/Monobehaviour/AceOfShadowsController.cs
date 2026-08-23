using System.Collections.Generic;
using AceOfShadows.Logic;
using TimerUtil;
using UnityEngine;
using UnityEngine.UI;

namespace AceOfShadows.Monobehaviour
{
    public class AceOfShadowsController : MonoBehaviour
    {
        [SerializeField] private int totalCards = 144;
        [SerializeField] private float moveInterval = 1f;
        [SerializeField] private float maxRotationDegrees = 6f;
        [SerializeField] private CardView cardPrefab;
        [SerializeField] private Transform sourceStackRoot;
        [SerializeField] private Transform targetStackRoot;
        [SerializeField] private StackCounterView sourceCounterView;
        [SerializeField] private StackCounterView targetCounterView;
        [SerializeField] private FinishedMessageView finishedMessageView;
        [SerializeField] private Image countdownFill;

        private CardDeck _deck;
        private readonly Dictionary<int, CardView> _cardViewsByCardId = new();
        private CountdownTimer _timer;

        private void Awake()
        {
            _deck = new CardDeck(totalCards);
            _deck.CardMoved += OnCardMoved;
            _deck.AllAnimationsFinished += OnAllAnimationsFinished;

            CreateCardViews();

            sourceCounterView.Bind(_deck.Source);
            targetCounterView.Bind(_deck.Target);
            finishedMessageView.Initialize();

            _timer = TimerService.CreateCountdownTimer(moveInterval, loopCount: -1)
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

                var localPosition = CardStackLayout.GetOffset(i);
                var localRotation = CardStackLayout.GetRandomZRotation(maxRotationDegrees);
                view.SetPositionImmediate(localPosition, localRotation);

                _cardViewsByCardId[card.Id] = view;
            }
        }

        private void TryMoveNext()
        {
            if (_deck.Source.Count > 0)
            {
                _deck.MoveNext();
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
            var localRotation = CardStackLayout.GetRandomZRotation(maxRotationDegrees, ySeed: 180);

            view.MoveTo(localPosition, localRotation, _deck.NotifyCardLanded);
        }

        private void OnAllAnimationsFinished()
        {
            finishedMessageView.Show();
        }
    }
}
