using System.Collections.Generic;

namespace MagicWords.Logic
{
    public static class SpeakerAvatarLookup
    {
        // First match wins when a speaker name appears more than once in the
        // avatars list - the real endpoint data does this deliberately (two
        // "Sheldon" entries, one with a broken URL). A plain Dictionary keyed by
        // name would throw on that; this tolerates it instead.
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
