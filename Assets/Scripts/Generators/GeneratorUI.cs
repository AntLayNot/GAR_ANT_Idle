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
    public Button buyButton;    // <-- bouton principal "Acheter"

    [Header("Buy Mode Dropdown")]
    public TMP_Dropdown buyModeDropdown;

    private int currentBuyAmount = 1;

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
    public Color upgradeStarRedColor = new Color(1.0f, 0.2f, 0.2f);     // rouge

    [Header("Tier Colors (par 1000 niveaux)")]
    public Color tierYellowColor = new Color(1.0f, 0.85f, 0.2f);     // 1000–4000
    public Color tierPurpleColor = new Color(0.8f, 0.4f, 1.0f);     // 4000–8000
    public Color tierBlueColor = new Color(0.3f, 0.6f, 1.0f);      // 8000–12000
    public Color tierGreenColor = new Color(0.4f, 1.0f, 0.6f);    // 12000–16000
    public Color tierPinkColor = new Color(1.0f, 0.4f, 0.8f);    // 16000+

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
        // Bouton "Acheter"
        if (buyButton != null)
            buyButton.onClick.AddListener(OnBuyClicked);

        // Dropdown x1 / x10 / x25 / x100 / xMax
        if (buyModeDropdown != null)
        {
            buyModeDropdown.onValueChanged.AddListener(OnBuyModeChanged);
            OnBuyModeChanged(buyModeDropdown.value); // initialise le mode
        }

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

        // Production (incluant le multiplicateur global)
        if (productionText != null)
        {
            // Prod locale (base + milestones)
            double baseProd = generator.GetProductionPerSecond();

            // Multiplicateur global (upgrades)
            double globalMult = 1.0;
            if (GlobalUpgradeManager.Instance != null)
                globalMult = GlobalUpgradeManager.Instance.globalProductionMultiplier;

            // Prod effective réelle
            double effectiveProd = baseProd * globalMult;

            productionText.text = $"Production : {NumberFormatter.Format(effectiveProd)} /s";
        }


        // Coût (en fonction du mode d'achat)
        if (costText != null)
        {
            if (currentBuyAmount == -1)
            {
                // Mode xMax : on calcule combien de niveaux on peut acheter
                double available = GameManager.Instance != null ? GameManager.Instance.stardust : 0.0;
                int maxLevels = generator.GetMaxAffordableLevels(available);

                if (maxLevels > 0)
                {
                    double totalCost = generator.GetCostForLevels(maxLevels);
                    costText.text = $"Coût (Max x{maxLevels}) : {NumberFormatter.Format(totalCost)}";
                }
                else
                {
                    // Rien d'achetable
                    costText.text = "Coût (Max) : 0";
                }
            }
            else
            {
                // Mode x1 / x10 / x25 / x100
                int packSize = Mathf.Max(1, currentBuyAmount);
                double totalCost = generator.GetCostForLevels(packSize);

                if (packSize == 1)
                {
                    costText.text = $"Coût : {NumberFormatter.Format(totalCost)}";
                }
                else
                {
                    costText.text = $"Coût (x{packSize}) : {NumberFormatter.Format(totalCost)}";
                }
            }
        }


        // Étoiles d'upgrade (25 / 100 / cycles)
        UpdateUpgradeStars();

        // Effet sonore pour le tier 1000+
        CheckGoldTierEffects();
    }

    private void OnBuyClicked()
    {
        if (generator == null) return;

        int bought = 0;

        if (currentBuyAmount == -1)
        {
            // Mode xMax
            bought = generator.TryBuyMaxLevels();
        }
        else
        {
            bought = generator.TryBuyLevels(currentBuyAmount);
        }

        if (bought <= 0)
        {
            Debug.Log("Pas assez de poussière d'étoile pour acheter ces niveaux.");
        }

        Refresh();
    }

    private void OnBuyModeChanged(int index)
    {
        switch (index)
        {
            case 0: // x1
                currentBuyAmount = 1;
                break;

            case 1: // x10
                currentBuyAmount = 10;
                break;

            case 2: // x25
                currentBuyAmount = 25;
                break;

            case 3: // x100
                currentBuyAmount = 100;
                break;

            case 4: // xMax
                currentBuyAmount = -1; // -1 = mode MAX
                break;

            default:
                currentBuyAmount = 1;
                break;
        }

        Debug.Log("Buy mode = " + currentBuyAmount);

        Refresh();
    }

    /// <summary>
    /// Indicateur visuel des paliers locaux :
    /// - 4 grosses étoiles.
    /// - Tous les 25 niveaux : +1 orange (dans la centaine courante).
    /// - Tous les 100 niveaux : +1 rouge (centaines complètes dans la boucle locale).
    /// - Les petites étoiles indiquent les cycles de 4 centaines.
    /// - Ensuite, une surcouche par 1000 niveaux vient recolorer les premières étoiles.
    /// </summary>
    private void UpdateUpgradeStars()
    {
        if (upgradeStars == null || upgradeStars.Count == 0 || generator == null)
            return;

        int level = generator.level;
        int starCount = upgradeStars.Count;

        // ---------- LOGIQUE LOCALE (25 / 100) ----------

        // Total de centaines complètes depuis le début (100, 200, 300, ...)
        int totalHundreds = level / 100;

        // On découpe en "cycles" de starCount centaines (par exemple 4)
        int cycles = 0;
        int localHundreds = 0;

        if (totalHundreds > 0)
        {
            int t = totalHundreds - 1;
            cycles = t / starCount;               // cycles complets dépassés
            localHundreds = (t % starCount) + 1;
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

        // --- Grosses étoiles : rouge / orange / gris (base) ---
        for (int i = 0; i < starCount; i++)
        {
            var img = upgradeStars[i];
            if (img == null) continue;

            if (i < localHundreds)
            {
                // centaines complètes dans la boucle actuelle
                img.color = upgradeStarRedColor;
            }
            else if (i < localHundreds + orangeCount)
            {
                // progression par paliers de 25 dans la centaine actuelle
                img.color = upgradeStarOrangeColor;
            }
            else
            {
                // pas encore atteinte dans la boucle actuelle
                img.color = upgradeStarInactiveColor;
            }
        }

        // --- Petites étoiles de rang (cycles complets déjà passés) ---
        UpdateRankStars(cycles);

        // ---------- SURCOUCHE PAR 1000 NIVEAUX ----------
        ApplyThousandTierColors(level);
    }

    /// <summary>
    /// Recolore les premières étoiles en fonction des paliers de 1000 :
    /// - 1000–4000 : les étoiles se complètent en jaune (1 par 1000).
    /// - 4000–8000 : même principe en violet.
    /// - 8000–12000 : même principe en bleu.
    /// - 12000–16000 : même principe en vert.
    /// - 16000+ : même principe en rose.
    /// </summary>
    private void ApplyThousandTierColors(int level)
    {
        if (upgradeStars == null || upgradeStars.Count == 0)
            return;

        int starCount = upgradeStars.Count;

        // Combien de milliers complets ?
        int tierIndex = level / 1000; // 0,1,2,3,...

        if (tierIndex <= 0)
            return; // en-dessous de 1000, pas de surcouche

        // Chaque phase utilise starCount paliers (1000 * starCount niveaux)
        // Ex : avec 4 étoiles => 4 * 1000 = 4000 niveaux par phase
        int phaseSize = starCount;

        // Phase : 0 = jaune, 1 = violet, 2 = bleu, 3 = vert, 4+ = rose
        int phaseIndex = (tierIndex - 1) / phaseSize;

        // Nombre d'étoiles remplies dans cette phase (1..4)
        int fillCount = (tierIndex - 1) % phaseSize + 1;
        fillCount = Mathf.Clamp(fillCount, 1, starCount);

        // Sélection de la couleur de phase
        Color phaseColor;
        switch (phaseIndex)
        {
            case 0:
                phaseColor = tierYellowColor;   // 1000–4000
                break;
            case 1:
                phaseColor = tierPurpleColor;   // 4000–8000
                break;
            case 2:
                phaseColor = tierBlueColor;     // 8000–12000
                break;
            case 3:
                phaseColor = tierGreenColor;    // 12000–16000
                break;
            default:
                phaseColor = tierPinkColor;     // 16000+
                break;
        }

        // On recolore les premières étoiles avec la couleur de phase
        for (int i = 0; i < starCount; i++)
        {
            var img = upgradeStars[i];
            if (img == null) continue;

            if (i < fillCount)
                img.color = phaseColor;
        }
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

            Debug.Log($"[GeneratorUI] Tier de milliers atteint pour {generator.displayName} (niveau {generator.level}).");
        }
        else if (!isGoldTierReached)
        {
            // Si on descend en-dessous de 1000 (cas rare), on reset
            hasTriggeredGoldTierFx = false;
        }
    }

    public void SetLockedState(bool locked)
    {
        // Désactive le bouton d'achat principal
        if (buyButton != null)
            buyButton.interactable = !locked;

        // Désactive le dropdown (x1 / x10 / x25 / x100 / max)
        if (buyModeDropdown != null)
            buyModeDropdown.interactable = !locked;

        // Rend visuellement le panel grisé / normal
        if (canvasGroup != null)
            canvasGroup.alpha = locked ? 0.4f : 1f;

        // Affiche ou masque le texte "Débloqué à XXX"
        if (lockedText != null)
            lockedText.gameObject.SetActive(locked);
    }
} 