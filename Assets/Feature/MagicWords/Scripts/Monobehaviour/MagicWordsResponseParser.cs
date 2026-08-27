using System;
using MagicWords.Logic;
using UnityEngine;

namespace MagicWords.Monobehaviour
{
    public static class MagicWordsResponseParser
    {
        /// Returns null when the payload is not usable, rather than throwing.
        /// JsonUtility.FromJson throws on anything that is not valid JSON - an HTTP 200
        /// carrying a captive-portal login page or a proxy error page is the realistic
        /// case - and an exception thrown here kills the calling coroutine silently,
        /// leaving the screen blank forever with no failure state. The brief names a
        /// malformed-payload path as a requirement, so this has to degrade, not die.
        public static MagicWordsResponseDto Parse(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
                return null;

            try
            {
                return JsonUtility.FromJson<MagicWordsResponseDto>(json);
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"MagicWords: response was not valid JSON - {exception.Message}");
                return null;
            }
        }
    }
}
