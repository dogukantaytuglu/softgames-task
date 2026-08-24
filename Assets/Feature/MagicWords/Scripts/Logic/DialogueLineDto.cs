using System;

namespace MagicWords.Logic
{
    // Field names match the endpoint's JSON keys exactly - required for
    // UnityEngine.JsonUtility, which matches by name, not by attribute.
    [Serializable]
    public class DialogueLineDto
    {
        public string name;
        public string text;
    }
}
