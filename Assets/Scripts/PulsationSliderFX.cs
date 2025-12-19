using UnityEngine;
using UnityEngine.UI;

public class PulsationSliderFX : MonoBehaviour
{
    public Image fillImage;

    [Header("Glow Pulse")]
    public float pulseSpeed = 2.5f;
    public float pulseStrength = 0.12f;

    [Header("Color Shift")]
    public Color lowColor = new Color(0.3f, 0.8f, 1f);
    public Color highColor = new Color(0.9f, 0.4f, 1f);

    private RectTransform rt;

    void Awake()
    {
        rt = transform as RectTransform;
    }

    void Update()
    {
        if (PulsationManager.Instance == null || fillImage == null)
            return;

        float charge = PulsationManager.Instance.charge01;

        // Pulse scale (respiration)
        float pulse = 1f + Mathf.Sin(Time.unscaledTime * pulseSpeed) * pulseStrength * charge;
        rt.localScale = new Vector3(1f, pulse, 1f);

        // Color shift selon la charge
        fillImage.color = Color.Lerp(lowColor, highColor, charge);
    }
}
