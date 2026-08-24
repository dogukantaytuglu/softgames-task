using MagicWords.Logic;
using NUnit.Framework;

namespace MagicWords.Tests
{
    public class DialogueSequenceBuilderTests
    {
        [Test]
        public void Build_ResolvesPositionFromMatchingAvatar()
        {
            var response = new MagicWordsResponseDto
            {
                dialogue = new[] { new DialogueLineDto { name = "Sheldon", text = "Hi" } },
                avatars = new[] { new AvatarDto { name = "Sheldon", url = "u", position = "left" } }
            };

            var sequence = DialogueSequenceBuilder.Build(response);

            Assert.AreEqual(1, sequence.Count);
            Assert.AreEqual(DialoguePosition.Left, sequence.MoveNext().Position);
        }

        [Test]
        public void Build_NoMatchingAvatar_DefaultsToRight()
        {
            var response = new MagicWordsResponseDto
            {
                dialogue = new[] { new DialogueLineDto { name = "Unknown", text = "Hi" } },
                avatars = new AvatarDto[0]
            };

            var sequence = DialogueSequenceBuilder.Build(response);

            Assert.AreEqual(DialoguePosition.Right, sequence.MoveNext().Position);
        }

        [Test]
        public void Build_DuplicateAvatarNames_FirstOneWins()
        {
            var response = new MagicWordsResponseDto
            {
                dialogue = new[] { new DialogueLineDto { name = "Sheldon", text = "Hi" } },
                avatars = new[]
                {
                    new AvatarDto { name = "Sheldon", url = "https://good", position = "left" },
                    new AvatarDto { name = "Sheldon", url = "https://bad:81", position = "right" }
                }
            };

            var line = DialogueSequenceBuilder.Build(response).MoveNext();

            Assert.AreEqual("https://good", line.AvatarUrl);
            Assert.AreEqual(DialoguePosition.Left, line.Position);
        }

        [Test]
        public void Build_UnusedAvatarEntry_DoesNotProduceExtraDialogueLine()
        {
            var response = new MagicWordsResponseDto
            {
                dialogue = new[] { new DialogueLineDto { name = "Sheldon", text = "Hi" } },
                avatars = new[]
                {
                    new AvatarDto { name = "Sheldon", url = "u", position = "left" },
                    new AvatarDto { name = "Nobody", url = "broken", position = "right" }
                }
            };

            var sequence = DialogueSequenceBuilder.Build(response);

            Assert.AreEqual(1, sequence.Count);
        }

        [Test]
        public void Build_StripsEmojiTokensFromDisplayText()
        {
            var response = new MagicWordsResponseDto
            {
                dialogue = new[] { new DialogueLineDto { name = "Sheldon", text = "I admit {satisfied} it works." } },
                avatars = new AvatarDto[0]
            };

            var line = DialogueSequenceBuilder.Build(response).MoveNext();

            Assert.AreEqual("I admit it works.", line.DisplayText);
        }

        [Test]
        public void Build_LineMissingName_IsSkipped()
        {
            var response = new MagicWordsResponseDto
            {
                dialogue = new[]
                {
                    new DialogueLineDto { name = "", text = "Hi" },
                    new DialogueLineDto { name = "Sheldon", text = "Hi" }
                },
                avatars = new AvatarDto[0]
            };

            var sequence = DialogueSequenceBuilder.Build(response);

            Assert.AreEqual(1, sequence.Count);
        }

        [Test]
        public void Build_NullResponse_ReturnsEmptySequence()
        {
            var sequence = DialogueSequenceBuilder.Build(null);

            Assert.AreEqual(0, sequence.Count);
        }
    }
}
