using FpsCounter.Logic;
using TMPro;
using UnityEngine;

namespace FpsCounter.Monobehaviour
{
    public class FpsCountUIController : MonoBehaviour
    {
        [SerializeField] private TMP_Text fpsText;
        [SerializeField] private float updateInterval = 0.5f;

        private FpsCalculator _fpsCalculator;
        private int _lastDisplayedFps = -1;

        private void Awake()
        {
            _fpsCalculator = new FpsCalculator(updateInterval);
        }

        private void Update()
        {
            _fpsCalculator.Sample(Time.unscaledDeltaTime);

            var roundedFps = Mathf.RoundToInt(_fpsCalculator.CurrentFps);
            if (roundedFps == _lastDisplayedFps)
                return;

            _lastDisplayedFps = roundedFps;
            fpsText.text = $"{roundedFps} FPS";
        }
    }
}
