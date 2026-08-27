using MagicWords.Logic;
using TimerUtil;
using UnityEngine;
using UnityEngine.UI;

namespace MagicWords.Monobehaviour
{
    public class MagicWordsController : MonoBehaviour
    {
        [SerializeField] private MagicWordsConfig config;
        [SerializeField] private DialogueBoxView leftBox;
        [SerializeField] private DialogueBoxView rightBox;
        [SerializeField] private Button advanceButton;
        [SerializeField] private DialogueFinishedView finishedView;
        [SerializeField] private DialogueProgressView progressView;

        private readonly MagicWordsRepository _repository = new();
        private DialogueSequence _sequence;
        private DialogueBoxView _activeBox;
        private CountdownTimer _autoAdvanceTimer;
        private bool _isRevealing;

        private void Awake()
        {
            leftBox.Initialize();
            rightBox.Initialize();
            finishedView.Initialize();
            progressView.Initialize();
            advanceButton.onClick.AddListener(OnAdvanceClicked);

            _autoAdvanceTimer = TimerService.CreateCountdownTimer(config.AutoAdvanceDelay, loopCount: 1)
                .OnComplete(NextDialogue);

            StartCoroutine(_repository.Fetch(config.EndpointUrl, config.RequestTimeoutSeconds, OnDialogueLoaded, OnDialogueLoadFailed));
        }

        private void OnDestroy()
        {
            _autoAdvanceTimer?.Stop();
            advanceButton.onClick.RemoveListener(OnAdvanceClicked);
        }

        private void OnDialogueLoaded(MagicWordsResponseDto dto)
        {
            _sequence = DialogueSequenceBuilder.Build(dto);
            if (_sequence.Count == 0)
            {
                OnDialogueLoadFailed("Dialogue response contained no usable lines.");
                return;
            }

            NextDialogue();
        }

        private void OnDialogueLoadFailed(string error)
        {
            Debug.LogWarning($"MagicWords: failed to load dialogue - {error}");
            finishedView.ShowFailure(error);
        }

        private void OnAdvanceClicked()
        {
            if (_sequence == null)
                return;

            if (_isRevealing)
            {
                _activeBox.CompleteRevealImmediately();
                OnRevealComplete();
                return;
            }

            _autoAdvanceTimer.Stop();
            NextDialogue();
        }

        private void NextDialogue()
        {
            _autoAdvanceTimer.Stop();

            if (_sequence.IsFinished)
            {
                EndDialogue();
                return;
            }

            ShowLine(_sequence.MoveNext());
        }

        private void ShowLine(DialogueLine line)
        {
            var box = line.Position == DialoguePosition.Left ? leftBox : rightBox;
            var otherBox = box == leftBox ? rightBox : leftBox;

            otherBox.SnapHidden();
            otherBox.gameObject.SetActive(false);

            progressView.SetProgress(_sequence.CurrentNumber, _sequence.Count);

            _activeBox = box;
            box.gameObject.SetActive(true);
            box.Bind(line.SpeakerName, null);
            box.SlideIn(config.BoxMoveDuration, config.BoxMoveEase);

            _isRevealing = true;
            box.PlayReveal(line.DisplayText, config.CharactersPerSecond, OnRevealComplete);

            StartCoroutine(AvatarSpriteLoader.Load(line.AvatarUrl, sprite => box.Bind(line.SpeakerName, sprite)));
        }

        private void OnRevealComplete()
        {
            _isRevealing = false;
            _autoAdvanceTimer.Start();
        }

        private void EndDialogue()
        {
            leftBox.gameObject.SetActive(false);
            rightBox.gameObject.SetActive(false);
            progressView.Hide();
            finishedView.Show();
        }
    }
}
