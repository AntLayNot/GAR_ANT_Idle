using UnityEngine;
using UnityEngine.UI;

public class HitMarkUI : MonoBehaviour
{
    [Header("Refs")]
    public Image image;

    [Header("Life")]
    public float lifetime = 0.6f;

    [Header("Anim")]
    public float popScale = 1.15f;
    public float popInSpeed = 18f;
    public float fadeOutStart = 0.15f;

    private float t = 0f;
    private Color baseColor;
    private Vector3 initialScale;

    public void Init(PulsationManager.HitQuality hit, Color color)
    {
        if (image != null)
        {
            image.color = color;
            baseColor = color;
        }
        initialScale = transform.localScale;
        transform.localScale = initialScale * 0.9f;
    }

    private void Update()
    {
        t += Time.unscaledDeltaTime;

        // Pop-in
        float s = Mathf.Lerp(transform.localScale.x, initialScale.x * popScale, Time.unscaledDeltaTime * popInSpeed);
        transform.localScale = new Vector3(s, s, 1f);

        // Fade-out
        if (image != null && t >= fadeOutStart)
        {
            float u = Mathf.InverseLerp(fadeOutStart, lifetime, t);
            Color c = baseColor;
            c.a = Mathf.Lerp(baseColor.a, 0f, u);
            image.color = c;
        }

        if (t >= lifetime)
        {
            Destroy(gameObject);
        }
    }
}
