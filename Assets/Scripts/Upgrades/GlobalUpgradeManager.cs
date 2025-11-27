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

        foreach (var upg in upgrades)
        {
            if (upg == null || upg.level <= 0) continue;

            double totalValue = upg.GetCurrentValue(); // ex: 0.5 pour +50%

            switch (upg.type)
            {
                case GlobalUpgradeType.GlobalProduction:
                    globalProductionMultiplier *= (1.0 + totalValue);
                    break;

                case GlobalUpgradeType.OfflineGain:
                    offlineGainMultiplier *= (1.0 + totalValue);
                    break;

                case GlobalUpgradeType.GeneratorCostReduction:
                    // Ici on applique une réduction cumulée (ex: -10% puis -10%)
                    // On convertit le totalValue en réduction : valuePerLevel = 0.05 => -5% par niveau.
                    double reduction = totalValue;
                    generatorCostReductionMultiplier *= (1.0 - reduction);
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
