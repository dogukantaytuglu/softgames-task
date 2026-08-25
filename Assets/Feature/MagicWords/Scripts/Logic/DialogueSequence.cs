using System;
using System.Collections.Generic;

namespace MagicWords.Logic
{
    public sealed class DialogueSequence
    {
        private readonly List<DialogueLine> _lines;
        private int _index = -1;

        public IReadOnlyList<DialogueLine> Lines => _lines;
        public int Count => _lines.Count;
        public bool HasStarted => _index >= 0;
        public bool IsFinished => HasStarted && _index >= _lines.Count - 1;
        public DialogueLine Current => HasStarted && _index < _lines.Count ? _lines[_index] : null;

        public DialogueSequence(IReadOnlyList<DialogueLine> lines)
        {
            if (lines == null)
                throw new ArgumentNullException(nameof(lines));

            _lines = new List<DialogueLine>(lines);
        }

        public DialogueLine MoveNext()
        {
            if (_index >= _lines.Count - 1)
                throw new InvalidOperationException("The dialogue sequence has no more lines.");

            _index++;
            return _lines[_index];
        }
    }
}
