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

    [Tooltip("Tous les X niveaux, un bonus ultime est appliqué (1000 par défaut).")]
    public int megaMilestoneInterval = 1000;

    [Tooltip("Multiplicateur appliqué à chaque palier ultime (1000+).")]
    public double megaMilestoneMultiplier = 10.0;


    [Header("État actuel")]
    public int level = 0;

    /// <summary>
    /// Production par seconde actuelle (base * level * bonus paliers)
    /// </summary>
    public double GetProductionPerSecond()
    {
        if (level <= 0) return 0.0;

        // Prod linéaire de base
        double baseProd = baseProductionPerSecond * level;

<<<<<<< Updated upstream
        // Multiplicateurs de milestones (25 / 100 / 1000, etc.)
        double milestoneMult = GetMilestoneMultiplier();

        // Prod finale du générateur
        double finalProd = baseProd * milestoneMult;

        return finalProd;
=======
        double perLevelProduction = baseProductionPerSecond * (1.0 + BaseProdUpgrade / 100.0);
        double linearProduction = perLevelProduction * level;

        // Les paliers restent intéressants mais on atténue l'exponentielle avec un logarithme.
        double milestoneMult = GetMilestoneMultiplier();
        double dampenedMilestone = 1.0 + Math.Log10(1.0 + milestoneMult);

        return linearProduction * dampenedMilestone;
>>>>>>> Stashed changes
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

        int weakCount = GetWeakMilestoneCount();   // tous les 25
        int strongCount = GetStrongMilestoneCount(); // tous les 100
        int megaCount = GetMegaMilestoneCount();   // tous les 1000

        double mult = 1.0;

        // bonus faible (25)
        if (weakCount > 0)
            mult *= Math.Pow(1.0 + weakMilestoneMultiplier, weakCount);

        // bonus fort (100)
        if (strongCount > 0)
            mult *= Math.Pow(1.0 + strongMilestoneMultiplier, strongCount);

        // bonus méga (1000)
        if (megaCount > 0)
            mult *= Math.Pow(1.0 + megaMilestoneMultiplier, megaCount);

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

    public int GetMegaMilestoneCount()
    {
        if (megaMilestoneInterval <= 0) return 0;
        return level / megaMilestoneInterval;
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
        // Formule de base (adapte à ta version)
        double cost = baseCost * Math.Pow(costMultiplier, level);

        if (GlobalUpgradeManager.Instance != null)
        {
            double reductionMult = GlobalUpgradeManager.Instance.generatorCostReductionMultiplier;
            cost *= reductionMult;
        }

        // 🔒 Sécurité anti-overflow
        if (double.IsNaN(cost) || double.IsInfinity(cost))
        {
            // On considère que c'est trop cher / endgame
            return double.MaxValue / 2.0;
        }

        return cost;
    }


    /// <summary>
    /// Coût total pour acheter "count" niveaux de plus à partir du niveau actuel.
    /// </summary>
    public double GetCostForLevels(int count)
    {
        if (count <= 0) return 0;

        double total = 0;
        int tempLevel = level;

        for (int i = 0; i < count; i++)
        {
            double cost = baseCost * Math.Pow(costMultiplier, tempLevel);

            if (GlobalUpgradeManager.Instance != null)
            {
                double reductionMult = GlobalUpgradeManager.Instance.generatorCostReductionMultiplier;
                cost *= reductionMult;
            }

            // Si ce palier part déjà en NaN/Inf → on arrête ici
            if (double.IsNaN(cost) || double.IsInfinity(cost))
            {
                total = double.PositiveInfinity;
                break;
            }

            total += cost;
            tempLevel++;

            // Et on coupe court si la somme devient trop grosse
            if (double.IsInfinity(total))
            {
                total = double.PositiveInfinity;
                break;
            }
        }

        return total;
    }


    /// <summary>
    /// Combien de niveaux on peut acheter au maximum avec une certaine quantité de poussière ?
    /// </summary>
    public int GetMaxAffordableLevels(double availableStardust)
    {
        int tempLevel = level;
        int bought = 0;
        double remaining = availableStardust;

        while (true)
        {
            double cost = baseCost * Math.Pow(costMultiplier, tempLevel);

            if (GlobalUpgradeManager.Instance != null)
            {
                double reductionMult = GlobalUpgradeManager.Instance.generatorCostReductionMultiplier;
                cost *= reductionMult;
            }

            if (remaining < cost)
                break;

            remaining -= cost;
            tempLevel++;
            bought++;

            // Sécurité anti-boucle infinie
            if (bought > 1_000_000)
                break;
        }

        return bought;
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

    public int TryBuyLevels(int count)
    {
        int bought = 0;

        // On essaie d'acheter "count" niveaux maximum,
        // mais on s'arrête si on n'a plus assez de stardust.
        for (int i = 0; i < count; i++)
        {
            if (!TryBuyLevel()) // utilise ta logique existante (coût, stardust, etc.)
                break;

            bought++;
        }

        return bought; // combien de niveaux ont vraiment été achetés
    }

    public int TryBuyMaxLevels()
    {
        int bought = 0;

        while (true)
        {
            double cost = GetNextLevelCost();
            if (GameManager.Instance.stardust < cost)
                break;

            // Achat du niveau
            if (!TryBuyLevel())
                break;

            bought++;

            // Sécurité : évite les boucles infinies (en cas de bug)
            if (bought > 1000000)
                break;
        }

        return bought;
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
