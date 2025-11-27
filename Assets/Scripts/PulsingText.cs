using UnityEngine;
using TMPro;

public class PulsingText : MonoBehaviour
{
    public TMP_Text text;
    public float speed = 2f;
    public float minScale = 0.95f;
    public float maxScale = 1.05f;

    private void Reset()
    {
        text = GetComponent<TMP_Text>();
    }

    private void Update()
    {
        if (text == null) return;

        float t = (Mathf.Sin(Time.time * speed) + 1f) / 2f; // 0→1
        float scale = Mathf.Lerp(minScale, maxScale, t);
        text.rectTransform.localScale = new Vector3(scale, scale, 1f);
    }
}
