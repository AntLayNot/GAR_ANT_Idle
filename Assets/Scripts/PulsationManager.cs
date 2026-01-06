using UnityEngine;

public class PulsationManager : MonoBehaviour
{
    public static PulsationManager Instance { get; private set; }

    [Header("Charge")]
    [Range(0f, 1f)]
    public float charge01 = 0f;

    [Tooltip("Charge de base gagnée par clic (sans bonus).")]
    public float baseChargePerClick = 0.02f;

    [Tooltip("Perte de charge par seconde.")]
    public float decayPerSecond = 0.06f;

    [Header("Multiplicateur (hors Surge)")]
    [Tooltip("Multiplicateur max quand charge=1 (hors Surge). Ex: 2 => x2.")]
    public float maxProductionMultiplier = 2.0f;

    [Header("Flow / Rythme")]
    public bool flowEnabled = true;

    [Tooltip("Intervalle idéal entre clics (en secondes).")]
    public float idealInterval = 0.50f;

    [Tooltip("Fenêtre Perfect (ex: 0.06 => ±60ms).")]
    public float perfectWindow = 0.06f;

    [Tooltip("Fenêtre Good (ex: 0.14 => ±140ms).")]
    public float goodWindow = 0.14f;

    [Tooltip("Combo max affichée (le bonus continue mais l'affichage peut cap).")]
    public int comboDisplayCap = 999;

    [Header("Bonus de combo")]
    [Tooltip("Bonus de charge par combo. Ex 0.03 => +3%/combo sur la charge gagnée.")]
    public float chargeBonusPerCombo = 0.03f;

    [Tooltip("Bonus de multiplicateur par combo (appliqué à la production). Ex 0.01 => +1%/combo.")]
    public float productionBonusPerCombo = 0.01f;

    [Tooltip("Combo perdue si Miss.")]
    public bool resetComboOnMiss = true;

    [Header("Astral Surge (bonus)")]
    [Tooltip("Se déclenche si charge=1 et combo>=minComboForSurge.")]
    public int minComboForSurge = 10;

    [Tooltip("Durée du Surge en secondes.")]
    public float surgeDuration = 6f;

    [Tooltip("Multiplicateur appliqué pendant le Surge (en plus du reste).")]
    public float surgeMultiplier = 3f;

    [Tooltip("Après déclenchement, la charge retombe à ce niveau (0..1).")]
    [Range(0f, 1f)]
    public float chargeAfterSurge = 0.25f;

    // --- runtime ---
    private float lastClickTime = -999f;
    private int combo = 0;

    private bool surgeActive = false;
    private float surgeRemaining = 0f;

    public enum HitQuality { None, Perfect, Good, Miss }

    [Header("Debug / UI")]
    public HitQuality lastHit = HitQuality.None;
    public float lastTimingDiff = 0f;
    public float lastTimingSignedDiff = 0f;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Update()
    {
        if (charge01 > 0f)
        {
            charge01 -= decayPerSecond * Time.unscaledDeltaTime;
            charge01 = Mathf.Clamp01(charge01);
        }

        if (surgeActive)
        {
            surgeRemaining -= Time.unscaledDeltaTime;
            if (surgeRemaining <= 0f)
            {
                surgeActive = false;
                surgeRemaining = 0f;
            }
        }
    }

    public int GetCombo() => combo;
    public bool IsSurgeActive() => surgeActive;
    public float GetSurgeRemaining() => surgeRemaining;

    /// <summary>
    /// Appelé quand le joueur clique sur le bouton Pulsation.
    /// </summary>
    public void RegisterClick()
    {
        float now = Time.unscaledTime;
        float dt = now - lastClickTime;
        lastClickTime = now;

        // Diff au rythme
        float signedDiff = dt - idealInterval;
        float diff = Mathf.Abs(signedDiff);

        lastTimingDiff = diff;
        lastTimingSignedDiff = signedDiff;

        // Qualité du hit
        HitQuality hit;
        if (!flowEnabled)
        {
            hit = HitQuality.Good;
        }
        else
        {
            if (diff <= perfectWindow) hit = HitQuality.Perfect;
            else if (diff <= goodWindow) hit = HitQuality.Good;
            else hit = HitQuality.Miss;
        }

        lastHit = hit;

        // Combo
        if (hit == HitQuality.Perfect) combo += 2;
        else if (hit == HitQuality.Good) combo += 1;
        else if (hit == HitQuality.Miss && resetComboOnMiss) combo = 0;

        if (combo > comboDisplayCap) combo = comboDisplayCap;

        // Gain de charge selon qualité + combo
        float qualityMult = 1f;
        switch (hit)
        {
            case HitQuality.Perfect: qualityMult = 1.25f; break;
            case HitQuality.Good: qualityMult = 1.00f; break;
            case HitQuality.Miss: qualityMult = 0.25f; break;
        }

        float comboChargeBonus = 1f + (combo * chargeBonusPerCombo);
        float gained = baseChargePerClick * qualityMult * comboChargeBonus;

        charge01 = Mathf.Clamp01(charge01 + gained);

        if (!surgeActive && charge01 >= 1f && combo >= minComboForSurge)
        {
            TriggerSurge();
        }
    }

    private void TriggerSurge()
    {
        surgeActive = true;
        surgeRemaining = surgeDuration;

        charge01 = Mathf.Clamp01(chargeAfterSurge);

        Debug.Log($"[Pulsation] ASTRAL SURGE! duration={surgeDuration}s, mult={surgeMultiplier}x");
    }

    /// <summary>
    /// Multiplicateur de production total issu de la pulsation.
    /// </summary>
    public double GetProductionMultiplier()
    {
        float baseMult = Mathf.Lerp(1f, maxProductionMultiplier, charge01);
        float comboMult = 1f + (combo * productionBonusPerCombo);
        float surgeMult = surgeActive ? surgeMultiplier : 1f;

        float final = baseMult * comboMult * surgeMult;
        return (double)final;
    }
}
