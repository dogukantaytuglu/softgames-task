using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace MagicWords.Monobehaviour
{
    /// <summary>
    /// Fetches avatar images. Any failure (missing URL, broken port, 404, timeout) resolves
    /// via onLoaded(null) rather than throwing - callers are expected to fall back to a
    /// designed placeholder, not to treat this as exceptional.
    ///
    /// Results are cached per URL, failures included. The endpoint's 17 lines are spoken by
    /// four people, so without a cache the same face is downloaded once per line and the
    /// portrait visibly falls back to the placeholder and pops again on every single line;
    /// caching the failures likewise means a URL that cannot resolve costs one request, not one
    /// per line. Instance-scoped, not static, so the cache dies with the screen that owns it.
    /// </summary>
    public class AvatarSpriteLoader
    {
        private readonly Dictionary<string, Sprite> _cache = new();

        public IEnumerator Load(string url, Action<Sprite> onLoaded)
        {
            if (string.IsNullOrEmpty(url))
            {
                onLoaded?.Invoke(null);
                yield break;
            }

            if (_cache.TryGetValue(url, out var cached))
            {
                onLoaded?.Invoke(cached);
                yield break;
            }

            using var request = UnityWebRequestTexture.GetTexture(url);
            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                _cache[url] = null;
                onLoaded?.Invoke(null);
                yield break;
            }

            var texture = DownloadHandlerTexture.GetContent(request);
            var sprite = Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height), new Vector2(0.5f, 0.5f));
            _cache[url] = sprite;
            onLoaded?.Invoke(sprite);
        }

        /// Only the retry path calls this: a retry that reused cached failures would be
        /// telling the player it tried again when it did not.
        public void ClearCache()
        {
            _cache.Clear();
        }
    }
}
