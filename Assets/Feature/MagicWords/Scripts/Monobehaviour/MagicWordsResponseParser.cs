using MagicWords.Logic;
using UnityEngine;

namespace MagicWords.Monobehaviour
{
    public static class MagicWordsResponseParser
    {
        public static MagicWordsResponseDto Parse(string json)
        {
            return JsonUtility.FromJson<MagicWordsResponseDto>(json);
        }
    }
}
