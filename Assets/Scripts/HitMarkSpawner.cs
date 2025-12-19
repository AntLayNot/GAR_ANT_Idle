using UnityEngine;

public class HitMarkSpawner : MonoBehaviour
{
    [Header("Refs")]
    public PulsationManager pulsation;

    [Tooltip("Le track (la zone horizontale).")]
    public RectTransform track;

    [Tooltip("Prefab du hit mark (UI) à instancier.")]
    public HitMarkUI hitMarkPrefab;

    [Tooltip("Parent où instancier les hit marks (souvent le track).")]
    public RectTransform marksParent;

    [Header("Placement")]
    [Tooltip("Position X du centre (0 = centre du track).")]
    public float centerX = 0f;

    [Tooltip("Échelle de conversion timing->pixels (plus grand = plus écarté).")]
    public float pixelsPerSecond = 300f;

    [Tooltip("Clamp X max (si 0 => clamp sur largeur du track).")]
    public float clampX = 0f;

    [Tooltip("Hauteur aléatoire (optionnel) pour éviter l'empilement parfait.")]
    public float randomY = 6f;

    [Header("Couleurs")]
    public Color perfectColor = new Color(0.95f, 0.95f, 1.0f, 1f);
    public Color goodColor = new Color(0.45f, 0.95f, 1.0f, 1f);
    public Color missColor = new Color(1.0f, 0.25f, 0.35f, 1f);

    private PulsationManager.HitQuality _lastConsumedHit = PulsationManager.HitQuality.None;
    private float _lastHitTime = -999f;

    private void Awake()
    {
        if (pulsation == null) pulsation = PulsationManager.Instance;
        if (marksParent == null) marksParent = track;
    }

    private void Update()
    {
        if (pulsation == null) pulsation = PulsationManager.Instance;
        if (pulsation == null || track == null || hitMarkPrefab == null) return;

        // On consomme un hit "nouveau" une seule fois.
        // (lastHit est mis à jour à chaque clic)
        if (pulsation.lastHit == PulsationManager.HitQuality.None)
            return;

        // Anti double-consommation sur une même frame / même clic
        // -> on compare un timestamp rudimentaire
        float now = Time.unscaledTime;
        if (Mathf.Abs(now - _lastHitTime) < 0.01f && pulsation.lastHit == _lastConsumedHit)
            return;

        SpawnHitMark(pulsation.lastHit, pulsation.lastTimingSignedDiff);

        _lastConsumedHit = pulsation.lastHit;
        _lastHitTime = now;

        // On remet à None pour que l'UI texte (si tu en as) puisse aussi le consommer,
        // mais si tu préfères, tu peux ne PAS reset et gérer via un event.
        pulsation.lastHit = PulsationManager.HitQuality.None;
    }

    private void SpawnHitMark(PulsationManager.HitQuality hit, float signedDiff)
    {
        RectTransform parent = marksParent != null ? marksParent : track;

        HitMarkUI mark = Instantiate(hitMarkPrefab, parent);
        RectTransform rt = mark.transform as RectTransform;

        // Convertir diff (sec) -> offset pixels
        float x = centerX + (signedDiff * pixelsPerSecond);

        // clamp
        float halfW = track.rect.width * 0.5f;
        float maxX = (clampX > 0f) ? clampX : halfW;

        x = Mathf.Clamp(x, -maxX, maxX);

        float y = (randomY > 0f) ? Random.Range(-randomY, randomY) : 0f;
        rt.anchoredPosition = new Vector2(x, y);

        // Couleur selon qualité
        Color c = goodColor;
        switch (hit)
        {
            case PulsationManager.HitQuality.Perfect: c = perfectColor; break;
            case PulsationManager.HitQuality.Good: c = goodColor; break;
            case PulsationManager.HitQuality.Miss: c = missColor; break;
        }

        mark.Init(hit, c);
    }
}
