using UnityEngine;
using UnityEngine.UI;

public class RhythmGuideUI : MonoBehaviour
{
    [System.Serializable]
    public class BeatEntry
    {
        [Tooltip("Le cercle/halo du beat (idéalement enfant du Track).")]
        public RectTransform beatPulse;

        [Tooltip("Image du beatPulse (optionnel, pour faire varier l'alpha).")]
        public Image beatImage;

        [Tooltip("Décalage temporel (secondes) pour déphaser ce beat.")]
        public float phaseOffsetSeconds = 0f;

        [Tooltip("Si > 0 : interval custom pour ce beat (sinon on utilise PulsationManager.idealInterval).")]
        public float customInterval = 0f;

        [HideInInspector] public float lastBeatIndex = -1f;
        [HideInInspector] public float pulse01 = 0f;
        [HideInInspector] public Vector3 baseScale = Vector3.one;
        [HideInInspector] public float baseAlpha = 1f;
    }

    [Header("Refs")]
    public PulsationManager pulsation;
    public RectTransform track;

    [Header("Beats (jusqu'à 3)")]
    public BeatEntry[] beats = new BeatEntry[3];

    [Header("Pulse Settings (global)")]
    [Tooltip("Scale ajoutée au beat au moment du beat (0.25 = +25%).")]
    public float beatPulseStrength = 0.25f;

    [Tooltip("Vitesse du retour du pulse.")]
    public float beatPulseReturnSpeed = 12f;

    [Tooltip("Alpha max ajouté au beat au moment du beat.")]
    [Range(0f, 1f)]
    public float beatPulseAlphaBoost = 0.35f;

    [Tooltip("Clamp de scale : le beat ne dépassera pas la hauteur du track.")]
    public bool clampToTrackHeight = true;

    private void Awake()
    {
        if (pulsation == null) pulsation = PulsationManager.Instance;

        // Capture des valeurs de base
        if (beats != null)
        {
            for (int i = 0; i < beats.Length; i++)
            {
                var b = beats[i];
                if (b == null) continue;

                if (b.beatPulse != null)
                    b.baseScale = b.beatPulse.localScale;

                if (b.beatImage != null)
                    b.baseAlpha = b.beatImage.color.a;
            }
        }
    }

    private void Update()
    {
        if (pulsation == null) pulsation = PulsationManager.Instance;
        if (pulsation == null) return;
        if (track == null) return;
        if (beats == null || beats.Length == 0) return;

        float defaultInterval = Mathf.Max(0.05f, pulsation.idealInterval);
        float now = Time.unscaledTime;

        for (int i = 0; i < beats.Length; i++)
        {
            var b = beats[i];
            if (b == null || b.beatPulse == null) continue;

            float interval = (b.customInterval > 0.01f) ? b.customInterval : defaultInterval;
            interval = Mathf.Max(0.05f, interval);

            // temps décalé (phase offset)
            float t = now + b.phaseOffsetSeconds;

            // détecter beat (index)
            float beatIndex = Mathf.Floor(t / interval);
            if (beatIndex != b.lastBeatIndex)
            {
                b.lastBeatIndex = beatIndex;
                b.pulse01 = 1f; // déclenche pulse
            }

            // décroissance pulse
            b.pulse01 = Mathf.MoveTowards(b.pulse01, 0f, Time.unscaledDeltaTime * beatPulseReturnSpeed);

            // appliquer pulse
            ApplyBeatPulse(b);
        }
    }

    private void ApplyBeatPulse(BeatEntry b)
    {
        // scale
        float targetScale = 1f + b.pulse01 * beatPulseStrength;

        float finalScale = targetScale;

        // clamp pour rester dans le track
        if (clampToTrackHeight)
        {
            float trackH = track.rect.height;

            float baseBeatSize = Mathf.Max(b.beatPulse.rect.width, b.beatPulse.rect.height);
            float baseScaledSize = baseBeatSize * b.baseScale.x; // on suppose base scale uniforme (x=y)

            float maxScale = (baseScaledSize > 0f) ? (trackH / baseScaledSize) : 1f;
            finalScale = Mathf.Min(targetScale, maxScale);
        }

        b.beatPulse.localScale = b.baseScale * finalScale;

        // alpha
        if (b.beatImage != null)
        {
            Color c = b.beatImage.color;
            c.a = Mathf.Clamp01(b.baseAlpha + b.pulse01 * beatPulseAlphaBoost);
            b.beatImage.color = c;
        }
    }
}
