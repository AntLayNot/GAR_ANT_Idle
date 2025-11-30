using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class GeneratorUI : MonoBehaviour
{
    [Header("Références")]
    public Generator generator;

    [Header("UI Elements")]
    public TMP_Text nameText;
    public TMP_Text levelText;
    public TMP_Text productionText;
    public TMP_Text costText;
    public Button buyButton;

    [Header("Visuel")]
    public Image starImage;
    public Image backgroundImage;
    public StarGlowPulse glowPulse;

    [Header("Lock / Unlock")]
    public CanvasGroup canvasGroup;
    public TMP_Text lockedText;

    [Header("Upgrade Indicator (grosses étoiles)")]
    public List<Image> upgradeStars; // grosses étoiles
    public Color upgradeStarInactiveColor = new Color(0.3f, 0.3f, 0.35f); // gris
    public Color upgradeStarOrangeColor = new Color(1.0f, 0.7f, 0.2f);   // orange
    public Color upgradeStarRedColor = new Color(1.0f, 0.2f, 0.2f);   // rouge
    public Color upgradeStarGoldColor = new Color(1.0f, 0.85f, 0.2f);  // doré (1000+)

    [Header("Upgrade Rank Indicator (petites étoiles)")]
    public List<Image> rankStars;  // petites étoiles en haut à droite
    public Color rankStarInactiveColor = new Color(0.15f, 0.15f, 0.2f);
    public Color rankStarActiveColor = new Color(1.0f, 0.85f, 0.2f); // doré

    [Header("FX Niveau 1000+ (son uniquement)")]
    [Tooltip("Source audio pour les SFX (sinon, laisse vide si tu gères ailleurs).")]
    public AudioSource sfxSource;
    [Tooltip("Son joué à l'instant où on atteint 1000.")]
    public AudioClip goldTierSfx;

    // Pour ne pas rejouer le son en boucle
    private bool hasTriggeredGoldTierFx = false;

    private void Start()
    {
        if (buyButton != null)
            buyButton.onClick.AddListener(OnBuyClicked);

        if (generator != null)
        {
            ApplyVisualFromGenerator();
            Refresh();
        }
    }

    /// <summary>
    /// Appelé par GeneratorListUI juste après l'instanciation du panel.
    /// </summary>
    public void Init(Generator gen)
    {
        generator = gen;
        ApplyVisualFromGenerator();
        Refresh();
    }

    private void Update()
    {
        if (generator == null) return;
        Refresh();
    }

    /// <summary>
    /// Configure tout le visuel d'après les données du Generator :
    /// nom, sprite, couleurs, glow, texte de lock.
    /// </summary>
    private void ApplyVisualFromGenerator()
    {
        if (generator == null) return;

        // Nom
        if (nameText != null)
            nameText.text = generator.displayName;

        // Sprite + couleur de l'étoile
        if (starImage != null)
        {
            if (generator.starSprite != null)
                starImage.sprite = generator.starSprite;

            starImage.color = generator.starColor;
        }

        // Fond légèrement teinté
        if (backgroundImage != null)
        {
            Color c = generator.starColor;
            c.a = 0.25f;
            backgroundImage.color = c;
        }

        // Glow spécifique
        if (glowPulse != null)
        {
            Color glowColor = generator.glowColor;
            if (glowColor.a <= 0f) glowColor.a = 0.4f;

            glowPulse.Setup(glowColor, generator.glowSpeedMultiplier);
        }

        // Texte de lock (basé sur totalStardustEarned)
        if (lockedText != null)
        {
            double req = generator.unlockAtStardust;
            lockedText.text = $"Débloqué à {NumberFormatter.Format(req)}";
        }

        // Reset FX son or
        hasTriggeredGoldTierFx = false;
    }

    public void Refresh()
    {
        if (generator == null) return;

        // Niveau
        if (levelText != null)
            levelText.text = $"Niveau : {generator.level}";

        // Production
        if (productionText != null)
        {
            double prod = generator.GetProductionPerSecond();
            productionText.text = $"Production : {NumberFormatter.Format(prod)} /s";
        }

        // Coût
        if (costText != null)
        {
            double cost = generator.GetNextLevelCost();
            costText.text = $"Coût : {NumberFormatter.Format(cost)}";
        }

        // Étoiles d'upgrade (25 / 100 / cycles / 1000+ doré)
        UpdateUpgradeStars();

        // Effet sonore pour le tier 1000+
        CheckGoldTierEffects();
    }

    private void OnBuyClicked()
    {
        if (generator == null) return;

        if (!generator.TryBuyLevel())
        {
            Debug.Log("Pas assez de poussière d'étoile pour acheter ce niveau.");
        }

        Refresh();
    }

    /// <summary>
    /// Indicateur visuel des paliers :
    /// - 4 grosses étoiles.
    /// - Tous les 25 niveaux : +1 orange (dans la centaine courante).
    /// - Tous les 100 niveaux : +1 rouge (centaines complètes dans la boucle locale).
    /// - Les petites étoiles indiquent les cycles de 4 centaines.
    /// - À 1000+ : la première grosse étoile devient dorée.
    /// </summary>
    private void UpdateUpgradeStars()
    {
        if (upgradeStars == null || upgradeStars.Count == 0 || generator == null)
            return;

        int level = generator.level;
        int starCount = upgradeStars.Count;

        // 🔴 Total de centaines complètes depuis le début (100, 200, 300, ...)
        int totalHundreds = level / 100;

        // On découpe en "cycles" de starCount centaines (par exemple 4)
        int cycles = 0;
        int localHundreds = 0; // 0..starCount pour la boucle actuelle

        if (totalHundreds > 0)
        {
            int t = totalHundreds - 1;
            cycles = t / starCount;             // cycles complets dépassés
            localHundreds = (t % starCount) + 1; // 1..starCount
        }
        else
        {
            cycles = 0;
            localHundreds = 0;
        }

        // Reste dans la centaine actuelle (0..99)
        int remainderInHundred = level % 100;

        // Nombre d'étoiles oranges (1 par palier de 25 dans la centaine en cours, max 3)
        int orangeCount = remainderInHundred / 25; // 0..3

        int maxOrangeSlots = starCount - localHundreds;
        orangeCount = Mathf.Clamp(orangeCount, 0, maxOrangeSlots);

        // --- Grosses étoiles : dorée (1000+), rouges, oranges, grises ---
        for (int i = 0; i < starCount; i++)
        {
            var img = upgradeStars[i];
            if (img == null) continue;

            // 🌟 Première étoile dorée si niveau >= 1000
            if (i == 0 && generator.level >= 1000)
            {
                img.color = upgradeStarGoldColor;
                continue;
            }

            if (i < localHundreds)
            {
                // 🔴 centaines complètes dans la boucle actuelle
                img.color = upgradeStarRedColor;
            }
            else if (i < localHundreds + orangeCount)
            {
                // 🟠 progression par paliers de 25 dans la centaine actuelle
                img.color = upgradeStarOrangeColor;
            }
            else
            {
                // ⚪ pas encore atteinte dans la boucle actuelle
                img.color = upgradeStarInactiveColor;
            }
        }

        // --- Petites étoiles de rang (cycles complets déjà passés) ---
        UpdateRankStars(cycles);
    }

    /// <summary>
    /// Met à jour les petites étoiles de rang (cycles complets de starCount centaines).
    /// </summary>
    private void UpdateRankStars(int cycles)
    {
        if (rankStars == null || rankStars.Count == 0)
            return;

        int activeCount = Mathf.Clamp(cycles, 0, rankStars.Count);

        for (int i = 0; i < rankStars.Count; i++)
        {
            var img = rankStars[i];
            if (img == null) continue;

            img.color = (i < activeCount) ? rankStarActiveColor : rankStarInactiveColor;
        }
    }

    /// <summary>
    /// Gère l'effet sonore "événement 1000+" :
    /// - Son cosmique joué une seule fois quand on passe le palier 1000.
    /// </summary>
    private void CheckGoldTierEffects()
    {
        if (generator == null) return;

        bool isGoldTierReached = generator.level >= 1000;

        if (isGoldTierReached && !hasTriggeredGoldTierFx)
        {
            hasTriggeredGoldTierFx = true;

            // Son cosmique
            if (sfxSource != null && goldTierSfx != null)
            {
                sfxSource.PlayOneShot(goldTierSfx);
            }

            Debug.Log($"[GeneratorUI] Tier doré atteint pour {generator.displayName} (niveau {generator.level}).");
        }
        else if (!isGoldTierReached)
        {
            // Si on descend en-dessous de 1000 (cas rare), on reset
            hasTriggeredGoldTierFx = false;
        }
    }

    public void SetLockedState(bool locked)
    {
        if (buyButton != null)
            buyButton.interactable = !locked;

        if (canvasGroup != null)
            canvasGroup.alpha = locked ? 0.4f : 1f;

        if (lockedText != null)
            lockedText.gameObject.SetActive(locked);
    }
}
