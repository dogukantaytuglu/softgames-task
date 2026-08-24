using MagicWords.Logic;
using NUnit.Framework;

namespace MagicWords.Tests
{
    public class DialogueTextFormatterTests
    {
        [Test]
        public void StripTokens_NoTokens_ReturnsUnchanged()
        {
            var result = DialogueTextFormatter.StripTokens("Hello there.");

            Assert.AreEqual("Hello there.", result);
        }

        [Test]
        public void StripTokens_TokenMidSentence_RemovesTokenAndCollapsesSpace()
        {
            var result = DialogueTextFormatter.StripTokens("I admit {satisfied} the design is elegant.");

            Assert.AreEqual("I admit the design is elegant.", result);
        }

        [Test]
        public void StripTokens_TokenAtStart_TrimsLeadingSpace()
        {
            var result = DialogueTextFormatter.StripTokens("{intrigued} Are you feeling okay?");

            Assert.AreEqual("Are you feeling okay?", result);
        }

        [Test]
        public void StripTokens_TokenAtEnd_TrimsTrailingSpace()
        {
            var result = DialogueTextFormatter.StripTokens("That is wonderful {win}");

            Assert.AreEqual("That is wonderful", result);
        }

        [Test]
        public void StripTokens_MultipleTokens_RemovesAll()
        {
            var result = DialogueTextFormatter.StripTokens("{neutral} Well {affirmative} that settles it {laughing}");

            Assert.AreEqual("Well that settles it", result);
        }

        [Test]
        public void StripTokens_EmptyOrNull_ReturnsSameValue()
        {
            Assert.AreEqual("", DialogueTextFormatter.StripTokens(""));
            Assert.IsNull(DialogueTextFormatter.StripTokens(null));
        }
    }
}
