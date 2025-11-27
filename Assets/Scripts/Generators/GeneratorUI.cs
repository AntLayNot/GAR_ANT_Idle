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

    [Header("Upgrade Indicator")]
    public List<Image> upgradeStars;
    public Color upgradeStarInactiveColor = new Color(0.3f, 0.3f, 0.35f); // gris
    public Color upgradeStarOrangeColor = new Color(1.0f, 0.7f, 0.2f);  // orange
    public Color upgradeStarRedColor = new Color(1.0f, 0.2f, 0.2f);  // rouge
    [Header("Gold Tier")]
    public Color upgradeStarGoldColor = new Color(1.0f, 0.85f, 0.2f); // doré

    private void Start()
    {
        if (buyButton != null)
            buyButton.onClick.AddListener(OnBuyClicked);

        // Si le Generator est déjà assigné (via Init), on met à jour le visuel
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

        // Étoiles d'upgrade (paliers 25 / 100 / 1000)
        UpdateUpgradeStars();
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
    /// Met à jour les étoiles d'upgrade :
    /// - weak milestones (25 niveaux)
    /// - 4 weak -> 1 rouge (100 niveaux)
    /// - 40 weak -> 1 doré (1000 niveaux)
    /// Une étoile active seulement pour la progression en cours.
    /// </summary>
    private void UpdateUpgradeStars()
    {
        if (upgradeStars == null || upgradeStars.Count == 0 || generator == null)
            return;

        // Nombre total de paliers de 25 niveaux
        int weakMilestones = generator.GetWeakMilestoneCount();   // level / 25

        // Gold = palier 1000 = 40 weak milestones
        int goldMilestones = weakMilestones / 40;

        // Red = palier 100 = 4 weak milestones
        int remainingAfterGold = weakMilestones % 40;
        int redMilestones = remainingAfterGold / 4;

        // Orange = progression après les rouges
        int remainingAfterRed = remainingAfterGold % 4;
        int orangeMilestones = remainingAfterRed; // 0..3

        for (int i = 0; i < upgradeStars.Count; i++)
        {
            var img = upgradeStars[i];
            if (img == null) continue;

            if (i < goldMilestones)
            {
                // Doré (paliers 1000 complétés)
                img.color = upgradeStarGoldColor;
            }
            else if (i < goldMilestones + redMilestones)
            {
                // Rouge (paliers 100 complétés)
                img.color = upgradeStarRedColor;
            }
            else if (i == goldMilestones + redMilestones && orangeMilestones > 0)
            {
                // Orange (progression 25/50/75 vers le prochain rouge)
                img.color = upgradeStarOrangeColor;
            }
            else
            {
                // Gris
                img.color = upgradeStarInactiveColor;
            }
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
