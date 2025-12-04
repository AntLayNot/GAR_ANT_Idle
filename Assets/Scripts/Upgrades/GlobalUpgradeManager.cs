using System.Collections.Generic;
using UnityEngine;

public class GlobalUpgradeManager : MonoBehaviour
{
    public static GlobalUpgradeManager Instance { get; private set; }

    [Header("Liste des upgrades globales")]
    public List<GlobalUpgrade> upgrades = new List<GlobalUpgrade>();

    [Header("Multiplicateurs appliqués au jeu")]
    public double globalProductionMultiplier = 1.0;
    public double offlineGainMultiplier = 1.0;
    public double generatorCostReductionMultiplier = 1.0; // <= 1 (ex: 0.95 = -5%)

    public double synergyPerGenerator = 0.0;      // ex : 0.02 = +2% par générateur débloqué
    public double constellationTierBonus = 0.0;   // ex : 0.05 = +5% par tier de 100
    public double singularityMultiplier = 1.0;    // multiplicateur ultime

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

    private void Start()
    {
        RecomputeMultipliers();
    }

    public void RecomputeMultipliers()
    {
        globalProductionMultiplier = 1.0;
        offlineGainMultiplier = 1.0;
        generatorCostReductionMultiplier = 1.0;

        synergyPerGenerator = 0.0;
        constellationTierBonus = 0.0;
        singularityMultiplier = 1.0;

        foreach (var upg in upgrades)
        {
            if (upg == null || upg.level <= 0) continue;

            double totalValue = upg.GetCurrentValue(); // valuePerLevel * level

            switch (upg.type)
            {
                case GlobalUpgradeType.GlobalProduction:
                    globalProductionMultiplier *= (1.0 + totalValue);
                    break;

                case GlobalUpgradeType.OfflineGain:
                    offlineGainMultiplier *= (1.0 + totalValue);
                    break;

                case GlobalUpgradeType.GeneratorCostReduction:
                    double reduction = totalValue;
                    generatorCostReductionMultiplier *= (1.0 - reduction);
                    break;

                // Generator Synergy : on stocke un pourcentage "par générateur"
                case GlobalUpgradeType.GeneratorSynergy:
                    synergyPerGenerator += totalValue; // ex: +0.02, +0.04, etc.
                    break;

                // Constellation Boost : bonus par tier de 100
                case GlobalUpgradeType.ConstellationBoost:
                    constellationTierBonus += totalValue; // ex : +0.05
                    break;

                // Singularity Core : multiplicateur final
                case GlobalUpgradeType.SingularityCore:
                    singularityMultiplier *= (1.0 + totalValue);
                    break;
            }
        }
    }


    // Sauvegarde des niveaux des upgrades
    public void SaveUpgrades()
    {
        foreach (var upg in upgrades)
        {
            if (upg == null) continue;
            string key = $"ASTRAL_UPG_{upg.id}_LEVEL";
            PlayerPrefs.SetInt(key, upg.level);
        }
    }

    // Chargement des niveaux
    public void LoadUpgrades()
    {
        foreach (var upg in upgrades)
        {
            if (upg == null) continue;
            string key = $"ASTRAL_UPG_{upg.id}_LEVEL";
            if (PlayerPrefs.HasKey(key))
            {
                upg.level = PlayerPrefs.GetInt(key, 0);
            }
        }

        RecomputeMultipliers();
    }
}
