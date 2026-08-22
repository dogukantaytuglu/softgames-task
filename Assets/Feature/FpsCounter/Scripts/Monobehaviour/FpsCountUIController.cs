using TMPro;
using UnityEngine;

public class FpsCountUIController : MonoBehaviour
{
    [SerializeField] private TMP_Text fpsText;
    [SerializeField] private float updateInterval = 0.5f;

    private FpsCalculator _fpsCalculator;

    private void Awake()
    {
        _fpsCalculator = new FpsCalculator(updateInterval);
    }

    private void Update()
    {
        _fpsCalculator.Sample(Time.unscaledDeltaTime);
        fpsText.text = $"{_fpsCalculator.CurrentFps:0} FPS";
    }
}