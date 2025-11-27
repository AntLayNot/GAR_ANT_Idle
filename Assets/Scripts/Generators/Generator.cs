using UnityEngine;
using System;

[System.Serializable]
public class Generator : MonoBehaviour
{
    [Header("Identification")]
    public string id = "STAR_1";
    public string displayName = "Étoile Naissante";

    [Header("Stats de base")]
    public double baseProductionPerSecond = 1;   // Prod au niveau 1
    public double BaseProdUpgrade;               // Ajoute un certain montant après chaque upgrade (ex - 500/100 = 5%)
    public double baseCost = 10;                 // Coût niveau 1
    public double costMultiplier = 1.15;         // Multiplicateur de coût par niveau

    [Header("Déblocage")]
    public double unlockAtStardust = 0;          // Utilise totalStardustEarned du GameManager

    [Header("Visuel")]
    public Sprite starSprite;
    public Color starColor = Color.white;
    public Color glowColor = Color.white;
    public float glowSpeedMultiplier = 1f;

    [Header("Paliers d'amélioration automatiques")]
    [Tooltip("Tous les X niveaux, un bonus faible est appliqué (25 par défaut).")]
    public int weakMilestoneInterval = 25;
    [Tooltip("Multiplicateur appliqué à chaque palier faible.")]
    public double weakMilestoneMultiplier = 1.7;

    [Tooltip("Tous les X niveaux, un bonus élevé est appliqué (100 par défaut).")]
    public int strongMilestoneInterval = 100;
    [Tooltip("Multiplicateur appliqué à chaque palier fort.")]
    public double strongMilestoneMultiplier = 3.4;

    [Header("État actuel")]
    public int level = 0;

    /// <summary>
    /// Production par seconde actuelle (base * level * bonus paliers)
    /// </summary>
    public double GetProductionPerSecond()
    {
        if (level <= 0) return 0;


        double baseUpgrade = baseProductionPerSecond * BaseProdUpgrade / 100;   // Multiplicateur %
        double baseProd = baseUpgrade * level;
        double milestoneMult = GetMilestoneMultiplier();

        return baseProd * milestoneMult;
    }

    /// <summary>
    /// Calcule le multiplicateur de production en fonction des paliers atteints.
    /// - Tous les weakMilestoneInterval niveaux => bonus faible
    /// - Tous les strongMilestoneInterval niveaux => bonus fort
    /// Les deux se cumulent.
    /// </summary>
    private double GetMilestoneMultiplier()
    {
        if (level <= 0) return 1.0;

        int weakCount = GetWeakMilestoneCount();
        int strongCount = GetStrongMilestoneCount();

        // Interprétation :
        // weakMilestoneMultiplier = 0.7  => +70% par palier de 25
        // strongMilestoneMultiplier = 3.4 => +340% par palier de 100
        double mult = 1.0;

        // bonus faible
        if (weakCount > 0)
            mult *= Math.Pow(1.0 + weakMilestoneMultiplier, weakCount);

        // bonus fort
        if (strongCount > 0)
            mult *= Math.Pow(1.0 + strongMilestoneMultiplier, strongCount);

        return mult;
    }


    public int GetWeakMilestoneCount()
    {
        if (weakMilestoneInterval <= 0) return 0;
        return level / weakMilestoneInterval;
    }

    public int GetStrongMilestoneCount()
    {
        if (strongMilestoneInterval <= 0) return 0;
        return level / strongMilestoneInterval;
    }

    /// <summary>
    /// Niveau global d'amélioration pour l’affichage (basé sur les paliers forts).
    /// </summary>
    public int GetUpgradeTier()
    {
        return GetStrongMilestoneCount(); // 1 tier par palier de 100 niveaux
    }


    /// <summary>
    /// Coût pour acheter 1 niveau de plus.
    /// </summary>
    public double GetNextLevelCost()
    {
        double cost = baseCost * Math.Pow(costMultiplier, level);

        if (GlobalUpgradeManager.Instance != null)
        {
            double reductionMult = GlobalUpgradeManager.Instance.generatorCostReductionMultiplier;
            cost *= reductionMult; // ex: 0.9 => -10%
        }

        return cost;
    }


    /// <summary>
    /// Tente d'acheter un niveau. Retourne true si réussite.
    /// </summary>
    public bool TryBuyLevel()
    {
        double cost = GetNextLevelCost();

        if (GameManager.Instance.SpendStardust(cost))
        {
            level++;
            return true;
        }

        return false;
    }

    /// <summary>
    /// Déblocage basé sur le total de poussière gagnée (totalStardustEarned).
    /// </summary>
    public bool IsUnlocked()
    {
        if (level > 0) return true;
        if (GameManager.Instance == null) return false;

        return GameManager.Instance.totalStardustEarned >= unlockAtStardust;
    }
}
