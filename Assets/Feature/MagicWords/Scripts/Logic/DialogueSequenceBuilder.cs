using System.Collections.Generic;

namespace MagicWords.Logic
{
    public static class DialogueSequenceBuilder
    {
        public static DialogueSequence Build(MagicWordsResponseDto response)
        {
            var lines = new List<DialogueLine>();
            var dtoLines = response?.dialogue;

            if (dtoLines != null)
            {
                foreach (var dtoLine in dtoLines)
                {
                    if (dtoLine == null || string.IsNullOrEmpty(dtoLine.name) || dtoLine.text == null)
                        continue;

                    var avatar = SpeakerAvatarLookup.FindBySpeakerName(response.avatars, dtoLine.name);
                    var position = ParsePosition(avatar?.position);
                    var displayText = DialogueTextFormatter.FormatTokens(dtoLine.text);

                    lines.Add(new DialogueLine(dtoLine.name, displayText, avatar?.url, position));
                }
            }

            return new DialogueSequence(lines);
        }

        // No avatar entry for a speaker, or an unrecognized position string,
        // both default to Right rather than throwing - the brief explicitly
        // calls out that avatar data may be missing.
        private static DialoguePosition ParsePosition(string rawPosition)
        {
            return rawPosition == "left" ? DialoguePosition.Left : DialoguePosition.Right;
        }
    }
}
