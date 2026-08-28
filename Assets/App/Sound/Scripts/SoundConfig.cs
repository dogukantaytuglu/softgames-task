using UnityEngine;

namespace Sound
{
    [CreateAssetMenu(fileName = "SoundConfig", menuName = "Sound/Config")]
    public class SoundConfig : ScriptableObject
    {
        [SerializeField] private AudioClip clip;
        [SerializeField, MinMaxRange(0f, 1f)] private Vector2 volumeRange = new(1f, 1f);
        [SerializeField, MinMaxRange(-3f, 3f)] private Vector2 pitchRange = new(1f, 1f);
        [SerializeField] private bool loop;
        [SerializeField, MinMaxRange(0, 256)] private Vector2Int priorityRange = new(128, 128);

        public AudioClip Clip => clip;
        public float Volume => Random.Range(volumeRange.x, volumeRange.y);
        public float Pitch => Random.Range(pitchRange.x, pitchRange.y);
        public bool Loop => loop;
        public int Priority => Random.Range(priorityRange.x, priorityRange.y + 1);
    }
}
