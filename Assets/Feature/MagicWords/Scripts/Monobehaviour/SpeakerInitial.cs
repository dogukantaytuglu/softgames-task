namespace MagicWords.Monobehaviour
{
    /// The letter shown on a speaker's placeholder avatar. Shared by the large portrait and the
    /// chip inside the dialogue box so the same speaker never gets two different placeholders.
    public static class SpeakerInitial
    {
        public static string Of(string speakerName)
        {
            if (string.IsNullOrWhiteSpace(speakerName))
                return "?";

            var trimmed = speakerName.Trim();
            return char.IsLetterOrDigit(trimmed[0]) ? trimmed.Substring(0, 1).ToUpperInvariant() : "?";
        }
    }
}
