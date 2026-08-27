using MagicWords.Monobehaviour;
using NUnit.Framework;

namespace MagicWords.Tests
{
    /// The brief calls out "handle cases where avatar URLs may not load or data is
    /// missing" as an explicit requirement, so the malformed-payload path is a feature
    /// here, not an edge case. These tests pin the one contract the rest of the screen
    /// depends on: Parse never throws, and returns null for anything unusable.
    public class MagicWordsResponseParserTests
    {
        private const string ValidPayload =
            "{\"dialogue\":[{\"name\":\"Sheldon\",\"text\":\"Hello {satisfied}\"}]," +
            "\"avatars\":[{\"name\":\"Sheldon\",\"url\":\"http://example.com/a.png\",\"position\":\"left\"}]}";

        [Test]
        public void Parse_ValidPayload_ReturnsDialogueAndAvatars()
        {
            var result = MagicWordsResponseParser.Parse(ValidPayload);

            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.dialogue.Length);
            Assert.AreEqual(1, result.avatars.Length);
        }

        [Test]
        public void Parse_ValidPayload_MapsDialogueFieldsByJsonKey()
        {
            var result = MagicWordsResponseParser.Parse(ValidPayload);

            Assert.AreEqual("Sheldon", result.dialogue[0].name);
            Assert.AreEqual("Hello {satisfied}", result.dialogue[0].text);
        }

        [Test]
        public void Parse_ValidPayload_MapsAvatarFieldsByJsonKey()
        {
            var result = MagicWordsResponseParser.Parse(ValidPayload);

            Assert.AreEqual("Sheldon", result.avatars[0].name);
            Assert.AreEqual("http://example.com/a.png", result.avatars[0].url);
            Assert.AreEqual("left", result.avatars[0].position);
        }

        [Test]
        public void Parse_Null_ReturnsNull()
        {
            Assert.IsNull(MagicWordsResponseParser.Parse(null));
        }

        [TestCase("")]
        [TestCase("   ")]
        [TestCase("\n\t ")]
        public void Parse_EmptyOrWhitespace_ReturnsNull(string json)
        {
            Assert.IsNull(MagicWordsResponseParser.Parse(json));
        }

        /// The realistic production failure: an HTTP 200 that carries a captive-portal
        /// login page or a proxy error page instead of the API response.
        [Test]
        public void Parse_HtmlErrorPage_ReturnsNullWithoutThrowing()
        {
            var html = "<!DOCTYPE html><html><body><h1>502 Bad Gateway</h1></body></html>";

            Assert.IsNull(MagicWordsResponseParser.Parse(html));
        }

        [Test]
        public void Parse_TruncatedJson_ReturnsNullWithoutThrowing()
        {
            var truncated = "{\"dialogue\":[{\"name\":\"Sheldon\",\"te";

            Assert.IsNull(MagicWordsResponseParser.Parse(truncated));
        }

        [Test]
        public void Parse_JsonArrayAtRoot_ReturnsNullWithoutThrowing()
        {
            Assert.IsNull(MagicWordsResponseParser.Parse("[{\"name\":\"Sheldon\"}]"));
        }

        [Test]
        public void Parse_PlainText_ReturnsNullWithoutThrowing()
        {
            Assert.IsNull(MagicWordsResponseParser.Parse("Service Unavailable"));
        }

        /// Documents a real JsonUtility behaviour the caller has to compensate for:
        /// well-formed JSON of the wrong shape parses "successfully" into a DTO whose
        /// arrays are null. Parse deliberately does not treat that as a failure - it is
        /// valid JSON - which is exactly why MagicWordsRepository checks the dialogue
        /// array separately before reporting success.
        [Test]
        public void Parse_ValidJsonOfWrongShape_ReturnsDtoWithNullArrays()
        {
            var result = MagicWordsResponseParser.Parse("{\"unrelated\":42}");

            Assert.IsNotNull(result);
            Assert.IsNull(result.dialogue);
            Assert.IsNull(result.avatars);
        }

        [Test]
        public void Parse_EmptyDialogueArray_ReturnsDtoWithEmptyArray()
        {
            var result = MagicWordsResponseParser.Parse("{\"dialogue\":[],\"avatars\":[]}");

            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.dialogue.Length);
        }

        /// The endpoint ships duplicate speaker entries and an avatar whose URL points at
        /// an API root rather than an image. Neither is the parser's problem to solve,
        /// but it must not choke on them - SpeakerAvatarLookup and AvatarSpriteLoader
        /// handle them downstream.
        [Test]
        public void Parse_DuplicateSpeakersAndOddUrls_ParsesEveryEntry()
        {
            var payload =
                "{\"dialogue\":[{\"name\":\"Nobody\",\"text\":\"Hi\"}]," +
                "\"avatars\":[{\"name\":\"Sheldon\",\"url\":\"http://x:81/a.png\",\"position\":\"left\"}," +
                "{\"name\":\"Sheldon\",\"url\":\"http://y/b.png\",\"position\":\"right\"}]}";

            var result = MagicWordsResponseParser.Parse(payload);

            Assert.AreEqual(2, result.avatars.Length);
        }

        [Test]
        public void Parse_MissingAvatarsKey_ReturnsDialogueWithNullAvatars()
        {
            var result = MagicWordsResponseParser.Parse("{\"dialogue\":[{\"name\":\"A\",\"text\":\"B\"}]}");

            Assert.AreEqual(1, result.dialogue.Length);
            Assert.IsNull(result.avatars);
        }
    }
}
