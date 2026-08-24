using System;

namespace MagicWords.Logic
{
    [Serializable]
    public class MagicWordsResponseDto
    {
        public DialogueLineDto[] dialogue;
        public AvatarDto[] avatars;
    }
}
