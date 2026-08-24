using System;
using UnityEngine;

namespace PhoenixFlame.Monobehaviour
{
    // One selectable fire look. LargeFlame02's shader takes color from two
    // separate properties - the base map tint and the (HDR) emission tint -
    // so each option needs both, not just one Color.
    [Serializable]
    public sealed class PhoenixFlameColorOption
    {
        [SerializeField] private string displayName;
        [SerializeField] private Color baseColor = Color.white;
        [ColorUsage(true, true)]
        [SerializeField] private Color emissionColor = Color.white;

        public string DisplayName => displayName;
        public Color BaseColor => baseColor;
        public Color EmissionColor => emissionColor;
    }
}
