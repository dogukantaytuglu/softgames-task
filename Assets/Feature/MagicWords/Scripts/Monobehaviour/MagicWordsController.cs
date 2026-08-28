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
        [SerializeField] private SpeakerPortraitView leftPortrait;
        [SerializeField] private SpeakerPortraitView rightPortrait;
        [SerializeField] private Button advanceButton;
        [SerializeField] private DialogueFinishedView finishedView;
        [SerializeField] private DialogueProgressView progressView;

        private readonly MagicWordsRepository _repository = new();
        private readonly AvatarSpriteLoader _avatarLoader = new();
        private MagicWordsResponseDto _response;
        private DialogueSequence _sequence;
        private DialogueBoxView _activeBox;
        private CountdownTimer _autoAdvanceTimer;
        private bool _isRevealing;

        // Avatars arrive asynchronously; spamming the advance button can land an old
        // avatar on the line that is showing now. Every line takes a token and a late
        // callback for a superseded token is dropped.
        private int _lineToken;

        private void Awake()
        {
            Debug.Log($"[MagicWords] Awake - charactersPerSecond={config.CharactersPerSecond}, "
                + $"autoAdvanceDelay={config.AutoAdvanceDelay}, boxMoveDuration={config.BoxMoveDuration}");

            leftBox.Initialize();
            rightBox.Initialize();
            leftPortrait.Initialize();
            rightPortrait.Initialize();
            finishedView.Initialize();
            progressView.Initialize();
            advanceButton.onClick.AddListener(OnAdvanceClicked);

            _autoAdvanceTimer = TimerService.CreateCountdownTimer(config.AutoAdvanceDelay, loopCount: 1)
                .OnComplete(NextDialogue);

            Fetch();
        }

        private void OnDisable()
        {
            _autoAdvanceTimer?.Stop();
            advanceButton.onClick.RemoveListener(OnAdvanceClicked);
        }

        private void Fetch()
        {
            Debug.Log($"[MagicWords] Fetch - url={config.EndpointUrl}, timeout={config.RequestTimeoutSeconds}s");
            StartCoroutine(_repository.Fetch(config.EndpointUrl, config.RequestTimeoutSeconds, OnDialogueLoaded, OnDialogueLoadFailed));
        }

        private void OnDialogueLoaded(MagicWordsResponseDto dto)
        {
            Debug.Log($"[MagicWords] OnDialogueLoaded - dto.dialogue count={dto?.dialogue?.Length ?? -1}, "
                + $"dto.avatars count={dto?.avatars?.Length ?? -1}");

            _response = dto;
            _sequence = DialogueSequenceBuilder.Build(dto);

            Debug.Log($"[MagicWords] Built sequence - {_sequence.Count} usable line(s) "
                + $"(dropped {(dto?.dialogue?.Length ?? 0) - _sequence.Count} for missing name/text)");

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
            finishedView.ShowFailure(error, Retry);
        }

        private void OnAdvanceClicked()
        {
            Debug.Log($"[MagicWords] OnAdvanceClicked - sequence={(_sequence == null ? "null" : "set")}, "
                + $"isRevealing={_isRevealing}");

            if (_sequence == null)
                return;

            if (_isRevealing)
            {
                Debug.Log("[MagicWords] Advance while revealing - completing reveal immediately instead of skipping to next line");
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

            Debug.Log($"[MagicWords] NextDialogue - HasStarted={_sequence.HasStarted}, "
                + $"CurrentNumber={_sequence.CurrentNumber}, Count={_sequence.Count}, "
                + $"IsFinished={_sequence.IsFinished}");

            if (_sequence.IsFinished)
            {
                Debug.Log("[MagicWords] Sequence finished - ending conversation");
                EndDialogue();
                return;
            }

            ShowLine(_sequence.MoveNext());
        }

        private void ShowLine(DialogueLine line)
        {
            var speaksOnLeft = line.Position == DialoguePosition.Left;
            var box = speaksOnLeft ? leftBox : rightBox;
            var otherBox = speaksOnLeft ? rightBox : leftBox;
            var portrait = speaksOnLeft ? leftPortrait : rightPortrait;
            var otherPortrait = speaksOnLeft ? rightPortrait : leftPortrait;

            Debug.Log($"[MagicWords] ShowLine #{_sequence.CurrentNumber}/{_sequence.Count} - "
                + $"speaker='{line.SpeakerName}', position={line.Position}, box={box.gameObject.name}, "
                + $"textLength={line.DisplayText?.Length ?? -1}, avatarUrl='{line.AvatarUrl}', "
                + $"text preview='{(line.DisplayText?.Length > 40 ? line.DisplayText.Substring(0, 40) + "..." : line.DisplayText)}'");

            otherBox.SnapHidden();
            otherBox.gameObject.SetActive(false);

            progressView.SetProgress(_sequence.CurrentNumber, _sequence.Count);

            _activeBox = box;
            advanceButton.interactable = true;
            box.gameObject.SetActive(true);
            box.Bind(line.SpeakerName);
            box.SlideIn(config.BoxMoveDuration, config.BoxMoveEase);

            portrait.SetSpeaking(true);
            otherPortrait.SetSpeaking(false);

            _isRevealing = true;
            Debug.Log($"[MagicWords] ShowLine #{_sequence.CurrentNumber} - starting reveal, "
                + $"box.activeInHierarchy={box.gameObject.activeInHierarchy}, "
                + $"box.activeSelf={box.gameObject.activeSelf}");
            box.PlayReveal(line.DisplayText, config.CharactersPerSecond, OnRevealComplete);

            var token = ++_lineToken;
            Debug.Log($"[MagicWords] ShowLine #{_sequence.CurrentNumber} - avatar load token={token}");
            StartCoroutine(_avatarLoader.Load(line.AvatarUrl, sprite =>
            {
                if (token != _lineToken)
                {
                    Debug.Log($"[MagicWords] Avatar callback for token={token} dropped - "
                        + $"current token is now {_lineToken} (line advanced before avatar loaded)");
                    return;
                }

                Debug.Log($"[MagicWords] Avatar callback for token={token} applied - sprite={(sprite == null ? "null (fallback)" : sprite.name)}");
                box.Bind(line.SpeakerName);
                portrait.Bind(line.SpeakerName, sprite);
            }));
        }

        private void OnRevealComplete()
        {
            Debug.Log($"[MagicWords] OnRevealComplete #{_sequence.CurrentNumber} - starting auto-advance timer "
                + $"({config.AutoAdvanceDelay}s)");
            _isRevealing = false;
            _autoAdvanceTimer.Start();
        }

        private void EndDialogue()
        {
            Debug.Log("[MagicWords] EndDialogue");
            HideConversation();
            finishedView.Show(_sequence.Count, Replay);
        }

        /// Replays the conversation already in hand - the endpoint is not asked again, because
        /// nothing about it has changed and the player asked for the conversation, not a refetch.
        private void Replay()
        {
            Debug.Log("[MagicWords] Replay");
            finishedView.Hide();
            _sequence = DialogueSequenceBuilder.Build(_response);
            NextDialogue();
        }

        /// The failure path, which does have to go back to the network.
        private void Retry()
        {
            finishedView.Hide();
            _avatarLoader.ClearCache();
            Fetch();
        }

        private void HideConversation()
        {
            leftBox.gameObject.SetActive(false);
            rightBox.gameObject.SetActive(false);
            leftPortrait.Hide();
            rightPortrait.Hide();
            progressView.Hide();

            // The advance button covers the whole screen and sits under the end panel. Left live
            // it would keep re-triggering the ending behind the panel on every stray tap.
            advanceButton.interactable = false;
        }
    }
}
