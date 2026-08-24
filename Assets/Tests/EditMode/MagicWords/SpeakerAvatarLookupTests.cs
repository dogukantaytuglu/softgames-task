using System.Collections.Generic;
using MagicWords.Logic;
using NUnit.Framework;

namespace MagicWords.Tests
{
    public class SpeakerAvatarLookupTests
    {
        [Test]
        public void FindBySpeakerName_NullList_ReturnsNull()
        {
            var result = SpeakerAvatarLookup.FindBySpeakerName(null, "Sheldon");

            Assert.IsNull(result);
        }

        [Test]
        public void FindBySpeakerName_NoMatch_ReturnsNull()
        {
            var avatars = new List<AvatarDto> { new() { name = "Leonard", url = "u", position = "right" } };

            var result = SpeakerAvatarLookup.FindBySpeakerName(avatars, "Sheldon");

            Assert.IsNull(result);
        }

        [Test]
        public void FindBySpeakerName_DuplicateNames_FirstMatchWins()
        {
            var working = new AvatarDto { name = "Sheldon", url = "https://good", position = "left" };
            var broken = new AvatarDto { name = "Sheldon", url = "https://bad:81", position = "right" };
            var avatars = new List<AvatarDto> { working, broken };

            var result = SpeakerAvatarLookup.FindBySpeakerName(avatars, "Sheldon");

            Assert.AreSame(working, result);
        }
    }
}
