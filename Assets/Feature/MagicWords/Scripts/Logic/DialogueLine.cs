namespace MagicWords.Logic
{
    public sealed class DialogueLine
    {
        public string SpeakerName { get; }
        public string DisplayText { get; }
        public string AvatarUrl { get; }
        public DialoguePosition Position { get; }

        public DialogueLine(string speakerName, string displayText, string avatarUrl, DialoguePosition position)
        {
            SpeakerName = speakerName;
            DisplayText = displayText;
            AvatarUrl = avatarUrl;
            Position = position;
        }
    }
}
