using System;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Currency")]
    public double stardust = 0;              // Poussière d'étoile actuelle
    public double totalStardustEarned = 0;   // Total de poussière gagnée depuis le début

    [Header("Production (debug / UI)")]
    public double totalPerSecond = 0;        // Production totale effective (avec tous les multiplicateurs)

    [Header("All Generators in the Scene")]
    public List<Generator> generators = new List<Generator>();

    // Keys de sauvegarde
    private const string StardustKey = "ASTRAL_STARDUST";
    private const string TotalStardustEarnedKey = "ASTRAL_TOTAL_STARDUST";
    private const string LastSaveKey = "ASTRAL_LAST_SAVE_TIME";

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
        LoadGame();
    }

    private void Update()
    {
        // On calcule la production totale /s avec tous les multiplicateurs
        double effectivePerSecond = ComputeTotalProductionPerSecond();

        // Stocké pour l'UI
        totalPerSecond = effectivePerSecond;

        // Gain pour cette frame
        double deltaGain = effectivePerSecond * Time.deltaTime;
        if (deltaGain > 0)
        {
            AddStardust(deltaGain);
        }
    }

    private void OnApplicationQuit()
    {
        SaveGame();
    }

    /// <summary>
    /// Ajoute de la poussière d'étoile, met à jour le total gagné.
    /// </summary>
    public void AddStardust(double amount)
    {
        if (amount <= 0) return; // on ne compte que le gain positif dans le total

        stardust += amount;
        totalStardustEarned += amount;

        if (stardust < 0) stardust = 0;
    }

    /// <summary>
    /// Tente de dépenser de la poussière d'étoile.
    /// </summary>
    public bool SpendStardust(double amount)
    {
        if (stardust >= amount)
        {
            stardust -= amount;
            return true;
        }

        return false;
    }

    /// <summary>
    /// Sauvegarde la monnaie, les niveaux de générateurs et la date de sauvegarde.
    /// </summary>
    public void SaveGame()
    {
        // Sauvegarde de la monnaie actuelle
        PlayerPrefs.SetString(StardustKey, stardust.ToString("R"));
        // Sauvegarde du total gagné
        PlayerPrefs.SetString(TotalStardustEarnedKey, totalStardustEarned.ToString("R"));

        // Sauvegarde du niveau de chaque générateur
        foreach (var gen in generators)
        {
            if (gen == null) continue;
            string levelKey = $"ASTRAL_GEN_{gen.id}_LEVEL";
            PlayerPrefs.SetInt(levelKey, gen.level);
        }

        // Sauvegarde du timestamp pour l'offline
        long ticks = DateTime.UtcNow.Ticks;
        PlayerPrefs.SetString(LastSaveKey, ticks.ToString());

        PlayerPrefs.Save();

        // Sauvegarde des upgrades globales
        if (GlobalUpgradeManager.Instance != null)
        {
            GlobalUpgradeManager.Instance.SaveUpgrades();
        }

        Debug.Log("Game Saved.");
    }

<<<<<<< Updated upstream
    /// <summary>
    /// Reset complet de la progression (monnaie, niveaux, upgrades, PlayerPrefs).
    /// NE reset pas la structure du GameManager (DontDestroyOnLoad).
    /// </summary>
=======
>>>>>>> Stashed changes
    public void ResetGame()
    {
        stardust = 0;
        totalStardustEarned = 0;
<<<<<<< Updated upstream
        totalPerSecond = 0;

        // Reset niveaux des générateurs en mémoire + suppression des clés
        foreach (var gen in generators)
        {
            if (gen == null) continue;

=======

        foreach (var gen in generators)
        {
            if (gen == null) continue;
>>>>>>> Stashed changes
            gen.level = 0;

            string levelKey = $"ASTRAL_GEN_{gen.id}_LEVEL";
            PlayerPrefs.DeleteKey(levelKey);
        }

<<<<<<< Updated upstream
        // Reset des upgrades globales
=======
>>>>>>> Stashed changes
        if (GlobalUpgradeManager.Instance != null)
        {
            foreach (var upg in GlobalUpgradeManager.Instance.upgrades)
            {
                if (upg == null) continue;
                upg.level = 0;

                string upgradeKey = $"ASTRAL_UPG_{upg.id}_LEVEL";
                PlayerPrefs.DeleteKey(upgradeKey);
            }

            GlobalUpgradeManager.Instance.RecomputeMultipliers();
        }

<<<<<<< Updated upstream
        // Reset des autres clés de currency / temps
=======
>>>>>>> Stashed changes
        PlayerPrefs.DeleteKey(StardustKey);
        PlayerPrefs.DeleteKey(TotalStardustEarnedKey);
        PlayerPrefs.DeleteKey(LastSaveKey);
        PlayerPrefs.Save();
<<<<<<< Updated upstream

        Debug.Log("[GameManager] Reset du jeu effectué.");
    }

    /// <summary>
    /// Chargement de la progression (monnaie, générateurs, offline, upgrades globales).
    /// </summary>
=======
    }



