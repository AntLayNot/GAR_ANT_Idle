using System;
using UnityEngine;

public enum GlobalUpgradeType
{
    GlobalProduction,      // Multiplie toute la production
    OfflineGain,           // Multiplie les gains offline
    GeneratorCostReduction,// Réduction du coût des générateurs

    GeneratorSynergy,      // +X% de prod par générateur débloqué
    ConstellationBoost,    // +Y% par tier (palier de 100) du générateur
    SingularityCore        // multiplicateur lié au prestige
}

[Serializable]
public class GlobalUpgrade
{
    [Header("Identification")]
    public string id = "UPG_GLOBAL_1";
    public string displayName = "Chant Éternel";
    [TextArea] public string description = "+25% de production globale.";

    [Header("Config")]
    public GlobalUpgradeType type = GlobalUpgradeType.GlobalProduction;

    public double baseCost = 1000;
    public double costMultiplier = 2.0;     // Coût x2 à chaque niveau
    public int maxLevel = 10;
    public double valuePerLevel = 0.25;     // +25% par niveau → x1.25

    [Header("État")]
    public int level = 0;

    public bool IsMaxed => maxLevel > 0 && level >= maxLevel;

    public double GetCurrentValue()
    {
        // Exemple : valuePerLevel = 0.25, level = 2 → +0.5 → x1.5
        return valuePerLevel * level;
    }

    public double GetNextCost()
    {
        return baseCost * Math.Pow(costMultiplier, level);
    }

    public bool TryBuy()
    {
        if (IsMaxed) return false;

        double cost = GetNextCost();
        if (!GameManager.Instance.SpendStardust(cost))
            return false;

        level++;
        // Recalcule les multiplicateurs globaux
        GlobalUpgradeManager.Instance.RecomputeMultipliers();
        return true;
    }
}
