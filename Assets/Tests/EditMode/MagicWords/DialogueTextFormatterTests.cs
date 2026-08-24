using MagicWords.Logic;
using NUnit.Framework;

namespace MagicWords.Tests
{
    public class DialogueTextFormatterTests
    {
        [Test]
        public void FormatTokens_NoTokens_ReturnsUnchanged()
        {
            var result = DialogueTextFormatter.FormatTokens("Hello there.");

            Assert.AreEqual("Hello there.", result);
        }

        [Test]
        public void FormatTokens_KnownTokenMidSentence_ReplacesWithSpriteTag()
        {
            var result = DialogueTextFormatter.FormatTokens("I admit {satisfied} the design is elegant.");

            Assert.AreEqual("I admit <sprite name=\"satisfied\"> the design is elegant.", result);
        }

        [Test]
        public void FormatTokens_KnownTokenAtStart_KeepsSpriteTag()
        {
            var result = DialogueTextFormatter.FormatTokens("{intrigued} Are you feeling okay?");

            Assert.AreEqual("<sprite name=\"intrigued\"> Are you feeling okay?", result);
        }

        [Test]
        public void FormatTokens_KnownTokenAtEnd_KeepsSpriteTag()
        {
            var result = DialogueTextFormatter.FormatTokens("That is wonderful {win}");

            Assert.AreEqual("That is wonderful <sprite name=\"win\">", result);
        }

        [Test]
        public void FormatTokens_MultipleKnownTokens_ReplacesAll()
        {
            var result = DialogueTextFormatter.FormatTokens("{neutral} Well {affirmative} that settles it {laughing}");

            Assert.AreEqual(
                "<sprite name=\"neutral\"> Well <sprite name=\"affirmative\"> that settles it <sprite name=\"laughing\">",
                result);
        }

        [Test]
        public void FormatTokens_UnknownToken_StripsAndCollapsesSpace()
        {
            var result = DialogueTextFormatter.FormatTokens("This is a {surprised} twist.");

            Assert.AreEqual("This is a twist.", result);
        }

        [Test]
        public void FormatTokens_MixOfKnownAndUnknownTokens_ReplacesKnownStripsUnknown()
        {
            var result = DialogueTextFormatter.FormatTokens("{satisfied} Well {surprised} that works.");

            Assert.AreEqual("<sprite name=\"satisfied\"> Well that works.", result);
        }

        [Test]
        public void FormatTokens_EmptyOrNull_ReturnsSameValue()
        {
            Assert.AreEqual("", DialogueTextFormatter.FormatTokens(""));
            Assert.IsNull(DialogueTextFormatter.FormatTokens(null));
        }
    }
}
