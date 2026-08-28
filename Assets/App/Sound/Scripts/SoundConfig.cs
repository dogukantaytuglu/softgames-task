using UnityEngine;

namespace Sound
{
    [CreateAssetMenu(fileName = "SoundConfig", menuName = "Sound/Config")]
    public class SoundConfig : ScriptableObject
    {
        [SerializeField] private AudioClip clip;
        [SerializeField, Range(0f, 1f)] private float volume = 1f;
        [SerializeField, Range(-3f, 3f)] private float pitch = 1f;
        [SerializeField] private bool loop;
        [SerializeField, Range(0, 256)] private int priority = 128;

        public AudioClip Clip => clip;
        public float Volume => volume;
        public float Pitch => pitch;
        public bool Loop => loop;
        public int Priority => priority;
    }
}
