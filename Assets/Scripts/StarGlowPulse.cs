using UnityEngine;
using UnityEngine.UI;

public class StarGlowPulse : MonoBehaviour
{
    public Image glowImage;

    [Header("Pulse Settings")]
    public float pulseSpeedBase = 1.5f;   // vitesse de base
    public float minAlpha = 0.25f;
    public float maxAlpha = 0.8f;
    public float minScale = 1.0f;
    public float maxScale = 1.2f;

    private float pulseSpeed;   // vitesse finale calculée une fois
    private Color baseColor;
    private RectTransform rt;

    public bool isSetup = false;    // pour éviter les setups répétés

    private void Awake()
    {
        rt = GetComponent<RectTransform>();

        if (glowImage == null)
            glowImage = GetComponent<Image>();

        if (glowImage != null)
            baseColor = glowImage.color;

        pulseSpeed = pulseSpeedBase;
    }

    // Setup appelé UNE SEULE FOIS par GeneratorUI
    public void Setup(Color color, float speedMultiplier)
    {
        if (isSetup) return;   // empêche les répétitions destructrices

        if (glowImage != null)
        {
            baseColor = color;
            glowImage.color = color;
        }

        pulseSpeed = pulseSpeedBase * speedMultiplier;
        isSetup = true;
    }

    private void Update()
    {
        if (!isSetup || glowImage == null) return;

        float t = (Mathf.Sin(Time.time * pulseSpeed) + 1f) * 0.5f;

        // Alpha
        Color c = baseColor;
        c.a = Mathf.Lerp(minAlpha, maxAlpha, t);
        glowImage.color = c;

        // Scale
        float s = Mathf.Lerp(minScale, maxScale, t);
        rt.localScale = new Vector3(s, s, 1f);
    }
}
