using UnityEngine;

namespace Sound
{
    public static class SoundService
    {
        private const string EnabledPrefKey = "Sound.Enabled";
        private const int PoolSize = 8;
        private static AudioSource[] _pool;
        private static SoundConfig[] _activeConfig;
        private static bool? _isEnabled;

        public static bool IsEnabled
        {
            get
            {
                if (_isEnabled == null)
                {
                    _isEnabled = PlayerPrefs.GetInt(EnabledPrefKey, 1) == 1;
                    AudioListener.volume = _isEnabled.Value ? 1f : 0f;
                }

                return _isEnabled.Value;
            }
        }

        public static void SetEnabled(bool enabled)
        {
            _isEnabled = enabled;
            AudioListener.volume = enabled ? 1f : 0f;
            PlayerPrefs.SetInt(EnabledPrefKey, enabled ? 1 : 0);
            PlayerPrefs.Save();
        }

        public static void Play(SoundConfig config)
        {
            if (config == null || config.Clip == null)
            {
                Debug.LogWarning("SoundService.Play called with no SoundConfig/clip assigned.");
                return;
            }

            EnsurePool();
            var index = GetFreeSourceIndex();
            var source = _pool[index];
            source.clip = config.Clip;
            source.volume = config.Volume;
            source.pitch = config.Pitch;
            source.loop = config.Loop;
            source.priority = config.Priority;
            source.Play();
            _activeConfig[index] = config;
        }

        public static void Stop(SoundConfig config)
        {
            if (_pool == null) return;

            for (var i = 0; i < _pool.Length; i++)
                if (_activeConfig[i] == config && _pool[i].isPlaying)
                    _pool[i].Stop();
        }

        private static void EnsurePool()
        {
            if (_pool != null) return;

            var host = new GameObject("SoundService");
            Object.DontDestroyOnLoad(host);

            _pool = new AudioSource[PoolSize];
            _activeConfig = new SoundConfig[PoolSize];
            for (var i = 0; i < PoolSize; i++)
            {
                _pool[i] = host.AddComponent<AudioSource>();
                _pool[i].playOnAwake = false;
            }
        }

        private static int GetFreeSourceIndex()
        {
            for (var i = 0; i < _pool.Length; i++)
                if (!_pool[i].isPlaying)
                    return i;

            return 0;
        }
    }
}
