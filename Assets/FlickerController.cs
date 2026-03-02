using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class FlickerController : MonoBehaviour
{
    [Header("Flicker Settings")]
    [SerializeField] private float flickerDuration = 1f;
    [SerializeField] private float flickerMinInterval = 0.05f;
    [SerializeField] private float flickerMaxInterval = 0.15f;
    [SerializeField] private float flickerMinIntensity = 0f;
    [SerializeField] private float flickerMaxIntensity = 1f;

    private Light2D _light;
    private float _originalIntensity;
    private bool _isFlickering;

    void Awake()
    {
        _light = GetComponent<Light2D>();
        _originalIntensity = _light != null ? _light.intensity : 1f;
    }

    void Update()
    {
        
    }

    /// <summary>
    /// Flickers the Light2D for the specified duration, then restores its original intensity.
    /// </summary>
    public void Flicker()
    {
        if (_isFlickering) return;
        StartCoroutine(FlickerCoroutine());
    }

    private IEnumerator FlickerCoroutine()
    {
        _isFlickering = true;
        float elapsed = 0f;

        while (elapsed < flickerDuration)
        {
            _light.intensity = Random.Range(flickerMinIntensity, flickerMaxIntensity);
            float wait = Random.Range(flickerMinInterval, flickerMaxInterval);
            elapsed += wait;
            yield return new WaitForSeconds(wait);
        }

        _light.intensity = _originalIntensity;
        _isFlickering = false;
    }
}
