using System.Text;

namespace MagicWords.Logic
{
    // Strips {word} emoji tokens embedded in the endpoint's dialogue text (e.g.
    // "{satisfied}") - the endpoint uses named tokens, not real Unicode emoji
    // codepoints. The natural next step is mapping known tokens to a
    // TextMeshPro <sprite> tag backed by a matching TMP Sprite Asset; no such
    // asset exists yet, so tokens are stripped cleanly rather than left as
    // literal "{word}" text or rendered as broken/empty glyphs.
    public static class DialogueTextFormatter
    {
        public static string StripTokens(string rawText)
        {
            if (string.IsNullOrEmpty(rawText))
                return rawText;

            var builder = new StringBuilder(rawText.Length);
            var insideToken = false;

            foreach (var c in rawText)
            {
                if (c == '{')
                {
                    insideToken = true;
                    continue;
                }

                if (c == '}')
                {
                    insideToken = false;
                    continue;
                }

                if (!insideToken)
                    builder.Append(c);
            }

            return CollapseSpaces(builder.ToString());
        }

        private static string CollapseSpaces(string text)
        {
            var builder = new StringBuilder(text.Length);
            var lastWasSpace = false;

            foreach (var c in text.Trim())
            {
                var isSpace = c == ' ';
                if (isSpace && lastWasSpace)
                    continue;

                builder.Append(c);
                lastWasSpace = isSpace;
            }

            return builder.ToString();
        }
    }
}