>>>>>>> Stashed changes
    private void LoadGame()
    {
        // Chargement de la currency actuelle
        if (PlayerPrefs.HasKey(StardustKey))
        {
            string value = PlayerPrefs.GetString(StardustKey, "0");
            if (double.TryParse(value, out double parsed))
            {
                stardust = parsed;
            }
        }

        // Chargement du total gagné
        if (PlayerPrefs.HasKey(TotalStardustEarnedKey))
        {
            string value = PlayerPrefs.GetString(TotalStardustEarnedKey, "0");
            if (double.TryParse(value, out double parsed))
            {
                totalStardustEarned = parsed;
            }
        }

        // Chargement des niveaux des générateurs
        foreach (var gen in generators)
        {
            if (gen == null) continue;

            string levelKey = $"ASTRAL_GEN_{gen.id}_LEVEL";
            if (PlayerPrefs.HasKey(levelKey))
            {
                gen.level = PlayerPrefs.GetInt(levelKey, 0);
            }
            else
            {
<<<<<<< Updated upstream
                // Si aucune sauvegarde n'existe pour ce générateur, on force à 0
                gen.level = 0;
            }
        }

        // Chargement des upgrades globales AVANT de calculer l'offline
        if (GlobalUpgradeManager.Instance != null)
        {
            GlobalUpgradeManager.Instance.LoadUpgrades();
=======
                // s'il n'y a PAS de sauvegarde pour ce générateur, niveau 0.
                gen.level = 0;
            }
>>>>>>> Stashed changes
        }




        // Offline progress
        if (PlayerPrefs.HasKey(LastSaveKey))
        {
            string ticksStr = PlayerPrefs.GetString(LastSaveKey, "0");
            if (long.TryParse(ticksStr, out long ticks))
            {
                DateTime lastSave = new DateTime(ticks, DateTimeKind.Utc);
                TimeSpan delta = DateTime.UtcNow - lastSave;

                double secondsAway = Math.Max(0, delta.TotalSeconds);

                // Production totale effective /s (avec milestones + upgrades globales)
                double effectivePerSecond = ComputeTotalProductionPerSecond();

                double offlineGain = effectivePerSecond * secondsAway;

                // Applique le multiplicateur d'upgrade offline
                if (GlobalUpgradeManager.Instance != null)
                {
                    double offlineMult = GlobalUpgradeManager.Instance.offlineGainMultiplier;
                    offlineGain *= offlineMult;
                }

                if (offlineGain > 0)
                {
                    AddStardust(offlineGain);
                }

                Debug.Log($"Offline gain: +{offlineGain} stardust (away {secondsAway} s)");
            }
        }
    }

    /// <summary>
    /// Calcule la production totale /s de tous les générateurs,
    /// en incluant :
    /// - prod de base + milestones (dans Generator.GetProductionPerSecond())
    /// - bonus de constellation (par tier de 100)
    /// - synergie par générateur débloqué
    /// - multiplicateur global
    /// - multiplicateur Singularity (endgame)
    /// </summary>
    public double ComputeTotalProductionPerSecond()
    {
        double sumPerSecond = 0.0;
        int unlockedCount = 0;

        foreach (var gen in generators)
        {
            if (gen == null) continue;

            if (gen.IsUnlocked())
                unlockedCount++;

            // Production de base du générateur (incluant milestones)
            double prod = gen.GetProductionPerSecond();

            // Bonus de constellation : +X% par tier de 100 niveaux
            if (GlobalUpgradeManager.Instance != null)
            {
                double constBonus = GlobalUpgradeManager.Instance.constellationTierBonus;
                if (constBonus > 0)
                {
                    int tier = gen.GetUpgradeTier(); // ex : 1 pour 100+, 2 pour 200+, etc.
                    if (tier > 0)
                    {
                        prod *= (1.0 + constBonus * tier);
                    }
                }
            }

            sumPerSecond += prod;
        }

        // Multiplicateurs globaux
        double globalMult = 1.0;
        double synergyMult = 1.0;
        double singularityMult = 1.0;

        if (GlobalUpgradeManager.Instance != null)
        {
            globalMult = GlobalUpgradeManager.Instance.globalProductionMultiplier;

            // Synergie : +X% par générateur débloqué
            double perGen = GlobalUpgradeManager.Instance.synergyPerGenerator;
            if (perGen > 0 && unlockedCount > 0)
            {
                synergyMult += perGen * unlockedCount;
            }

            // Singularity Core (endgame)
            singularityMult = GlobalUpgradeManager.Instance.singularityMultiplier;
        }

        double effectivePerSecond = sumPerSecond * globalMult * synergyMult * singularityMult;
        return effectivePerSecond;
    }
}
