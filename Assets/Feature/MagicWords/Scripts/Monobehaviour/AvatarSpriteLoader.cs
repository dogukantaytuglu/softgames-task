using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

namespace MagicWords.Monobehaviour
{
    // Any failure (missing URL, broken port, 404, timeout) resolves via
    // onLoaded(null) rather than throwing - callers are expected to fall back
    // to a placeholder sprite, not treat this as exceptional.
    public static class AvatarSpriteLoader
    {
        public static IEnumerator Load(string url, Action<Sprite> onLoaded)
        {
            if (string.IsNullOrEmpty(url))
            {
                onLoaded?.Invoke(null);
                yield break;
            }

            using var request = UnityWebRequestTexture.GetTexture(url);
            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                onLoaded?.Invoke(null);
                yield break;
            }

            var texture = DownloadHandlerTexture.GetContent(request);
            var sprite = Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height), new Vector2(0.5f, 0.5f));
            onLoaded?.Invoke(sprite);
        }
    }
}
