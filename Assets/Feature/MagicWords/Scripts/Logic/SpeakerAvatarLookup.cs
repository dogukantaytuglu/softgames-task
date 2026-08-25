using System.Collections.Generic;

namespace MagicWords.Logic
{
    public static class SpeakerAvatarLookup
    {
        // First match wins - the real endpoint has duplicate speaker names, which
        // would throw if this were a Dictionary keyed by name instead.
        public static AvatarDto FindBySpeakerName(IReadOnlyList<AvatarDto> avatars, string speakerName)
        {
            if (avatars == null)
                return null;

            for (var i = 0; i < avatars.Count; i++)
            {
                if (avatars[i] != null && avatars[i].name == speakerName)
                    return avatars[i];
            }

            return null;
        }
    }
}
