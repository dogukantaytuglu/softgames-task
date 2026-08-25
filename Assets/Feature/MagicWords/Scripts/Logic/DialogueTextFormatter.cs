using System.Collections.Generic;
using System.Text;

namespace MagicWords.Logic
{
    // The endpoint's {word} tokens are names, not Unicode emoji codepoints, so
    // this maps against an explicit known-token table rather than a codepoint
    // lookup; any unrecognized token is stripped rather than left as literal
    // text or rendered as a broken sprite.
    public static class DialogueTextFormatter
    {
        private static readonly Dictionary<string, string> KnownTokenSprites = new Dictionary<string, string>
        {
            { "affirmative", "affirmative" },
            { "intrigued", "intrigued" },
            { "laughing", "laughing" },
            { "neutral", "neutral" },
            { "satisfied", "satisfied" },
            { "win", "win" },
        };

        public static string FormatTokens(string rawText)
        {
            if (string.IsNullOrEmpty(rawText))
                return rawText;

            var builder = new StringBuilder(rawText.Length);
            var tokenBuilder = new StringBuilder();
            var insideToken = false;

            foreach (var c in rawText)
            {
                if (c == '{')
                {
                    insideToken = true;
                    tokenBuilder.Clear();
                    continue;
                }

                if (c == '}')
                {
                    insideToken = false;
                    if (KnownTokenSprites.TryGetValue(tokenBuilder.ToString(), out var spriteName))
                        builder.Append("<sprite name=\"").Append(spriteName).Append("\">");
                    continue;
                }

                if (insideToken)
                    tokenBuilder.Append(c);
                else
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
