using System;
using System.Collections;
using MagicWords.Logic;
using UnityEngine.Networking;

namespace MagicWords.Monobehaviour
{
    public class MagicWordsRepository
    {
        public IEnumerator Fetch(string url, Action<MagicWordsResponseDto> onSuccess, Action<string> onError)
        {
            using var request = UnityWebRequest.Get(url);
            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                onError?.Invoke(request.error);
                yield break;
            }

            var dto = MagicWordsResponseParser.Parse(request.downloadHandler.text);
            if (dto?.dialogue == null || dto.dialogue.Length == 0)
            {
                onError?.Invoke("Response parsed but contained no dialogue lines.");
                yield break;
            }

            onSuccess?.Invoke(dto);
        }
    }
}
