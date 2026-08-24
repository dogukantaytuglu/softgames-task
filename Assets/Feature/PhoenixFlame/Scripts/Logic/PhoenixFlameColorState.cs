using System;

namespace PhoenixFlame.Logic
{
    public sealed class PhoenixFlameColorState
    {
        private readonly int _optionCount;

        public int CurrentIndex { get; private set; }

        public PhoenixFlameColorState(int optionCount, int startIndex = 0)
        {
            if (optionCount <= 0)
                throw new ArgumentOutOfRangeException(nameof(optionCount));
            if (startIndex < 0 || startIndex >= optionCount)
                throw new ArgumentOutOfRangeException(nameof(startIndex));

            _optionCount = optionCount;
            CurrentIndex = startIndex;
        }

        // Returns false (no-op) when index is already current - the caller
        // shouldn't retrigger an Animator transition into the state it's already in.
        public bool TrySelect(int index)
        {
            if (index < 0 || index >= _optionCount)
                throw new ArgumentOutOfRangeException(nameof(index));
            if (index == CurrentIndex)
                return false;

            CurrentIndex = index;
            return true;
        }
    }
}
